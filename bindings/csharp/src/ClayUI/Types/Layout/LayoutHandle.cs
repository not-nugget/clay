using Clay.Types.Element;
using Clay.Types.Rendering;

namespace Clay.Types.Layout;

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