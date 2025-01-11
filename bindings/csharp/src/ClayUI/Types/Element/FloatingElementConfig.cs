using Clay.Types.Internal;

namespace Clay.Types.Element;

internal struct FloatingElementConfig : IElementConfig
{
    public ClayVector2              Offset             { get; set; }
    public ClayDimensions           Expand             { get; set; }
    public ushort                   ZIndex             { get; set; }
    public uint                     ParentId           { get; set; }
    public ClayFloatingAttachPoints Attachment         { get; set; }
    public ClayPointerCaptureMode   PointerCaptureMode { get; set; }
}