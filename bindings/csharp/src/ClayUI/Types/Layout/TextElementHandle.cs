using Clay.Types.Element;

namespace Clay.Types.Layout;

/// <summary>Represents text in Clay. Text elements are always configured
/// upon creation and immediately closed, as text may not have any children</summary>
public readonly struct TextElementHandle : IEquatable<TextElementHandle>
{
    private readonly LayoutHandle _layout;
    private readonly string       _text;

    internal TextElementHandle(LayoutHandle layout, string text, TextElementConfig config)
    {
        _layout = layout;
        _text   = text;

        Clay.OpenTextElement(text, ref config);

        //Clay.PostConfigureOpenElement();
        // DO NOT manually call Close. Clay automatically closes the text element for us
    }

    //TODO EQ must be strengthened, perhaps if we get the clay-generated ID and store it here. Text cannot have custom IDs for some reason
    public override bool Equals(object? obj)
        => obj is TextElementHandle other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(_layout.GetHashCode(), _text.GetHashCode());

    public bool Equals(TextElementHandle other)
        => ReferenceEquals(_layout, other._layout) && _text == other._text;

    public static bool operator ==(TextElementHandle left, TextElementHandle right) => left.Equals(right);
    public static bool operator !=(TextElementHandle left, TextElementHandle right) => !(left == right);
}