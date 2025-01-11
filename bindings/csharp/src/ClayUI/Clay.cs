using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Clay.Types.Error;

namespace Clay;

/// <summary>On element hover handler function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnHoverFunctionHandler(ElementId elementId, ClayPointerData pointerData, IntPtr userData);

/// <summary>Measure text function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayDimensions MeasureTextFunction([MarshalUsing(typeof(ClayStringToRefClrStringMarshaller))] ref string text, ref TextElementConfig config);

/// <summary>Query scroll offset function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayVector2 QueryScrollOffsetFunction(uint elementId);

[CustomMarshaller(typeof(string), MarshalMode.UnmanagedToManagedRef, typeof(ClayStringToRefClrStringMarshaller))]
internal static unsafe class ClayStringToRefClrStringMarshaller
{
    public static string ConvertToManaged(nint unmanaged)
    {
        var clayString = Marshal.PtrToStructure<ClayString>(unmanaged);
        return Marshal.PtrToStringUTF8(clayString.Chars, clayString.Length);
    }

    public static nint ConvertToUnmanaged(string managed)
    {
        var clayStr = new ClayString
        {
            Length = managed.Length,
            Chars  = Marshal.StringToCoTaskMemUTF8(managed),
        };
        return new IntPtr(Unsafe.AsPointer(ref clayStr));
    }
}

[CustomMarshaller(typeof(ScrollElementConfig), MarshalMode.ManagedToUnmanagedIn, typeof(ScrollElementConfigMarshaller))]
public static unsafe class ScrollElementConfigMarshaller
{
    private readonly ref struct ScrollElementConfigInterop(byte bHorizontal, byte bVertical)
    {
        private readonly byte _bHorizontal = bHorizontal;
        private readonly byte _bVertical   = bVertical;
    }
        
