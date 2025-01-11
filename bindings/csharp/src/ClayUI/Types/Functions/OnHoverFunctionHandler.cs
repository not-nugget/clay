using System.Runtime.InteropServices;

using Clay.Types.Element;

namespace Clay.Types.Functions;

/// <summary>On element hover handler function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void OnHoverFunctionHandler(ElementId elementId, ClayPointerData pointerData, IntPtr userData);