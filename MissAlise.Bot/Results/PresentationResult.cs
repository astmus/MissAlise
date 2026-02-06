namespace MissAlise.TelegramBot.Results;

public abstract record PresentationResult
{
    public static PresentationResult Empty { get; } = new EmptyResult();

    private sealed record EmptyResult : PresentationResult;
}

public sealed record MessageResult(OutMessage Message) : PresentationResult;

public sealed record MultiMessageResult(IReadOnlyList<OutMessage> Messages) : PresentationResult;

public sealed record ErrorResult(string Title, string? Details = null) : PresentationResult;
