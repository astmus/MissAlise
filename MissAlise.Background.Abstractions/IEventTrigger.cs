//using System;
//using System.Collections.Generic;
//using System.Text;

//namespace MissAlise.Background
//{
//	public interface IEventTrigger
//	{
//		DayOfWeek[] Days { get; set; }
//		TimeSpan? Delay { get; set; }
//		string Description { get; set; }
//		TimeOnly? EndAt { get; set; }
//		TimeSpan? FreezeTime { get; set; }
//		bool IsEnabled { get; set; }
//		BackgroundJob Job { get; }
//		string JobKey { get; set; }
//		TimeOnly? RunAt { get; set; }
//		DateOnly? StartAt { get; set; }
//		MonthWeek[] Weeks { get; set; }

//		bool Check();
//		ValueTask Fire(CancellationToken cancel);
//	}
//}
