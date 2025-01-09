namespace Clay.Types.Error;

/// <summary>Managed exception that may be thrown in response to an error raised by Clay</summary>
public sealed class ClayException(ErrorData data) : Exception($"Clay observed error \"${data.ErrorType}\" with message \"${data.ErrorText}\"")
{
    /// <summary>Clay provided data about the observed error</summary>
    public ErrorData ErrorData { get; } = data;
}