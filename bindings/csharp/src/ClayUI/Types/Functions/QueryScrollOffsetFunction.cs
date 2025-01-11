using System.Runtime.InteropServices;

using Clay.Types.Element;

namespace Clay.Types.Functions;

/// <summary>Query scroll offset function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate ClayVector2 QueryScrollOffsetFunction(uint elementId);