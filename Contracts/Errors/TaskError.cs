namespace autoplaysharp.Contracts.Errors
{
    public record TaskError;

    public record ElementNotFoundError(UIElement MissingElement) : TaskError;
}
