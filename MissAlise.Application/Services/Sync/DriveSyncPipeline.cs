using System.Security.Cryptography;
using System.Threading.Tasks.Dataflow;
using MissAlise.Application.Interfaces;
using MissAlise.Entities.OneDrive;

public sealed class DriveSyncPipeline : IAsyncDisposable
{
    private readonly IOneDriveService _graph;
    private readonly CancellationToken cancel;

    private readonly BufferBlock<ItemInfo> _ingress;
    private readonly BroadcastBlock<ItemInfo> _router;

    private readonly TransformManyBlock<ItemInfo, ItemInfo> _photoFilter;
    private readonly TransformManyBlock<ItemInfo, ItemInfo> _videoFilter;

    private readonly ActionBlock<ItemInfo> _photoProcessor;
    private readonly ActionBlock<ItemInfo> _videoProcessor;

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

        _ingress = new BufferBlock<ItemInfo>(new DataflowBlockOptions
        {
            BoundedCapacity = 100,
            CancellationToken = ct
        });

        _router = new BroadcastBlock<ItemInfo>(i => i);

        _photoFilter = new TransformManyBlock<ItemInfo, ItemInfo>(
            i => IsPhoto(i) ? new[] { i } : Array.Empty<ItemInfo>(),
            filterOptions);

        _videoFilter = new TransformManyBlock<ItemInfo, ItemInfo>(
            i => IsVideo(i) ? new[] { i } : Array.Empty<ItemInfo>(),
            filterOptions);

        _photoProcessor = new ActionBlock<ItemInfo>(
            i => SafeProcessAsync(i, photoRoot),
            processorOptions);

        _videoProcessor = new ActionBlock<ItemInfo>(
            i => SafeProcessAsync(i, videoRoot),
            processorOptions);

        var link = new DataflowLinkOptions { PropagateCompletion = true };

        _ingress.LinkTo(_router, link);
        _router.LinkTo(_photoFilter, link);
        _router.LinkTo(_videoFilter, link);
        _photoFilter.LinkTo(_photoProcessor, link);
        _videoFilter.LinkTo(_videoProcessor, link);
    }

    public Task PostAsync(ItemInfo item, CancellationToken ct)
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

    private async Task SafeProcessAsync(ItemInfo item, string root)
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

    private async Task ProcessMediaAsync(ItemInfo item, string root)
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
                await _graph.GetItemContent(item.Parent.DriveId, item.Id, cancel);

            using var fs = new FileStream(
                path,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None);

            await remoteStream.CopyToAsync(fs, cancel);

            fs.Position = 0;

            using var sha = SHA256.Create();
            var hash = Convert.ToHexString(sha.ComputeHash(fs));

            if (item.Sha256Hash != null &&
                !hash.Equals(item.Sha256Hash, StringComparison.OrdinalIgnoreCase))
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

    private static bool IsPhoto(ItemInfo i) =>
        i.Photo != null || i.Image != null;

    private static bool IsVideo(ItemInfo i) =>
        i.Video != null;

    private static DateTime GetItemDate(ItemInfo item) =>
        item.Photo?.Takendatetime?.DateTime
        //?? item.FileSystemInfo?.CreatedDateTime?.DateTime
        ?? item.LastModifiedDateTime?.DateTime
        ?? DateTime.UtcNow;
}