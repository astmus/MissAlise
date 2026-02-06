namespace MissAlise.TelegramBot.Results;

public sealed record OutMessage(
    string Text,
    OutParseMode ParseMode = OutParseMode.Markdown,
    OutKeyboard? Keyboard = null
);

public enum OutParseMode
{
    Plain,
    Markdown,
    MarkdownV2,
    Html
}
