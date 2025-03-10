using Microsoft.Extensions.Logging;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;
using MissAlise.Application.Interfaces;
using MissAlise.Background;
using MissAlise.Entities.OneDrive;
using MissAlise.Interfaces;

namespace MissAlise.Application.Users.Requests
{
	public record BackgroundJobDto(string Key, string Description, EventTrigger Trigger = null);
	public record GetBackgroundJobsQuery(int? userId) : ICommand<IEnumerable<BackgroundJobDto>>;

	public class GetBackgroundJobsQueryHandler : AsyncHandlerBase<GetBackgroundJobsQuery>, ICommandHandler<GetBackgroundJobsQuery, IEnumerable<BackgroundJobDto>>
	{
		private readonly ILogger<GetBackgroundJobsQueryHandler> log;
		private readonly IUserRepository usersRepository;
		private readonly IOneDriveService oneDrive;
		private readonly IHandleContext ctx;
		const string ROOT_SYNC_PATH = @"M:\Sync\"; // и вот это барахло тоже убрать
		public GetBackgroundJobsQueryHandler(ILogger<GetBackgroundJobsQueryHandler> log, IHandleContext ctx)
		{
			this.log = log;
			this.ctx = ctx;
		}

		public Task<Result<IEnumerable<BackgroundJobDto>>> Handle(GetBackgroundJobsQuery request, CancellationToken cancellationToken)
		{
			var items = BackgroundServer.Current.Jobs.Union(BackgroundServer.Current.Triggers.Select(trigger => trigger.Job)).Distinct().Select(job => new BackgroundJobDto(job.Key, job.Description));
			return Task.FromResult(Result.Ok(items));
		}

		protected override Task HandleAsync(GetBackgroundJobsQuery data, CancellationToken cancel)
			=> throw new NotImplementedException();
	}
}
