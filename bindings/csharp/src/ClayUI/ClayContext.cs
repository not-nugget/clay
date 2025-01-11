using Clay.Types.Error;

namespace Clay;

/// <summary>Represents a context used to interface with the Clay library</summary>
public sealed class ClayContext : IDisposable
{
    private readonly ClayArena     _arena;
    private          LayoutHandle? _currentLayout;

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
    ///     <br />
    ///     var layout = context.Layout()
    ///     using(layout)
    ///     {
    ///         // Layout dependent elements and code
    ///     }
    ///     <br />
    ///     // Layout independent code
    ///     <br />
    ///     // Access the render commands from the ended layout
    ///     var renderCommands = layout.RenderCommands;
    /// </code>
    ///     Valid alternative:
    ///     <code>
    ///     // Layout independent code
    ///     <br />
    ///     var layout = context.Layout();
    ///     <br />
    ///     // Layout dependent elements and code
    ///     <br />
    ///     // Manually end the layout for access to the render commands
    ///     var renderCommands = layout.End();
    /// </code>
    ///     You may also use a scoped-lifetime variable, but the layout must be returned to another
    ///     control flow. This way the layout handle will be ended automatically once the original
    ///     scope ends, and the render commands may be safely accessed
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

//TODO does this need to be a class?
/// <summary>Represents a layout in Clay. Only one layout may exist per <see cref="ClayContext" /></summary>
public sealed class LayoutHandle : IDisposable
{
    internal bool IsEnded { get; private set; }

    //TODO do we need to store a collection of the render commands, or can they be consumed exclusively through End()?
    public  IEnumerable<ClayRenderCommand> RenderCommands => _renderCommands;
    private ClayRenderCommand[]            _renderCommands = [];

    private readonly ClayContext _context;

    internal LayoutHandle(ClayContext context) { _context = context; }

    /// <summary>Ends the current layout</summary>
    /// <remarks>
    ///     Though manual layout management is possible, it is not recommended and should be
    ///     avoided in favor of automatic layout lifecycle management via lifetime scopes
    /// </remarks>
    public IEnumerable<ClayRenderCommand> End()
    {
        if (IsEnded)
            return RenderCommands;

        IsEnded         = true;
        _renderCommands = Clay.EndLayout();
        return RenderCommands;
    }

    public void Dispose()
        => End();

    #region Text Element overloads
    /// <summary>Creates a new text element with an optional configuration</summary>
    /// <remarks>Text elements are automatically opened, configured, and closed upon creation, as
    /// they are not allowed to have children</remarks>
    public TextElementHandle Text(string text, TextElementConfig config = default)
        => new TextElementHandle(this, text, config);
    #endregion

