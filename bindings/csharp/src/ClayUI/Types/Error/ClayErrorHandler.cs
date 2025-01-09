using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Clay.Types.Error;

/// <summary>Used to set a managed function as the error handler function within Clay. Supports setting additional
/// arbitrary data via <see cref="UserData"/></summary>
internal struct ClayErrorHandler
{
    /// <summary>Get or set the managed delegate used in the unmanaged library</summary>
    public ErrorHandlerFunction? ErrorHandlerFunction
    {
        get => _errorHandlerFunctionPointer != IntPtr.Zero ?
                   Marshal.GetDelegateForFunctionPointer<ErrorHandlerFunction>(_errorHandlerFunctionPointer) :
                   null;
        set => _errorHandlerFunctionPointer = value is not null ?
                  Marshal.GetFunctionPointerForDelegate(value) :
                  Marshal.GetFunctionPointerForDelegate(DefaultErrorHandlerFunction);
    }
    private IntPtr  _errorHandlerFunctionPointer;
    
    /// <summary>Custom, arbitrary user data provided to <see cref="ErrorHandlerFunction"/> via <see cref="ErrorData"/></summary>
    public  UIntPtr UserData             { get; set; }

    /// <summary>Throws a <see cref="ClayException"/> when called using <paramref name="errorData"/></summary>
    /// <exception cref="ClayException">Always thrown when called</exception>
    [DoesNotReturn]
    private static void DefaultErrorHandlerFunction(ErrorData errorData) 
        => throw new ClayException(errorData);
}