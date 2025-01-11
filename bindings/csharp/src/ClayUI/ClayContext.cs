using Clay.Types.Element;
using Clay.Types.Error;
using Clay.Types.Functions;
using Clay.Types.Internal;
using Clay.Types.Internal.Interop;
using Clay.Types.Layout;

namespace Clay;

/// <summary>Represents a context used to interface with the Clay library</summary>
public sealed class ClayContext : IDisposable
{
    private readonly ClayArenaMemoryHandle _arenaHandle;
    private          LayoutHandle?        _currentLayout;

    internal ClayContext(ClayArenaMemoryHandle arena) => _arenaHandle = arena;

    public void Dispose()
    {
        _currentLayout?.Dispose();
        _arenaHandle.Dispose();
    }

    /// <summary>Updates Clay's internal dimensions. Should match the dimensions of the render target</summary>
    public void UpdateDimensions(ClayDimensions dimensions) 
        => Clay.UpdateDimensions(dimensions);
    
    /// <summary>Updates Clay's internal mouse state information. Needed for interactive UI</summary>
    public void UpdateMouseState(ClayVector2 cursorPosition, bool isMouseDown) 
        => Clay.UpdateMouseState(cursorPosition, isMouseDown);

    public void UpdateScrollContainers(bool enableDragScrolling, ClayVector2 mouseWheelDelta, float deltaTime)
        => Clay.UpdateScrollContainers(enableDragScrolling, mouseWheelDelta, deltaTime);

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
    public static ClayContext Create(ClayDimensions dimensions, MeasureTextFunction measureTextFunction, ErrorHandlerFunction? errorHandlerCallback = null)
        => Clay.Initialize(dimensions, measureTextFunction, errorHandlerCallback);
}