    public static nint ConvertToUnmanaged(ScrollElementConfig managed)
    {
        var bHorizontal = (byte)(managed.Horizontal ? 1 : 0);
        var bVertical = (byte)(managed.Vertical ? 1 : 0);
        var  interop     = new ScrollElementConfigInterop(bHorizontal, bVertical);
        return (nint)Unsafe.AsPointer(ref interop);
    }
}

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

            //TODO TEMPORARY
            errorHandler.ErrorHandlerFunction = Temporary__ClayError;
            // if (errorHandlerCallback != null)
            //     errorHandler.ErrorHandlerFunction = errorHandlerCallback;

            var arena = Clay_CreateArenaWithCapacityAndMemory(memorySize, memory);
            Clay_Initialize(arena, dimensions, errorHandler);

            Clay_SetMeasureTextFunction(Temporary__MeasureText); //TODO temporary
            context = new ClayContext(arena);
        }
        catch (Exception e)
        {
            throw new ApplicationException("An error occurred while initializing Clay. See InnerException for details", e);
        }

        return context;
    }

    internal static void Temporary__ClayError(ErrorData errorData) { Console.Error.WriteLine($"Clay encountered an error {errorData.ErrorType}:{errorData.ErrorText}"); }

    internal static ClayDimensions Temporary__MeasureText(ref string text, ref TextElementConfig config)
    {
        Console.WriteLine($"Clay calling MeasureText: {text};");
        return new ClayDimensions()
        {
            Height = 10,
            Width  = 10,
        };
    }

    /// <summary>Start a new Clay layout</summary>
    internal static void BeginLayout()
        => Clay_BeginLayout();

    /// <summary>End the current Clay layout</summary>
    /// <returns>A set of commands which may be provided to any renderer of choice to render the Clay UI</returns>
    internal static ClayRenderCommand[] EndLayout()
    {
        var nativeArray    = Clay_EndLayout();
        var renderCommands = new ClayRenderCommand[nativeArray.Length]; //TODO array pooling?

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

    /// <summary>Opens a new Clay element</summary>
    internal static void OpenElement()
        => Clay__OpenElement();

    public static void OpenTextElement(string text, ref TextElementConfig config)
    {
        var clayString = new ClayString
        {
            Length = text.Length,
        };

        unsafe
        {
            fixed (char* stringPtr = text)
            {
                clayString.Chars = new IntPtr(&stringPtr[0]);
            }
        }

        try
        {
            Clay__OpenTextElement(clayString, ref config);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            throw;
        }
    }

    internal static void AttachId(ref ElementId elementId)
    {
        // TODO this will prevent a fatal error, BUT it will cause some issues if ever one tries comparing two ElementHandles, as their IDs will not be set correctly. I do not know if i currently have a way to get the current element's ID generated by clay, or get the current element's data to fetch the ID
        if (string.IsNullOrWhiteSpace(elementId.Value))
            return; // Do nothing if the ID is unset and let Clay generate the ID

        var seed = Clay__GetParentElementId();
        var i    = elementId.Offset;
        var clayString = new ClayString
        {
            Length = elementId.Value.Length,
        };

        unsafe
        {
            fixed (char* stringPtr = elementId.Value)
            {
                clayString.Chars = new IntPtr(&stringPtr[0]);
            }
        }

        var unmanagedElementId = Clay__HashString(clayString, i, seed);
        Clay__AttachId(unmanagedElementId);
        elementId = new ElementId
        {
            Value  = Marshal.PtrToStringUTF8(unmanagedElementId.StringId.Chars, unmanagedElementId.StringId.Length),
            Offset = unmanagedElementId.Offset,
            BaseId = unmanagedElementId.BaseId,
            IdHash = unmanagedElementId.Id,
        };
    }

    internal static void AttachLayoutConfig(LayoutConfig layout)
    {
        var          layoutConfigPtr = Clay__StoreLayoutConfig(layout);
        LayoutConfig layoutConfigRef;
        unsafe
        {
            layoutConfigRef = Unsafe.AsRef<LayoutConfig>((void*)layoutConfigPtr);
        }

        Clay__AttachLayoutConfig(ref layoutConfigRef);
    }

    internal static void AttachElementConfig(ClayElementConfigPointer configPointer)
    {
        IntPtr arenaPtr;
        switch (configPointer.Type)
        {
            case ClayElementConfigType.Rectangle:
                var rectangle = Marshal.PtrToStructure<RectangleElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreRectangleElementConfig(rectangle);
                break;
            case ClayElementConfigType.BorderContainer:
                var border = Marshal.PtrToStructure<BorderElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreBorderElementConfig(border);
                break;
            case ClayElementConfigType.FloatingContainer:
                var floating = Marshal.PtrToStructure<FloatingElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreFloatingElementConfig(floating);
                break;
            case ClayElementConfigType.ScrollContainer:
                var scroll = Marshal.PtrToStructure<ScrollElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreScrollElementConfig(scroll);
                break;
            case ClayElementConfigType.Image:
                var image = Marshal.PtrToStructure<ImageElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreImageElementConfig(image);
                break;
            case ClayElementConfigType.Text:
                var text = Marshal.PtrToStructure<TextElementConfig>(configPointer.ConfigPointer);
                arenaPtr = Clay__StoreTextElementConfig(text);
                break;
            case ClayElementConfigType.None:
                // Don't save or attach anything when not configuring an element
                return;
            case ClayElementConfigType.Custom:
                throw new NotImplementedException("Custom element configuration extensions are not yet supported");
            default:
                throw new InvalidOperationException("Attempted to attach and unknown or unsupported element config type");
        }
        
        Clay__AttachElementConfig((ClayElementConfigurationUnion)arenaPtr, configPointer.Type);
    }

    /// <summary>If an element is currently open, the pending set configurations are applied</summary>
    internal static void PostConfigureOpenElement()
        => Clay__ElementPostConfiguration();

    /// <summary>Closes the currently open Clay element</summary>
    internal static void CloseElement()
        => Clay__CloseElement();

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

    /// <returns><see cref="IntPtr"/> that points to a Clay managed instance of <see cref="LayoutConfig"/>(<paramref name="config"/>)</returns>
    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreLayoutConfig(LayoutConfig config);

    [LibraryImport("clay.dll")]
    private static partial void Clay__ElementPostConfiguration();

    [LibraryImport("clay.dll")]
    private static partial void Clay__AttachId(ClayElementId id);

    [LibraryImport("clay.dll")]
    private static partial void Clay__AttachLayoutConfig(ref LayoutConfig config);

    [LibraryImport("clay.dll")]
    private static partial void Clay__AttachElementConfig(ClayElementConfigurationUnion configuration, ClayElementConfigType type);

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreRectangleElementConfig(RectangleElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreTextElementConfig(TextElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreImageElementConfig(ImageElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreFloatingElementConfig(FloatingElementConfig config);

    // Not currently supporting custom element config
    // [LibraryImport("clay.dll")]
    // private static partial Clay_CustomElementConfig *    Clay__StoreCustomElementConfig(Clay_CustomElementConfig       config);

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreScrollElementConfig([MarshalUsing(typeof(ScrollElementConfigMarshaller))] ScrollElementConfig config);
    
    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__StoreBorderElementConfig(BorderElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial ClayElementId Clay__HashString(ClayString key, uint offset, uint seed);

    //TODO This just isn't working and I have no idea why. I'm marshalling ClayString when I set an element's ID just fine, but here it explodes
    [LibraryImport("clay.dll")]
    private static partial void Clay__OpenTextElement(ClayString content, ref TextElementConfig config);

    [LibraryImport("clay.dll")]
    private static partial uint Clay__GetParentElementId();

    [LibraryImport("clay.dll")]
    private static partial IntPtr Clay__GetOpenLayoutElement();
    #endregion
}

/// <summary>Interop type used by Clay for all things Text related</summary>
internal struct ClayString
{
    public int    Length { get; internal set; }
    public IntPtr Chars  { get; internal set; }
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

/// <summary>Represents a 4-channel color, stored in RGBA format</summary>
public struct ClayColor
{
    /// <summary>Red channel</summary>
    public float R { get; set; }

    /// <summary>Green channel</summary>
    public float G { get; set; }

    /// <summary>Blue channel</summary>
    public float B { get; set; }

    /// <summary>Alpha channel</summary>
    public float A { get; set; }

    public static implicit operator ClayColor(float                        spread) => new ClayColor { R = spread, G      = spread, B      = spread, A      = 255f };
    public static implicit operator ClayColor((float, float, float)        tuple)  => new ClayColor { R = tuple.Item1, G = tuple.Item2, B = tuple.Item3, A = 255f };
    public static implicit operator ClayColor((float, float, float, float) tuple)  => new ClayColor { R = tuple.Item1, G = tuple.Item2, B = tuple.Item3, A = tuple.Item4 };
}

public struct ClayBoundingBox
{
    public float X      { get; set; }
    public float Y      { get; set; }
    public float Width  { get; set; }
    public float Height { get; set; }
}

/// <summary>Uniquely identifies an element within Clay. Supports offset IDs (within Clay) to avoid generating many dynamic IDs at runtime</summary>
/// <param name="value">Unique element identifier</param>
/// <param name="offset">Unique element offset</param>
public readonly struct ElementId(string value, uint offset = 0u)
{
    /// <summary>Managed string representation of an element's ID</summary>
    public string Value { get; init; } = value;

    /// <summary>Offset index of an element's ID</summary>
    public uint Offset { get; init; } = offset;

    /// <summary>Clay-assigned BaseId (used with offset IDs)</summary>
    public uint? BaseId { get; internal init; }

    /// <summary>Clay-generated unique hash</summary>
    public uint? IdHash { get; internal init; }

    public static implicit operator ElementId(string         id)    => new ElementId(id);
    public static implicit operator ElementId((string, uint) tuple) => new ElementId(tuple.Item1, tuple.Item2);
}

/// <summary>Clay_ElementId Interop structure</summary>
internal struct ClayElementId
{
    #pragma warning disable CS0649 // Field is never assigned to, and will always have its default value - Field is assigned in unmanaged code
    internal uint       Id;
    internal uint       Offset;
    internal uint       BaseId;
    internal ClayString StringId;
    #pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
}

/// <summary>Specifies the radius of the edges of a rectangular element</summary>
public struct CornerRadius
{
    public  float TopLeft     { get; set; }
    private float TopRight    { get; set; }
    private float BottomLeft  { get; set; }
    private float BottomRight { get; set; }

    public static implicit operator CornerRadius(float                        radii) => new CornerRadius { TopLeft = radii, TopRight       = radii, BottomLeft       = radii, BottomRight       = radii };
    public static implicit operator CornerRadius((float, float, float, float) tuple) => new CornerRadius { TopLeft = tuple.Item1, TopRight = tuple.Item2, BottomLeft = tuple.Item3, BottomRight = tuple.Item4 };
}

[Flags]
public enum ClayElementConfigType : byte
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

/// <summary>Controls the default layout direction of an element's children</summary>
public enum LayoutDirection : byte
{
    /// <summary>Elements are placed starting from the left and continuing to the right</summary>
    LeftToRight,

    /// <summary>Elements are placed at the top and continuing to the bottom</summary>
    TopToBottom,
}

//TODO update names and add xmldoc comments to reflect the changes
/// <summary>Controls the horizontal alignment of an element's children</summary>
public enum LayoutAlignmentX : byte
{
    /// <summary>Child elements are horizontally aligned to the left of this element</summary>
    Left,

    /// <summary>Child elements are horizontally aligned to the right of this element</summary>
    Right,

    /// <summary>Child elements are horizontally aligned in the middle of this element</summary>
    Center,
}

//TODO update names and add xmldoc comments to reflect the changes
/// <summary>Controls the vertical alignment of an element's children</summary>
public enum LayoutAlignmentY
{
    /// <summary>Child elements are vertically aligned to the top of this element</summary>
    Top,

    /// <summary>Child elements are vertically aligned to the bottom of this element</summary>
    Bottom,

    /// <summary>Child elements are vertically aligned in the middle of this element</summary>
    Center,
}

/// <summary>Indicates how an element is sized</summary>
public enum SizingType
{
    //TODO verify all comments
    /// <summary>Element is sized to fit its contents</summary>
    Fit,

    /// <summary>Element will grow to fit its container</summary>
    Grow,

    /// <summary>Element is sized by a percentage of its container</summary>
    Percent,

    /// <summary>Element is sized by its fixed minimum and maximum</summary>
    Fixed,
}

/// <summary>Controls the alignment of an element's children</summary>
public struct ChildAlignment
{
    public LayoutAlignmentX X { get; set; }
    public LayoutAlignmentY Y { get; set; }

    public static implicit operator ChildAlignment((LayoutAlignmentX, LayoutAlignmentY) tuple) => new ChildAlignment { X = tuple.Item1, Y = tuple.Item2 };
}

/// <summary>Represents the min and max values of a sizing constraint</summary>
public struct SizingMinMax(float min, float max)
{
    public float Min { get; set; } = min;
    public float Max { get; set; } = max;

    public static implicit operator SizingMinMax((float, float) tuple) => new SizingMinMax(tuple.Item1, tuple.Item2);
}

/// <summary>Size of an element, represented as either a minimum or a maximum (<see cref="MinMax"/>),
/// <em>or</em> a percentage relative to its container (<see cref="Percent"/>)</summary>
[StructLayout(LayoutKind.Explicit)]
public struct Size
{
    [field: FieldOffset(0)]
    public SizingMinMax MinMax { get; set; }

    [field: FieldOffset(0)]
    public float Percent { get; set; }

    public static implicit operator Size(SizingMinMax   minMax) => new Size { MinMax = minMax };
    public static implicit operator Size((float, float) tuple)  => new Size { MinMax = tuple };

    public static implicit operator Size(float percent) => new Size { Percent = percent };
}

/// <summary>Controls sizing along an element's axis</summary>
public struct SizingAxis
{
    public Size       Size { get; set; }
    public SizingType Type { get; set; }

    public static implicit operator SizingAxis(float percent) => new SizingAxis
    {
        Size = new Size { Percent = percent },
        Type = SizingType.Percent,
    };

    public static implicit operator SizingAxis((float, float) tuple) => new SizingAxis
    {
        Size = new Size { MinMax = tuple },
        Type = SizingType.Fixed,
    };

    public static implicit operator SizingAxis(SizingType type) => new SizingAxis { Type = type, };
}

/// <summary>Controls the sizing of an element</summary>
public struct Sizing
{
    public SizingAxis Width  { get; set; }
    public SizingAxis Height { get; set; }

    public static implicit operator Sizing((SizingAxis, SizingAxis) tuple) => new Sizing
    {
        Width  = tuple.Item1,
        Height = tuple.Item2,
    };
}

/// <summary>Controls an element's padding</summary>
public struct Padding
{
    public ushort X { get; set; }
    public ushort Y { get; set; }

    public static implicit operator Padding((ushort, ushort) tuple) => new Padding
    {
        X = tuple.Item1,
        Y = tuple.Item2,
    };
}

//TODO layout config technically is not an element config, so it really shouldn't piggyback off of IElementConfig. Maybe have an outer interface that marks any config that can be used in the type overloads in LayoutHandle, or maybe not...this might be the nitpick of the century
/// <summary>Configures an element's various layout options</summary>
public struct LayoutConfig : IElementConfig
{
    public Sizing          Sizing          { get; set; }
    public Padding         Padding         { get; set; }
    public ushort          ChildGap        { get; set; }
    public ChildAlignment  ChildAlignment  { get; set; }
    public LayoutDirection LayoutDirection { get; set; }
}

public struct RectangleElementConfig : IElementConfig
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

public struct TextElementConfig : IElementConfig
{
    public ClayColor                     TextColor     { get; set; }
    public ushort                        FontId        { get; set; } //TODO helper FontAsset
    public ushort                        FontSize      { get; set; }
    public ushort                        LetterSpacing { get; set; }
    public ushort                        LineHeight    { get; set; }
    public ClayTextElementConfigWrapMode WrapMode      { get; set; }

    //TODO Read rectangle element above for information about element data extension
}

internal struct ImageElementConfig : IElementConfig
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

internal struct FloatingElementConfig : IElementConfig
{
    public ClayVector2              Offset             { get; set; }
    public ClayDimensions           Expand             { get; set; }
    public ushort                   ZIndex             { get; set; }
    public uint                     ParentId           { get; set; }
    public ClayFloatingAttachPoints Attachment         { get; set; }
    public ClayPointerCaptureMode   PointerCaptureMode { get; set; }
}

//TODO CustomElementConfig (Line 372 at time of writing)

public struct ScrollElementConfig : IElementConfig
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

internal struct BorderElementConfig : IElementConfig
{
    public ClayBorder   Left            { get; set; }
    public ClayBorder   Right           { get; set; }
    public ClayBorder   Top             { get; set; }
    public ClayBorder   Bottom          { get; set; }
    public ClayBorder   BetweenChildren { get; set; }
    public CornerRadius CornerRadius    { get; set; }
}

/// <summary>Required for interoperability. Should not be user facing</summary>
internal struct ClayElementConfigurationUnion
{
    // NOTE: there is no need to have all the different pointers, the C union
    // is just a wrapper around single pointer with more type definition information
    public IntPtr ConfigPointer { get; init; }

    public static explicit operator ClayElementConfigurationUnion(IntPtr pointer) => new ClayElementConfigurationUnion { ConfigPointer = pointer };
}

/// <summary>Represents a union of each individual element config type</summary>
internal struct ClayElementConfigPointer
{
    public ClayElementConfigType Type          { get; private set; }
    public IntPtr                ConfigPointer { get; private set; }

    /// <summary>Creates a new <see cref="ClayElementConfigPointer"/> using managed runtime type information</summary>
    internal static ClayElementConfigPointer FromElementConfig<T>(T config) where T : struct, IElementConfig
    {
        var pointer = new ClayElementConfigPointer();

        switch (config)
        {
            case RectangleElementConfig rect:
                pointer.Type = ClayElementConfigType.Rectangle;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref rect));
                }

                break;

            case BorderElementConfig border:
                pointer.Type = ClayElementConfigType.BorderContainer;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref border));
                }

                break;

            case FloatingElementConfig floating:
                pointer.Type = ClayElementConfigType.FloatingContainer;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref floating));
                }

                break;

            case ScrollElementConfig scroll:
                pointer.Type = ClayElementConfigType.ScrollContainer;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref scroll));
                }

                break;

            case ImageElementConfig image:
                pointer.Type = ClayElementConfigType.Image;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref image));
                }

                break;

            case TextElementConfig text:
                pointer.Type = ClayElementConfigType.Text;
                unsafe
                {
                    pointer.ConfigPointer = new IntPtr(Unsafe.AsPointer(ref text));
                }

                break;

            case ClayDefaultElementConfig:
                pointer.Type          = ClayElementConfigType.None;
                pointer.ConfigPointer = IntPtr.Zero;
                break;

            default:
                throw new NotSupportedException("An unsupported IElementConfig was encountered");
        }

        return pointer;
    }

    //TODO not sure if we ever have a need to fetch an element's configuration in the managed runtime
    // /// <summary>Tries to retrieve the element configuration located at <see cref="ConfigPointer"/> typed as <typeparamref name="T"/></summary>
    // /// <param name="config">Typed configuration if the retrieval was successful</param>
    // /// <typeparam name="T">Type to retrieve the configuration as</typeparam>
    // /// <returns><see langword="true"/> if the typed configuration was successfully retrieved, otherwise <see langword="false"/></returns>
    // public bool TryGetConfig<T>(out T? config) where T : struct, IElementConfig
    // {
    //     config = null;
    //     if (_configPtr == IntPtr.Zero || BitOperations.PopCount((byte)Type) > 1)
    //     {
    //         return false;
    //     }
    //
    //     // ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault - Default case will return false, which is intended
    //     switch (Type)
    //     {
    //         case ClayElementConfigType.Rectangle:
    //         case ClayElementConfigType.BorderContainer:
    //         case ClayElementConfigType.FloatingContainer:
    //         case ClayElementConfigType.ScrollContainer:
    //         case ClayElementConfigType.Image:
    //         case ClayElementConfigType.Text:
    //             try
    //             {
    //                 config = Marshal.PtrToStructure<T>(_configPtr);
    //             }
    //             catch
    //             {
    //                 // ignored, false will be returned as intended
    //             }
    //
    //             break;
    //     }
    //
    //     return config is not null;
    // }
}

