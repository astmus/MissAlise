using System.Diagnostics;
using System.Text.Json;

namespace MissAlise.Services
{
	public interface IMediaService
	{
		Task<MediaInfo> GetMetaData(string filePath, CancellationToken cancel);
	}

	public class FFProbeService : IMediaService
	{
		public async Task<MediaInfo> GetMetaData(string filePath, CancellationToken cancel)
		{
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffprobe",
					Arguments = $"-v quiet -print_format json -show_format -show_streams \"{filePath}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					//WorkingDirectory
				}
			};

			process.Start();
			//var output = await process.StandardOutput.ReadToEndAsync(cancel);
			await process.WaitForExitAsync(cancel);

			if (process.ExitCode != 0)
			{
				var output = await process.StandardError.ReadToEndAsync(cancel);
				throw new Exception("Ошибка при выполнении ffprobe.");
			}

			return await JsonSerializer.DeserializeAsync<MediaInfo>(process.StandardOutput.BaseStream, cancellationToken: cancel);
		}
	}
}
