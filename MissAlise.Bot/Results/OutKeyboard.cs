namespace MissAlise.TelegramBot.Results;

public sealed record OutKeyboard(IReadOnlyList<IReadOnlyList<OutButton>> Rows);

public sealed record OutButton(string Text, string? CallbackData = null, string? Url = null);
