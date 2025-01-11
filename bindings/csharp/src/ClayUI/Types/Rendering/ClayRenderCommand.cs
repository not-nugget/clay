using Clay.Types.Element;
using Clay.Types.Internal;

namespace Clay.Types.Rendering;

/// <summary>Command that can be used to draw a part of the Clay UI to a window</summary>
public struct ClayRenderCommand
{
    public ClayBoundingBox BoundingBox { get; private set; }

    //public IElementConfig  Config      => null; //TODO do we need the element config in the marshaled runtime?
    private ClayElementConfigPointer _configPointer;

    //public  string     Text => null; //TODO get the string from ClayString, if ClayString is valid
    private ClayString text;

    public uint                  Id          { get; set; }
    public ClayRenderCommandType CommandType { get; set; }
}