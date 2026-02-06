using System.Diagnostics;
using System.Threading.Channels;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.TelegramBot
{
	internal class UpdatesSource<TUpdate> : IAsyncEnumerable<TUpdate> where TUpdate : Update
	{
		private readonly ITelegramBotClient _botClient;
		private readonly ReceiverOptions? _receiverOptions;
		private readonly Func<Exception, CancellationToken, Task>? _pollingErrorHandler;
		private int _inProcess;
		private Enumerator _enumerator;

		public UpdatesSource(ITelegramBotClient botClient, ReceiverOptions? receiverOptions = null, Func<Exception, CancellationToken, Task>? pollingErrorHandler = null)
		{
			_botClient = botClient;
			_receiverOptions = receiverOptions;
			_pollingErrorHandler = pollingErrorHandler;
		}

		public IAsyncEnumerator<TUpdate> GetAsyncEnumerator(CancellationToken cancellationToken) 
		 {
			if (Interlocked.CompareExchange(ref _inProcess, 1, 0) is 1)
				throw new InvalidOperationException(nameof(GetAsyncEnumerator) + " may only be called once");

			return _enumerator = new(receiver: this, cancellationToken: cancellationToken);
		}

		protected class Enumerator : IAsyncEnumerator<TUpdate>
		{
			private readonly UpdatesSource<TUpdate> _receiver;
			private readonly CancellationTokenSource _cts;
			private readonly CancellationToken _token;
			private readonly UpdateType[]? _allowedUpdates;
			private readonly int? _limit;
			private Exception? _uncaughtException;
			private TUpdate? _current;
			private int _pendingUpdates;
			private int _messageOffset;
			private readonly Channel<TUpdate> _channel;

			public int PendingUpdates => _pendingUpdates;

			public Enumerator(UpdatesSource<TUpdate> receiver, CancellationToken cancellationToken)
			{
				_receiver = receiver;
				_cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, default);
				_token = _cts.Token;
				_messageOffset = receiver._receiverOptions?.Offset ?? 0;
				_limit = receiver._receiverOptions?.Limit ?? default;
				_allowedUpdates = receiver._receiverOptions?.AllowedUpdates;

				_channel = Channel.CreateUnbounded<TUpdate>(new() { SingleReader = true, SingleWriter = true });

#pragma warning disable CA2016
				_ = Task.Run(ReceiveUpdatesAsync, cancellationToken);
#pragma warning restore CA2016
			}

			public ValueTask<bool> MoveNextAsync()
			{
				if (_uncaughtException is not null) throw _uncaughtException;
				_token.ThrowIfCancellationRequested();
				if (_channel.Reader.TryRead(out _current))
				{
					Interlocked.Decrement(ref _pendingUpdates);
					return new(result: true);
				}

				return new(ReadAsync());
			}

			private async Task<bool> ReadAsync()
			{
				_current = await _channel.Reader.ReadAsync(_token).ConfigureAwait(false);
				Interlocked.Decrement(ref _pendingUpdates);
				return true;
			}

			private async Task ReceiveUpdatesAsync()
			{
				if (_receiver._receiverOptions?.DropPendingUpdates is true)
				{
					try
					{
						var updates = await _receiver._botClient.GetUpdates(-1, 1, 0, [], _token).ConfigureAwait(false);
						_messageOffset = updates.Length == 0 ? 0 : updates[^1].Id + 1;
					}
					catch (OperationCanceledException)
					{
						// ignored
					}
				}

						//int? id = 307409070;
				var getUpdatesRequest = new GetUpdatesRequest<TUpdate>
				{
					Offset = _messageOffset,
					Limit = _limit,
					Timeout = (int)_receiver._botClient.Timeout.TotalSeconds,
					AllowedUpdates = _allowedUpdates,
				};

				while (!_cts.IsCancellationRequested)
				{
					try
					{
						var updateArray = await _receiver._botClient.SendRequest(getUpdatesRequest, cancellationToken: _token).ConfigureAwait(false);

						if (updateArray.Length > 0)
						{
							_messageOffset = updateArray[^1].Id + 1;
							Interlocked.Add(ref _pendingUpdates, updateArray.Length);
														
							foreach (var update in updateArray)
							{
								var success = _channel.Writer.TryWrite(update);
								Debug.Assert(success, "TryWrite should succeed as we are using an unbounded channel");
							}
							getUpdatesRequest.Offset = _messageOffset;
						}
					}
					catch (OperationCanceledException)
					{
						return;
					}
#pragma warning disable CA1031
					catch (Exception ex)
#pragma warning restore CA1031
					{
						Debug.Assert(_uncaughtException is null);

						// If there is no errorHandler or the errorHandler throws, stop receiving
						if (_receiver._pollingErrorHandler is null)
						{
							_uncaughtException = ex;
							_cts.Cancel();
						}
						else
						{
							try
							{
								await _receiver._pollingErrorHandler(ex, _token).ConfigureAwait(false);
							}
#pragma warning disable CA1031
							catch (Exception errorHandlerException)
#pragma warning restore CA1031
							{
								_uncaughtException = new AggregateException("Exception was not caught by the errorHandler.", ex, errorHandlerException);
								_cts.Cancel();
							}
						}

						if (_uncaughtException is not null)
						{
#pragma warning disable CA2201
							_uncaughtException = new("Exception was not caught by the errorHandler.", _uncaughtException);
#pragma warning restore CA2201
						}
					}
				}
			}

			public TUpdate Current => _current!; // _current being null indicates MoveNextAsync was never called

			public ValueTask DisposeAsync()
			{			
				_cts.Cancel();
				_cts.Dispose();
				return ValueTask.CompletedTask;
			}
		}
	}
}
