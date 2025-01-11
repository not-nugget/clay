namespace Clay.Types.Element;

/// <summary>Indicates how an element is sized</summary>
public enum SizingType
{
    //TODO verify all comments
    /// <summary>Element is sized to fit its contents</summary>
    Fit,

    /// <summary>Element will grow to fit its container</summary>
    Grow,

    /// <summary>Element is sized by a percentage of its container</summary>
    Percent,

    /// <summary>Element is sized by its fixed minimum and maximum</summary>
    Fixed,
}