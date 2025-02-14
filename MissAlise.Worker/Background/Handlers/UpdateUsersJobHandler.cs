using MissAlise.Background;

namespace MissAlise.Worker.Background.Handlers
{
	public class UpdateUsersJobHandler : BackgroundJobHandler<UpdateUsersJob>
	{
		private readonly ILogger<UpdateUsersJobHandler> logger;
		private readonly IServiceScopeFactory factory;

		public UpdateUsersJobHandler(ILogger<UpdateUsersJobHandler> logger, IServiceScopeFactory factory)
		{
			this.logger = logger;
			this.factory = factory;
		}

		public override async Task HandleAsync(UpdateUsersJob backgroundTask, CancellationToken cancel)
		{
			logger.LogInformation(" Start task {task}", nameof(UpdateUsersJobHandler));
			//using var scope = factory.CreateScope();
			//using var db = scope.ServiceProvider.GetRequiredService<UserMediaContext>();
			//var items = db.Files.Include(file => file.Folder).ToList();
			//Folder f = new Folder() { Name = "Root", Path = "D:\\Images2" };
			//Folder f2 = new Folder() { Name = "Oli", Path = "D:\\Images\\Img1", Parent = f };
			//Folder f3 = new Folder() { Name = "Oli", Path = "D:\\Images\\Img2", Parent = f };
			//Folder f4 = new Folder() { Name = "Oli", Path = "D:\\Images\\Img1", Parent = f3 };
			//f.Folders.Add(f2);
			//f.Folders.Add(f3);
			//f3.Folders.Add(f4);
			//var fresult = await db.Folders.AddAsync(f);
			//await db.SaveChangesAsync(cancel);
			for (var i = 0; i < 1; i++)
			{
				//logger.LogInformation("{i} {time}", i, Time.Now);
				await Task.Delay(1000);
			}
		}
		
		public override async Task EndAsync(BackgroundJob<UpdateUsersJob> job, CancellationToken cancel)
		{
			logger.LogInformation(" end task {task}", nameof(UpdateUsersJobHandler));
			await base.EndAsync(job, cancel);
		}
	}
}