    #region Identified Element overloads
    /// <summary>Creates a new element with a custom ID and default configuration</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     using(var element = layout.Element("UniqueId"))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var element = layout.Element("UniqueId");
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element(ElementId elementId)
    {
        var handle = new ElementHandle(this, elementId);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new element with a custom ID and a single configuration</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    ///     <br />
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config = ...;
    ///     using(var element = layout.Element("UniqueId", config))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config = ...;
    ///     var element = layout.Element("UniqueId", config);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1>
    (
        ElementId elementId,
        T1        config
    )
        where T1 : struct, IElementConfig
    {
        var handle = new ElementHandle(this, elementId);
        handle.Configure(config);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new element with a custom ID and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     using(var element = layout.Element("UniqueId", config1, config2))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     var element = layout.Element("UniqueId", config1, config2);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2>
    (
        ElementId elementId,
        T1        config1,
        T2        config2
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
    {
        var handle = new ElementHandle(this, elementId);
        handle.Configure(config1);
        handle.Configure(config2);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new element with a custom ID and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element("UniqueId", config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element("UniqueId", config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3>
    (
        ElementId elementId,
        T1        config1,
        T2        config2,
        T3        config3
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
    {
        var handle = new ElementHandle(this, elementId);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new element with a custom ID and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element("UniqueId", config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element("UniqueId", config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3, T4>
    (
        ElementId elementId,
        T1        config1,
        T2        config2,
        T3        config3,
        T4        config4
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
        where T4 : struct, IElementConfig
    {
        var handle = new ElementHandle(this, elementId);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        handle.Configure(config4);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new element with a custom ID and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element("UniqueId", config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element("UniqueId", config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3, T4, T5>
    (
        ElementId elementId,
        T1        config1,
        T2        config2,
        T3        config3,
        T4        config4,
        T5        config5
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
        where T4 : struct, IElementConfig
        where T5 : struct, IElementConfig
    {
        var handle = new ElementHandle(this, elementId);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        handle.Configure(config4);
        handle.Configure(config5);
        Clay.PostConfigureOpenElement();
        return handle;
    }
    #endregion

    #region Anonymous Element overloads
    /// <summary>Creates a new anonymous element and default configuration</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     using(var element = layout.Element())
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var element = layout.Element();
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element()
    {
        var handle = new ElementHandle(this);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new anonymous element and a single configuration</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    ///     <br />
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config = ...;
    ///     using(var element = layout.Element(config))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config = ...;
    ///     var element = layout.Element(config);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1>(T1 config) where T1 : struct, IElementConfig
    {
        var handle = new ElementHandle(this);
        handle.Configure(config);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new anonymous element and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     using(var element = layout.Element(config1, config2))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     var element = layout.Element(config1, config2);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2>
    (
        T1 config1,
        T2 config2
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
    {
        var handle = new ElementHandle(this);
        handle.Configure(config1);
        handle.Configure(config2);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new anonymous element and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element(config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element(config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3>
    (
        T1 config1,
        T2 config2,
        T3 config3
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
    {
        var handle = new ElementHandle(this);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new anonymous element and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element(config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element(config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3, T4>
    (
        T1 config1,
        T2 config2,
        T3 config3,
        T4 config4
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
        where T4 : struct, IElementConfig
    {
        var handle = new ElementHandle(this);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        handle.Configure(config4);
        Clay.PostConfigureOpenElement();
        return handle;
    }

    /// <summary>Creates a new anonymous element and multiple configurations</summary>
    /// <returns><see cref="ElementHandle" /> to reference</returns>
    /// <remarks>
    ///     Elements may have an arbitrary number of ancestors, siblings and children, so long
    ///     as they are defined within the respective element's scope. See the example for recommended usage
    /// </remarks>
    /// <example>
    ///     Recommended approach:
    ///     <code>
    ///     // Parent and sibling elements and code, or root layout
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     using(var element = layout.Element(config1, config2, ...))
    ///     {
    ///         // Child elements and code
    ///     }
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Not-recommended alternative:
    ///     <code>
    ///     // Parent element or layout root
    ///     <br/>
    ///     var config1 = ...;
    ///     var config2 = ...;
    ///     ...
    ///     var element = layout.Element(config1, config2, ...);
    ///     <br/>
    ///     // Child Element(s) and code
    ///     <br/>
    ///     element.End();
    ///     <br/>
    ///     // Sibling elements and code
    /// </code>
    ///     Using the lifetime variable approach makes it more difficult to manage sibling
    ///     elements and element lifetimes. Block-scoped lifetimes show a clear relationship
    ///     between the elements defined before, around and within a given element (or layout)
    ///     at the cost of more nesting
    /// </example>
    public ElementHandle Element<T1, T2, T3, T4, T5>
    (
        T1 config1,
        T2 config2,
        T3 config3,
        T4 config4,
        T5 config5
    )
        where T1 : struct, IElementConfig
        where T2 : struct, IElementConfig
        where T3 : struct, IElementConfig
        where T4 : struct, IElementConfig
        where T5 : struct, IElementConfig
    {
        var handle = new ElementHandle(this);
        handle.Configure(config1);
        handle.Configure(config2);
        handle.Configure(config3);
        handle.Configure(config4);
        handle.Configure(config5);
        Clay.PostConfigureOpenElement();
        return handle;
    }
    #endregion
}

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

/// <summary>Represents text in Clay. Text elements are always configured
/// upon creation and immediately closed, as text may not have any children</summary>
public readonly struct TextElementHandle : IEquatable<TextElementHandle>
{
    private readonly LayoutHandle _layout;
    private readonly string       _text;

    internal TextElementHandle(LayoutHandle layout, string text, TextElementConfig config)
    {
        _layout = layout;
        _text   = text;

        Clay.OpenTextElement(text, ref config);

        //Clay.PostConfigureOpenElement();
        // DO NOT manually call Close. Clay automatically closes the text element for us
    }

    //TODO EQ must be strengthened, perhaps if we get the clay-generated ID and store it here. Text cannot have custom IDs for some reason
    public override bool Equals(object? obj)
        => obj is TextElementHandle other && Equals(other);

    public override int GetHashCode()
        => HashCode.Combine(_layout.GetHashCode(), _text.GetHashCode());

    public bool Equals(TextElementHandle other)
        => ReferenceEquals(_layout, other._layout) && _text == other._text;

    public static bool operator ==(TextElementHandle left, TextElementHandle right) => left.Equals(right);
    public static bool operator !=(TextElementHandle left, TextElementHandle right) => !(left == right);
}