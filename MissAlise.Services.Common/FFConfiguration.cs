namespace MissAlise.Services
{
	public class FFConfiguration
	{
		public string MpegPath { get; set; } = "ffmpeg"; //= Environment.GetEnvironmentVariable("ff")
		public string ProbePath { get; set; } = "ffprobe";//= Environment.GetEnvironmentVariable("ff")
	}
}
