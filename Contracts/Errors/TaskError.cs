namespace autoplaysharp.Contracts.Errors
{
    public record TaskError;

    public record ElementNotFoundError(UiElement MissingElement) : TaskError;
}
