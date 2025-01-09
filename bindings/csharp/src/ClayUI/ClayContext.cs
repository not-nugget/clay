using Clay.Types.Error;

namespace Clay;

/// <summary>Represents a context used to interface with the Clay library</summary>
public sealed class ClayContext : IDisposable
{
    private readonly ClayArena _arena;

    private LayoutHandle? _currentLayout;

    internal ClayContext(ClayArena arena) { _arena = arena; }

    public void Dispose()
    {
        _currentLayout?.Dispose();
        _arena.Dispose();
    }

    /// <summary>Starts a new layout</summary>
    /// <returns><see cref="LayoutHandle" /> to reference</returns>
    /// <exception cref="InvalidOperationException">
    ///     When called within the scope of an existing <see cref="LayoutHandle" />'s
    ///     lifetime
    /// </exception>
    /// <remarks>
    ///     It is recommended to group together a layout and its children
    ///     within the same block scope, either by using a scoped lifetime, or
    ///     with a scoped lifetime variable. See the example for details
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Layout independent code
    /// 
    ///     using(var layout = context.Layout())
    ///     {
    ///         // Layout dependent elements and code
    ///     }
    /// 
    ///     // Layout independent code
    /// </code>
    ///     Valid alternative:
    ///     <code>
    ///     // Layout independent code
    /// 
    ///     using var layout = context.Layout();
    /// 
    ///     // Layout dependent elements and code
    /// </code>
    /// </example>
    public LayoutHandle Layout()
    {
        if (_currentLayout is not null && !_currentLayout.IsEnded)
            throw new InvalidOperationException("ClayContext.BeginLayout() has already been called. You must dispose the previous layout, or call ClayContext.EndLayout() before beginning a new layout");

        _currentLayout = new LayoutHandle(this);
        Clay.BeginLayout();

        return _currentLayout;
    }

    /// <summary>Creates and initializes a new <see cref="ClayContext" /> instance with an optional error handler callback function</summary>
    public static ClayContext Create(ClayDimensions dimensions = default, ErrorHandlerFunction? errorHandlerCallback = null)
        => Clay.Initialize(dimensions, errorHandlerCallback);
}

/// <summary>Represents a layout in Clay. Only one layout may exist per <see cref="ClayContext" /></summary>
public sealed class LayoutHandle : IClayHandle
{
    private readonly ClayContext _context;

    internal LayoutHandle(ClayContext context) { _context = context; }

    public ClayRenderCommand[] RenderCommands { get; private set; } = [];
    
    public bool IsEnded { get; private set; }

    /// <summary>Ends the current layout</summary>
    /// <remarks>
    ///     Though manual layout management is possible, it is not recommended and should be
    ///     avoided in favor of automatic layout lifecycle management via lifetime scopes
    /// </remarks>
    public void End()
    {
        if (IsEnded)
            return;
        
        RenderCommands = Clay.EndLayout();
        IsEnded = true;
    }

    /// <summary>Creates a new element</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    /// 
    ///     using(var element = layout.Element(...))
    ///     {
    ///         // Child elements and code
    ///     }
    /// 
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    /// 
    ///     var element = layout.Element(...);
    /// 
    ///     // Child Element(s) and code
    /// 
    ///     element.End();
    /// 
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element()
    {
        //TODO element id thought: clay has a built-in mechanism for creating indexed IDs, would be nice to tap into that, although it may be condusive just to use the managed side of things for ID generation full stop
        return new ElementHandle(this);
    }
    
    public void Dispose()
        => End();
}

/// <summary>
///     Represents any element in Clay. Elements may have any number of ancestor, sibling or child elements defined,
///     so long as they are defined within the same layout via <see cref="LayoutHandle" />
/// </summary>
public struct ElementHandle : IClayHandle
{
    private readonly LayoutHandle _layout;

    public bool IsEnded { get; private set; }

    internal ElementHandle(LayoutHandle layout) { _layout = layout; }

    /// <summary>Ends the current element</summary>
    /// <remarks>
    ///     Though manual element management is possible, it is not advised and should be
    ///     avoided in favor of automatic element lifecycle management via lifetime scopes
    /// </remarks>
    public void End()
    {
        if (IsEnded)
            return;

        //TODO Call Clay.EndElement() and end the element within clay
        IsEnded = true;
    }

    public void Dispose()
        => End();
}

/// <summary>Exposes members which are shared by many different managed handles that point to unmanaged Clay items</summary>
public interface IClayHandle : IDisposable
{
    /// <summary>Indicates if the handle has been ended or otherwise disposed</summary>
    bool IsEnded { get; }

    /// <summary>Ends the given handle</summary>
    void End();
}