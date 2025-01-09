using System.Runtime.InteropServices;

using Clay.Types.Error;

namespace Clay;

/// <summary>On element hover handler function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnHoverFunctionHandler(ClayElementId elementId, ClayPointerData pointerData, IntPtr userData);

/// <summary>Measure text function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayDimensions MeasureTextFunction(ref ClayString text, ref ClayTextElementConfig config);

/// <summary>Query scroll offset function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayVector2 QueryScrollOffsetFunction(uint elementId);

/// <summary>Exposes the internal bindings to Clay for use in <see cref="ClayContext" /></summary>
internal static partial class Clay
{
    /// <summary>Initialize a new <see cref="ClayContext" /></summary>
    /// <param name="dimensions">Initial dimensions of the created window</param>
    /// <param name="errorHandlerCallback">Optional error handler callback delegate to be called if Clay encounters an error</param>
    /// <returns><see cref="ClayContext" /> instance for use in creating your Clay UI</returns>
    internal static ClayContext Initialize(ClayDimensions dimensions, ErrorHandlerFunction? errorHandlerCallback)
    {
        ClayContext? context;
        try
        {
            var memorySize        = Clay_MinMemorySize();
            var alignedMemorySize = (nuint)memorySize;

            IntPtr memory;
            unsafe
            {
                memory = new IntPtr(NativeMemory.AllocZeroed(alignedMemorySize));
            }

            var errorHandler = new ClayErrorHandler();
            if (errorHandlerCallback != null)
                errorHandler.ErrorHandlerFunction = errorHandlerCallback;

            var arena = Clay_CreateArenaWithCapacityAndMemory(memorySize, memory);
            Clay_Initialize(arena, dimensions, errorHandler);
            context = new ClayContext(arena);
        }
        catch (Exception e)
        {
            throw new ApplicationException("An error occurred while initializing Clay. See InnerException for details", e);
        }

        return context;
    }

    /// <summary>Start a new Clay layout</summary>
    internal static void BeginLayout()
        => Clay_BeginLayout();

    /// <summary>End the current Clay layout</summary>
    /// <returns>A set of commands which may be provided to any renderer of choice to render the Clay UI</returns>
    internal static ClayRenderCommand[] EndLayout()
    {
        var nativeArray    = Clay_EndLayout();
        var renderCommands = new ClayRenderCommand[nativeArray.Length];

        var    commandSize = Marshal.SizeOf<ClayRenderCommand>();
        IntPtr sourceArrayPtr;

        unsafe
        {
            sourceArrayPtr = new IntPtr(nativeArray.InternalArray);
        }

        for (var i = 0; i < nativeArray.Length; i++)
        {
            var commandPtr = new IntPtr(sourceArrayPtr + i * commandSize);
            renderCommands[i] = Marshal.PtrToStructure<ClayRenderCommand>(commandPtr);
        }

        return renderCommands;
    }

    #region 1:1 Bindings
    [LibraryImport("clay.dll")]
    private static partial uint Clay_MinMemorySize();

    [LibraryImport("clay.dll")]
    private static partial ClayArena Clay_CreateArenaWithCapacityAndMemory(uint capacity, IntPtr offset);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetPointerState(ClayVector2 position, [MarshalAs(UnmanagedType.Bool)] bool pointerDown);

    [LibraryImport("clay.dll")]
    private static partial void Clay_Initialize(ClayArena arena, ClayDimensions dimensions, ClayErrorHandler errorHandler);

    [LibraryImport("clay.dll")]
    private static partial void Clay_UpdateScrollContainers([MarshalAs(UnmanagedType.Bool)] bool enableDragScrolling, ClayVector2 scrollDelta, float deltaTime);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetLayoutDimensions(ClayDimensions dimensions);

    [LibraryImport("clay.dll")]
    private static partial void Clay_BeginLayout();

    [LibraryImport("clay.dll")]
    private static partial ClayRenderCommandArray Clay_EndLayout();

