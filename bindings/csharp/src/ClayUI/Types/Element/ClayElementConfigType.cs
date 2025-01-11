namespace Clay.Types.Element;

[Flags]
public enum ClayElementConfigType : byte
{
    None              = 0,
    Rectangle         = 1,
    BorderContainer   = 2,
    FloatingContainer = 4,
    ScrollContainer   = 8,
    Image             = 16,
    Text              = 32,
    Custom            = 64,
}