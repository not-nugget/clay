namespace Clay.Types.Element;

/// <summary>Controls the default layout direction of an element's children</summary>
public enum LayoutDirection : byte
{
    /// <summary>Elements are placed starting from the left and continuing to the right</summary>
    LeftToRight,

    /// <summary>Elements are placed at the top and continuing to the bottom</summary>
    TopToBottom,
}