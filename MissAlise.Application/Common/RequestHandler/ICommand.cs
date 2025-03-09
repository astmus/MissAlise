using MediatR;

namespace MissAlise.Application.Common.RequestHandler
{
	public interface ICommand : IRequest<Result> // ICommand<object> ?
	{
	}

	public interface ICommand<TResponse> : IRequest<Result<TResponse>>
	{
	}
}
