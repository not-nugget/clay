namespace Clay.Types.Error;

/// <summary>Represents the error type observed by Clay</summary>
public enum ErrorType : byte
{
    TextMeasurementFunctionNotProvided,
    ArenaCapacityExceeded,
    ElementsCapacityExceeded,
    TextMeasurementCapacityExceeded,
    DuplicateId,
    FloatingContainerParentNotFound,
    InternalError,
}