/// <summary>Marker interface used to ease the process of retrieving a typed element config from <see cref="ClayElementConfigPointer"/></summary>
public interface IElementConfig;

/// <summary>Represents an element with a default configuration</summary>
public readonly struct ClayDefaultElementConfig : IElementConfig;

public struct ClayScrollContainerData
{
    public  ClayVector2 ScrollPosition => Marshal.PtrToStructure<ClayVector2>(_scrollPosition);
    private IntPtr      _scrollPosition;

    public ClayDimensions          ScrollContainerDimensions { get; set; }
    public ClayDimensions          ContentDimensions         { get; set; }
    public ScrollElementConfig Config                    { get; set; }

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

/// <summary>Command that can be used to draw a part of the Clay UI to a window</summary>
public struct ClayRenderCommand
{
    public ClayBoundingBox BoundingBox { get; private set; }

    //public IElementConfig  Config      => null; //TODO do we need the element config in the marshaled runtime?
    private ClayElementConfigPointer _configPointer;

    //public  string     Text => null; //TODO get the string from ClayString, if ClayString is valid
    private ClayString text;

    public uint                  Id          { get; set; }
    public ClayRenderCommandType CommandType { get; set; }
}

/// <summary>Internal representation of an array of <see cref="ClayRenderCommand"/>s. Should be converted to a .NET Array and never shown to the user</summary>
internal struct ClayRenderCommandArray
{
    public int Capacity { get; private set; }

    public int Length { get; private set; }

    public unsafe void* InternalArray { get; private set; }
}