namespace Clay.Types.Rendering;

public enum ClayRenderCommandType : byte
{
    None,
    Rectangle,
    Border,
    Text,
    Image,
    ScissorStart,
    ScissorEnd,
    Custom,
}