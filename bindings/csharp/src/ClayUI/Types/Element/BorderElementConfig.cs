namespace Clay.Types.Element;

internal struct BorderElementConfig : IElementConfig
{
    public ClayBorder   Left            { get; set; }
    public ClayBorder   Right           { get; set; }
    public ClayBorder   Top             { get; set; }
    public ClayBorder   Bottom          { get; set; }
    public ClayBorder   BetweenChildren { get; set; }
    public CornerRadius CornerRadius    { get; set; }
}