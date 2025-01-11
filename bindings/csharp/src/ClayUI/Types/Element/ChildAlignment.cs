namespace Clay.Types.Element;

/// <summary>Controls the alignment of an element's children</summary>
public struct ChildAlignment
{
    public LayoutAlignmentX X { get; set; }
    public LayoutAlignmentY Y { get; set; }

    public static implicit operator ChildAlignment((LayoutAlignmentX, LayoutAlignmentY) tuple) => new ChildAlignment { X = tuple.Item1, Y = tuple.Item2 };
}