using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MissAlise.Application.Common;
using MissAlise.Application.Common.RequestHandler;

namespace MissAlise.Application.Commands
{	
	public record TestCommand() : ICommand;

	public class TestCommandHandler : AsyncHandlerBase<TestCommand>, ICommandHandler<TestCommand>
	{
		public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		protected override Task HandleAsync(TestCommand data, CancellationToken cancel)
		{
			throw new NotImplementedException();
		}
	}
}
