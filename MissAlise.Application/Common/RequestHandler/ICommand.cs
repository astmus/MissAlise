using MediatR;

namespace MissAlise.Application.Common.RequestHandler
{
	public interface ICommand : IRequest<Result>
	{
	}
	public interface ICommand<TResponse> : IRequest<Result<TResponse>>
	{
	}
}
