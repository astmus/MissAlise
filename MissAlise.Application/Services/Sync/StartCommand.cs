using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;

namespace MissAlise.Application.Services.Sync
{
	public record StartCommand() : ICommand;

	public class StartCommandHandler : AsyncHandlerBase<StartCommand>, ICommandHandler<StartCommand>
	{
		public Task<Result> Handle(StartCommand request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task HandleAsync(StartCommand data, CancellationToken cancel)
		{
			throw new NotImplementedException();
		}
	}
}
