using Clay.Types.Element;

namespace Clay.Types.Layout;

/// <summary>Represents an element in Clay. Elements may have any number of ancestor, sibling or child
/// elements defined, so long as they are defined within the same layout via <see cref="LayoutHandle" /></summary>
public struct ElementHandle : IDisposable, IEquatable<ElementHandle>
{
    private readonly LayoutHandle _layout;
    private readonly ElementId    _id;

    private bool _isEnded;

    //private ClayElementConfigType _configTypeReference = ClayElementConfigType.None;

    internal ElementHandle(LayoutHandle layout, ElementId id = default)
    {
        _layout = layout;

        Clay.OpenElement();
        Clay.AttachId(ref id);

        _id = id;
    }

    //TODO internally track configurations and catch early errors (such as trying to assign text configs to normal elements, or assigning two of the same configs to one element)?    
    /// <summary>Completes the current element's configuration and opens the new element</summary>
    public void Configure<T>(T config) where T : struct, IElementConfig
    {
        if (config is LayoutConfig layout)
        {
            Clay.AttachLayoutConfig(layout);
            return;
        }
        
        Clay.AttachElementConfig(ClayElementConfigPointer.FromElementConfig(config));
    }

    /// <summary>Ends and closes the current element</summary>
    /// <remarks>
    ///     Though manual element management is possible, it is not advised and should be
    ///     avoided in favor of automatic element lifecycle management via lifetime scopes
    /// </remarks>
    public void End()
    {
        if (_isEnded)
            return;

        Clay.CloseElement();
        _isEnded = true;
    }

    public void Dispose()
        => End();

    public override bool Equals(object? obj)
        => obj is ElementHandle other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(_layout.GetHashCode(), _id.Value.GetHashCode());

    public bool Equals(ElementHandle other)
        => ReferenceEquals(_layout, other._layout) && _id.Value == other._id.Value;

    public static bool operator ==(ElementHandle left, ElementHandle right) => left.Equals(right);
    public static bool operator !=(ElementHandle left, ElementHandle right) => !(left == right);
}