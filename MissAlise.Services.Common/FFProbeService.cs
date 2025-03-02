using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace MissAlise.Services
{
	public interface IMediaService
	{
		Task<MediaInfo> GetMetaDataAsync(string filePath, CancellationToken cancel);
		Task<MediaInfo> ImageFromAsync(string filePath, ApplyImageParams args, CancellationToken cancel);
	}

	public class FFProbeService : IMediaService
	{
		public async Task<MediaInfo> GetMetaDataAsync(string filePath, CancellationToken cancel)
		{
			var fileName = Path.GetFileName(filePath);
			var folder = Path.GetDirectoryName(filePath);
			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffprobe",
					Arguments = $"-v quiet -print_format json -show_format -show_streams \"{fileName}\"",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = folder
				}
			};

			process.Start();
			await process.WaitForExitAsync(cancel);

			if (process.ExitCode != 0)
			{
				var error = await process.StandardError.ReadToEndAsync(cancel);
				return new MediaInfo() { Anomaly = error };
			}

			return await JsonSerializer.DeserializeAsync<MediaInfo>(process.StandardOutput.BaseStream, cancellationToken: cancel);
		}

		public async Task<MediaInfo> ImageFromAsync(string filePath, ApplyImageParams args, CancellationToken cancel)
		{
			var inputName = Path.GetFileName(filePath);
			var folder = Path.GetDirectoryName(filePath);
			var outputName = $"{args.Name}.{args.Format}";

			var parameters = new StringBuilder($"-i \"{inputName}\" ");

			if (args.Height is int height && args.Width is int width)
				parameters.Append($"-vf scale={width}x{height}");

			parameters.Append($" \"{args.Name}.{args.Format}\"");

			var process = new Process
			{
				StartInfo = new ProcessStartInfo
				{
					FileName = "ffmpeg",
					Arguments = parameters.ToString(),
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = folder
				}
			};

			process.Start();
			await process.WaitForExitAsync(cancel);

			if (process.ExitCode != 0)
			{
				var error = await process.StandardError.ReadToEndAsync(cancel);
				return new MediaInfo() { Anomaly = error };
			}
			var info = await GetMetaDataAsync(Path.Combine(folder, outputName), cancel);
			return info;
		}

		private static void ExecuteCommand(string command)
		{
			var processInfo = new ProcessStartInfo("cmd", $"/C {command}")
			{
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using var process = new Process { StartInfo = processInfo };
			process.Start();

			string result = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			process.WaitForExit();

			if (!string.IsNullOrWhiteSpace(error))
			{

			}
		}
	}
}
