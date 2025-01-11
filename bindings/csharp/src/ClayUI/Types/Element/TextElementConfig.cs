namespace Clay.Types.Element;

public struct TextElementConfig : IElementConfig
{
    public ClayColor                     TextColor     { get; set; }
    public ushort                        FontId        { get; set; } //TODO helper FontAsset
    public ushort                        FontSize      { get; set; }
    public ushort                        LetterSpacing { get; set; }
    public ushort                        LineHeight    { get; set; }
    public ClayTextElementConfigWrapMode WrapMode      { get; set; }

    //TODO Read rectangle element above for information about element data extension
}