    [LibraryImport("clay.dll")]
    private static partial ClayElementId Clay_GetElementId(ClayString idString);

    [LibraryImport("clay.dll")]
    private static partial ClayElementId Clay_GetElementIdWithIndex(ClayString idString, uint index);

    [LibraryImport("clay.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool ClayHovered();

    [LibraryImport("clay.dll")]
    private static partial void Clay_OnHover(OnHoverFunctionHandler onHoverFunctionHandler, IntPtr userData);

    [LibraryImport("clay.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Clay_PointerOver(ClayElementId elementId);

    //TODO custom marshaller for ClayScrollContainerData
    [DllImport("clay.dll")]
    private static extern ClayScrollContainerData Clay_GetScrollContainerData(ClayElementId id);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetMeasureTextFunction(MeasureTextFunction measureTextFunction);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetQueryScrollOffsetFunction(QueryScrollOffsetFunction queryScrollOffsetFunction);

    // Accessing each index of the array can be done in managed memory and does not need to interop
    // [LibraryImport("clay.dll")]
    // private static partial Clay_RenderCommand *     Clay_RenderCommandArray_Get(Clay_RenderCommandArray* array, int32_t index);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetDebugModeEnabled([MarshalAs(UnmanagedType.Bool)] bool enabled);

    [LibraryImport("clay.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool Clay_IsDebugModeEnabled();

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetCullingEnabled([MarshalAs(UnmanagedType.Bool)] bool enabled);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetMaxElementCount(int maxElementCount);

    [LibraryImport("clay.dll")]
    private static partial void Clay_SetMaxMeasureTextCacheWordCount(int maxMeasureTextCacheWordCount);

    [LibraryImport("clay.dll")]
    private static partial void Clay__OpenElement();

    [LibraryImport("clay.dll")]
    private static partial void Clay__CloseElement();

    [DllImport("clay.dll")]
    private static extern ref ClayLayoutConfig Clay__StoreLayoutConfig(ClayLayoutConfig config);

    [LibraryImport("clay.dll")]
    private static partial void Clay__ElementPostConfiguration();

    [LibraryImport("clay.dll")]
    private static partial void Clay__AttachId(ClayElementId id);

    [LibraryImport("clay.dll")]
    private static partial void Clay__AttachLayoutConfig(ref ClayLayoutConfig config);

    [DllImport("clay.dll")]
    private static extern void Clay__AttachElementConfig(ClayElementConfigUnion config, ClayElementConfigType type);

    [DllImport("clay.dll")]
    private static extern ref ClayRectangleElementConfig Clay__StoreRectangleElementConfig(ClayRectangleElementConfig config);

    [DllImport("clay.dll")]
    private static extern ref ClayTextElementConfig Clay__StoreTextElementConfig(ClayTextElementConfig config);

    [DllImport("clay.dll")]
    private static extern ref ClayImageElementConfig Clay__StoreImageElementConfig(ClayImageElementConfig config);

    [DllImport("clay.dll")]
    private static extern ref ClayFloatingElementConfig Clay__StoreFloatingElementConfig(ClayFloatingElementConfig config);

    // Not currently supporting custom element config
    // [LibraryImport("clay.dll")]
    // private static partial Clay_CustomElementConfig *    Clay__StoreCustomElementConfig(Clay_CustomElementConfig       config);

    [DllImport("clay.dll")]
    private static extern ref ClayScrollElementConfig Clay__StoreScrollElementConfig(ClayScrollElementConfig config);

    [DllImport("clay.dll")]
    private static extern ref ClayBorderElementConfig Clay__StoreBorderElementConfig(ClayBorderElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial ClayElementId Clay__HashString(ClayString key, uint offset, uint seed);

    [LibraryImport("clay.dll")]
    private static partial void Clay__OpenTextElement(ClayString text, ref ClayTextElementConfig textConfig);

    [LibraryImport("clay.dll")]
    private static partial uint Clay__GetParentElementId();
    #endregion
}

public struct ClayString
{
    //TODO Clay_String is not a private type, but it should be marshalled to/from a string or ReadOnlySpan<char> in all user code for convinience
    internal int    length;
    internal IntPtr chars; //TODO SafeHandle, or unsafe ClayString*, or alternative layout?
}

internal struct ClayStringArray
{
    //TODO Clay_StringArray is a private type and therefore should not ever be user-accessible
    private int    capacity;
    private int    length;
    private IntPtr array; //TODO SafeHandle, or unsafe ClayString*, or alternative layout?
}

internal unsafe struct ClayArena : IDisposable
{
    //TODO Clay_Arena is not a private type, but the user should not have to worry about passing around their own arena. Instead, a ClayContext class should be created and is responsible for all unmanaged CLAY resources
    private nuint nextAllocation;
    private int   capacity;
    private void* memory; //TODO SafeHandle, or unsafe ClayString*, or alternative layout?

    public readonly void Dispose()
    {
        if (memory is not null)
            //TODO maybe this shouldn't be done? Much more research needs to be done with this, as it is very possible that clay will always correctly handle the disposal of the arena
            NativeMemory.Free(memory);
    }
}

public struct ClayDimensions
{
    public float Width  { get; set; }
    public float Height { get; set; }
}

public struct ClayVector2
{
    //TODO users should only ever deal with System.Numerics.Vector2 instead of the clay marshalled intermediate here
    public float X { get; set; }
    public float Y { get; set; }
}

public struct ClayColor
{
    //TODO the BCL has System.Drawing.Color, but more research should be conducted if this is to be used, and it cannot be directly marshaled, as it is in argb format, whereas clay is formatted as rgba
    public float R { get; set; }
    public float G { get; set; }
    public float B { get; set; }
    public float A { get; set; }
}

internal struct ClayBoundingBox
{
    //TODO is there a Bounds in the BCL? if not, this will suffice
    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }
}

//TODO the front-facing elementID should just be a string, as it is going to be a one-dimensional interop (C# to Clay). We should use an intermediate interop struct for the incoming raw bytes that just get converted to System.String, as we never have a need for the Clay formatting of the element id
public struct ClayElementId
{
    //TODO there is some logic in clay with what an element id is. this should be the public facing id that is shown to the user, the rest should be private by design
    private uint       id;
    private uint       offset;
    private uint       baseId;
    private ClayString stringId;
}

/// <summary>TODO</summary>
internal struct CornerRadius
{
    public  float TopLeft     { get; set; }
    private float TopRight    { get; set; }
    private float BottomLeft  { get; set; }
    private float BottomRight { get; set; }
}

[Flags]
internal enum ClayElementConfigType : byte
{
    None              = 0,
    Rectangle         = 1,
    BorderContainer   = 2,
    FloatingContainer = 4,
    ScrollContainer   = 8,
    Image             = 16,
    Text              = 32,
    Custom            = 64,
}

//TODO update names and add xmldoc comments to reflect the changes 
public enum ClayLayoutDirection : byte
{
    //TODO update names and add xmldoc comments to reflect the changes
    CLAY_LEFT_TO_RIGHT,
    CLAY_TOP_TO_BOTTOM,
}

//TODO update names and add xmldoc comments to reflect the changes
public enum ClayLayoutAlignmentX : byte
{
    //TODO update names and add xmldoc comments to reflect the changes
    CLAY_ALIGN_X_LEFT,
    CLAY_ALIGN_X_RIGHT,
    CLAY_ALIGN_X_CENTER,
}

//TODO update names and add xmldoc comments to reflect the changes
public enum ClayLayoutAlignmentY
{
    //TODO update names and add xmldoc comments to reflect the changes
    CLAY_ALIGN_Y_TOP,
    CLAY_ALIGN_Y_BOTTOM,
    CLAY_ALIGN_Y_CENTER,
}

//TODO this type needs to be understood better
public enum ClaySizingType
{
    CLAY__SIZING_TYPE_FIT,
    CLAY__SIZING_TYPE_GROW,
    CLAY__SIZING_TYPE_PERCENT,
    CLAY__SIZING_TYPE_FIXED,
}

public struct ClayChildAlignment
{
    public ClayLayoutAlignmentX X { get; set; }
    public ClayLayoutAlignmentY Y { get; set; }
}

public struct ClaySizingMinMax
{
    public float Min { get; set; }
    public float Max { get; set; }
}

//TODO this type is a union found within Clay_SizingAxis and is necessary for marshalling
public struct ClaySize
{
    public ClaySizingMinMax MinMax  { get; set; }
    public float            Percent { get; set; }
}

public struct ClaySizingAxis
{
    public ClaySize       Size { get; set; }
    public ClaySizingType Type { get; set; }
}

public struct ClaySizing
{
    public ClaySizingAxis Width  { get; set; }
    public ClaySizingAxis Height { get; set; }
}

public struct ClayPadding
{
    public ushort X { get; set; }
    public ushort Y { get; set; }
}

public struct ClayLayoutConfig
{
    public ClaySizing          Sizing          { get; set; }
    public ClayPadding         Padding         { get; set; }
    public ushort              ChildGap        { get; set; }
    public ClayChildAlignment  ChildAlignment  { get; set; }
    public ClayLayoutDirection LayoutDirection { get; set; }
}

internal struct ClayRectangleElementConfig
{
    public ClayColor    Color        { get; set; }
    public CornerRadius CornerRadius { get; set; }

    //TODO Clay has default functionality for extending the default rectangle element configuration with custom data. there is no real easy way to do this, and 99% of users won't need this, but if it becomes a requested feature, this binding extension should be able to support extension of the rectangle element. do note that this would indicate editing and recompiling the original header file with additional define constraints, as well as a custom dll import, so this might not be a trivial task
}

public enum ClayTextElementConfigWrapMode : byte
{
    CLAY_TEXT_WRAP_WORDS,
    CLAY_TEXT_WRAP_NEWLINES,
    CLAY_TEXT_WRAP_NONE,
}

public struct ClayTextElementConfig
{
    public ClayColor                     TextColor     { get; set; }
    public ushort                        FontId        { get; set; } //TODO helper FontAsset
    public ushort                        FontSize      { get; set; }
    public ushort                        LetterSpacing { get; set; }
    public ushort                        LineHeight    { get; set; }
    public ClayTextElementConfigWrapMode WrapMode      { get; set; }

    //TODO Read rectangle element above for information about element data extension
}

internal struct ClayImageElementConfig
{
    public IntPtr         ImageData        { get; set; }
    public ClayDimensions SourceDimensions { get; set; }

    //TODO Read rectangle element above for information about element data extension
}

internal enum ClayFloatingAttachPointType : byte
{
    CLAY_ATTACH_POINT_LEFT_TOP,
    CLAY_ATTACH_POINT_LEFT_CENTER,
    CLAY_ATTACH_POINT_LEFT_BOTTOM,
    CLAY_ATTACH_POINT_CENTER_TOP,
    CLAY_ATTACH_POINT_CENTER_CENTER,
    CLAY_ATTACH_POINT_CENTER_BOTTOM,
    CLAY_ATTACH_POINT_RIGHT_TOP,
    CLAY_ATTACH_POINT_RIGHT_CENTER,
    CLAY_ATTACH_POINT_RIGHT_BOTTOM,
}

internal struct ClayFloatingAttachPoints
{
    public ClayFloatingAttachPointType Element { get; set; }
    public ClayFloatingAttachPointType Parent  { get; set; }
}

internal enum ClayPointerCaptureMode //TODO NOTE: Once this is implemented, this can cause some damage because Passthrough will be increased by 1...
{
    CLAY_POINTER_CAPTURE_MODE_CAPTURE,

    //CLAY_POINTER_CAPTURE_MODE_PARENT, TODO not yet implemented in clay.h
    CLAY_POINTER_CAPTURE_MODE_PASSTHROUGH,
}

internal struct ClayFloatingElementConfig
{
    public ClayVector2              Offset             { get; set; }
    public ClayDimensions           Expand             { get; set; }
    public ushort                   ZIndex             { get; set; }
    public uint                     ParentId           { get; set; }
    public ClayFloatingAttachPoints Attachment         { get; set; }
    public ClayPointerCaptureMode   PointerCaptureMode { get; set; }
}

//TODO CustomElementConfig (Line 372 at time of writing)

public struct ClayScrollElementConfig
{
    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Horizontal { get; set; }

    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Vertical { get; set; }
}

internal struct ClayBorder
{
    public uint      Width { get; set; }
    public ClayColor Color { get; set; }
}

internal struct ClayBorderElementConfig
{
    public ClayBorder   Left            { get; set; }
    public ClayBorder   Right           { get; set; }
    public ClayBorder   Top             { get; set; }
    public ClayBorder   Bottom          { get; set; }
    public ClayBorder   BetweenChildren { get; set; }
    public CornerRadius CornerRadius    { get; set; }
}

// TODO the union members are pointers in Clay, not sure how this translates...may need to be unsafe and with actual pointers 
[StructLayout(LayoutKind.Explicit)]
public struct ClayElementConfigUnion
{
    [FieldOffset(0)]
    private ClayRectangleElementConfig rectangleElementConfig;

    [FieldOffset(0)]
    private ClayTextElementConfig textElementConfig;

    [FieldOffset(0)]
    private ClayImageElementConfig imageElementConfig;

    [FieldOffset(0)]
    private ClayFloatingElementConfig floatingElementConfig;

    [FieldOffset(0)]
    private ClayScrollElementConfig scrollElementConfig;

    [FieldOffset(0)]
    private ClayBorderElementConfig borderElementConfig;
}

/// <summary>Represents a union of each individual element config type</summary>
internal struct ClayElementConfig
{
    private ClayElementConfigType Type   { get; set; }
    public  IntPtr                Config { get; set; }
}

public struct ClayScrollContainerData
{
    public  ClayVector2 ScrollPosition => Marshal.PtrToStructure<ClayVector2>(_scrollPosition);
    private IntPtr      _scrollPosition;

    public ClayDimensions          ScrollContainerDimensions { get; set; }
    public ClayDimensions          ContentDimensions         { get; set; }
    public ClayScrollElementConfig Config                    { get; set; }

    [field: MarshalAs(UnmanagedType.Bool)]
    public bool Found { get; set; }
}

public enum ClayPointerDataInteractionState : byte
{
    CLAY_POINTER_DATA_PRESSED_THIS_FRAME,
    CLAY_POINTER_DATA_PRESSED,
    CLAY_POINTER_DATA_RELEASED_THIS_FRAME,
    CLAY_POINTER_DATA_RELEASED,
}

public struct ClayPointerData
{
    public ClayVector2                     Position { get; set; }
    public ClayPointerDataInteractionState State    { get; set; }
}

public enum ClayRenderCommandType : byte
{
    None,
    Rectangle,
    Border,
    Text,
    Image,
    ScissorStart,
    ScissorEnd,
    Custom,
}

public struct ClayRenderCommand
{
    private ClayBoundingBox        boundingBox;
    private ClayElementConfigUnion config;
    private ClayString             text; // TODO I wish there was a way to avoid having to have this on every render command
    private uint                   id;
    private ClayRenderCommandType  commandType;
}

internal struct ClayRenderCommandArray
{
    public int Capacity { get; private set; }

    public int Length { get; private set; }

    public unsafe void* InternalArray { get; private set; }
}