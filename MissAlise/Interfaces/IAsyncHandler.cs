namespace MissAlise.Interfaces
{
	public interface IAsyncHandler<in TData>
	{
		Task InvokeAsync(TData cmd, CancellationToken cancel);
	}
}
