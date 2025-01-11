namespace Clay.Types.Element;

/// <summary>Configures an element's various layout options</summary>
public struct LayoutConfig : IElementConfig
{
    //TODO layout config technically is not an element config, so it really shouldn't piggyback off of IElementConfig. Maybe have an outer interface that marks any config that can be used in the type overloads in LayoutHandle, or maybe not...this might be the nitpick of the century
    public Sizing          Sizing          { get; set; }
    public Padding         Padding         { get; set; }
    public ushort          ChildGap        { get; set; }
    public ChildAlignment  ChildAlignment  { get; set; }
    public LayoutDirection LayoutDirection { get; set; }
}