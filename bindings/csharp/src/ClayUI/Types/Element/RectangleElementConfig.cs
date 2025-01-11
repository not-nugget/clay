namespace Clay.Types.Element;

public struct RectangleElementConfig : IElementConfig
{
    public ClayColor    Color        { get; set; }
    public CornerRadius CornerRadius { get; set; }

    //TODO Clay has default functionality for extending the default rectangle element configuration with custom data. there is no real easy way to do this, and 99% of users won't need this, but if it becomes a requested feature, this binding extension should be able to support extension of the rectangle element. do note that this would indicate editing and recompiling the original header file with additional define constraints, as well as a custom dll import, so this might not be a trivial task
}