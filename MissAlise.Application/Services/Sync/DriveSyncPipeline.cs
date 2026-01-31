using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.Media;

public sealed class DriveSyncPipeline : IAsyncDisposable
{
    private readonly IOneDriveService _graph;
    private readonly CancellationToken cancel;

    private readonly BufferBlock<MediaItem> _ingress;
    private readonly BroadcastBlock<MediaItem> _router;

    private readonly TransformManyBlock<MediaItem, MediaItem> _photoFilter;
    private readonly TransformManyBlock<MediaItem, MediaItem> _videoFilter;

    private readonly ActionBlock<MediaItem> _photoProcessor;
    private readonly ActionBlock<MediaItem> _videoProcessor;

    private bool _completed;

    public DriveSyncPipeline(IOneDriveService graph, string photoRoot, string videoRoot, CancellationToken ct, int parallelism = 4)
    {
        _graph = graph;
        cancel = ct;

        var filterOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = 100,
            EnsureOrdered = false,
            CancellationToken = ct
        };

        var processorOptions = new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = parallelism,
            BoundedCapacity = 20,
            EnsureOrdered = false,
            CancellationToken = ct
        };

        _ingress = new BufferBlock<MediaItem>(new DataflowBlockOptions
        {
            BoundedCapacity = 100,
            CancellationToken = ct
        });

        _router = new BroadcastBlock<MediaItem>(i => i);

        _photoFilter = new TransformManyBlock<MediaItem, MediaItem>(
            i => IsPhoto(i) ? new[] { i } : Array.Empty<MediaItem>(),
            filterOptions);

        _videoFilter = new TransformManyBlock<MediaItem, MediaItem>(
            i => IsVideo(i) ? new[] { i } : Array.Empty<MediaItem>(),
            filterOptions);

        _photoProcessor = new ActionBlock<MediaItem>(
            i => SafeProcessAsync(i, photoRoot),
            processorOptions);

        _videoProcessor = new ActionBlock<MediaItem>(
            i => SafeProcessAsync(i, videoRoot),
            processorOptions);

        var link = new DataflowLinkOptions { PropagateCompletion = true };

        _ingress.LinkTo(_router, link);
        _router.LinkTo(_photoFilter, link);
        _router.LinkTo(_videoFilter, link);
        _photoFilter.LinkTo(_photoProcessor, link);
        _videoFilter.LinkTo(_videoProcessor, link);
    }

    public Task PostAsync(MediaItem item, CancellationToken ct)
    {
        if (_completed)
            throw new InvalidOperationException("Pipeline already completed");

        return _ingress.SendAsync(item, ct);
    }

    public Task Completion =>
        Task.WhenAll(
            _photoProcessor.Completion,
            _videoProcessor.Completion);

    public async Task CompleteAsync()
    {
        if (_completed)
            return;

        _completed = true;
        _ingress.Complete();

        await Completion;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_completed)
            await CompleteAsync();
    }

    private async Task SafeProcessAsync(MediaItem item, string root)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            cancel.ThrowIfCancellationRequested();

            try
            {
                await ProcessMediaAsync(item, root);
                return;
            }
            catch when (attempt < 3)
            {
                await Task.Delay(TimeSpan.FromSeconds(attempt), cancel);
            }
        }
    }

    private async Task ProcessMediaAsync(MediaItem item, string root)
    {
        var date = GetItemDate(item);

        var dir = Path.Combine(
            root,
            date.Year.ToString(),
            date.Month.ToString("D2"));

        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, item.Name);

        try
        {
            using var remoteStream =
                await _graph.GetItemContent("item.Parent.DriveId", item.Id, cancel);

            using var fs = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await remoteStream.CopyToAsync(fs, cancel);

            fs.Position = 0;

            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(fs));

            if (item.Hash != null &&
                !hash.Equals(item.Hash.Value, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(path);
                throw new InvalidDataException("SHA256 hash mismatch");
            }
        }
        catch (IOException)
        {
            // файл уже существует — считаем обработанным
        }
    }

    private static bool IsPhoto(MediaItem i) =>
        i is Photo;

    private static bool IsVideo(MediaItem i) =>
        i is Video;

    private static DateTime GetItemDate(MediaItem item) =>
        item.Timestamps?.TakenAt?.DateTime
        ?? item.Timestamps?.ModifiedAt.DateTime
        ?? DateTime.UtcNow;
}