using MissAlise.Application.Interfaces;

namespace MissAlise.Application
{
	public abstract class AsyncHandlerBase<TData> : IAsyncHandler<TData>
	{
		protected int isWork;
		protected CancellationTokenSource? src;
		protected abstract Task HandleAsync(TData data, CancellationToken cancel);

		protected virtual Task OnErroAsync(Exception error)
		{
			return Task.CompletedTask;
		}

		public async Task InvokeAsync(TData data, CancellationToken cancel)
		{
			if (Interlocked.CompareExchange(ref isWork, 1, 0) == 1) return;
			try
			{
				using (src = CancellationTokenSource.CreateLinkedTokenSource(cancel))
				{
					await HandleAsync(data, src.Token).ConfigureAwait(false);
				}
			}
			catch (Exception error)
			{
				await OnErroAsync(error).ConfigureAwait(false);
			}
			finally
			{
				Interlocked.Decrement(ref isWork);
			}
		}
	}
}
