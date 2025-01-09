using System.Runtime.InteropServices;

namespace Clay.Types.Error;

/// <summary>Error handler function signature which will be called by unmanaged code</summary>
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate void ErrorHandlerFunction(ErrorData errorText);