using System.Text.Json.Serialization;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace MissAlise.Bot
{
	internal class GetUpdatesRequest<TUpdate> : RequestBase<TUpdate[]> where TUpdate : Update
	{
		public GetUpdatesRequest() : base("getUpdates")
		{
		}

		/// <summary>Identifier of the first update to be returned. Must be greater by one than the highest among the identifiers of previously received updates. By default, updates starting with the earliest unconfirmed update are returned. An update is considered confirmed as soon as <see cref="TelegramBotClientExtensions.GetUpdates">GetUpdates</see> is called with an <see cref="Offset">Offset</see> higher than its <em>UpdateId</em>. The negative offset can be specified to retrieve updates starting from <em>-offset</em> update from the end of the updates queue. All previous updates will be forgotten.</summary>
		public int? Offset { get; set; }

		/// <summary>Limits the number of updates to be retrieved. Values between 1-100 are accepted. Defaults to 100.</summary>
		public int? Limit { get; set; }

		/// <summary>Timeout in seconds for long polling. Defaults to 0, i.e. usual short polling. Should be positive, short polling should be used for testing purposes only.</summary>
		public int? Timeout { get; set; }

		/// <summary>A list of the update types you want your bot to receive. For example, specify <c>["message", "EditedChannelPost", "CallbackQuery"]</c> to only receive updates of these types. See <see cref="Update"/> for a complete list of available update types. Specify an empty list to receive all update types except <em>ChatMember</em>, <em>MessageReaction</em>, and <em>MessageReactionCount</em> (default). If not specified, the previous setting will be used.<br/><br/>Please note that this parameter doesn't affect updates created before the call to getUpdates, so unwanted updates may be received for a short period of time.</summary>
		[JsonPropertyName("allowed_updates")]
		public IEnumerable<UpdateType>? AllowedUpdates { get; set; }
	}
}
