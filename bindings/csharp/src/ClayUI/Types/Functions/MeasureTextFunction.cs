using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

using Clay.Types.Element;
using Clay.Types.Internal;
using Clay.Types.Internal.Interop;

namespace Clay.Types.Functions;

/// <summary>Measure text function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayDimensions MeasureTextFunction([MarshalUsing(typeof(ClayStringToRefClrStringMarshaller))] ref string text, ref TextElementConfig config);