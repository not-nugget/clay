using Clay.Types.Internal;

namespace Clay.Types.Element;

internal struct ImageElementConfig : IElementConfig
{
    public IntPtr         ImageData        { get; set; }
    public ClayDimensions SourceDimensions { get; set; }

    //TODO Read rectangle element above for information about element data extension
}