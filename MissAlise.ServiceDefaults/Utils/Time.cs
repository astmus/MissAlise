namespace MissAlise.Utils
{
	public class Time
	{
		public static DateOnly DateNow => DateOnly.FromDateTime(Now);
		public static TimeOnly OnlyNow => TimeOnly.FromDateTime(Now);

		public static readonly TimeSpan Second = TimeSpan.FromSeconds(1);
		public static readonly TimeSpan Minute = TimeSpan.FromMinutes(1);
		public static readonly TimeSpan Day = TimeSpan.FromDays(1);
#if DEBUG
		public static DateTime Now => DateTime.UtcNow.ToLocalTime();
#else
        public static DateTime Now => DateTime.UtcNow;
#endif
		public static DateOnly OnlyDate(in DateTime dt) => DateOnly.FromDateTime(dt);
		public static TimeOnly OnlyTime(in DateTime dt) => TimeOnly.FromDateTime(dt);
	}
	public static class TimeExtension
	{
		public static DateTime TrimSeconds(this in DateTime dt)
		{
			return dt.AddTicks(-(dt.Ticks % TimeSpan.TicksPerSecond));
		}
	}
}