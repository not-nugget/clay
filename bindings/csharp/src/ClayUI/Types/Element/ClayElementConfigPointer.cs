using System.Runtime.CompilerServices;

namespace Clay.Types.Element;

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