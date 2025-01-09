using System.Runtime.InteropServices;

namespace Clay.Types.Error;

/// <summary>Data sent to the error function handler when Clay encounters an internal error</summary>
public struct ErrorData
{
    /// <summary>Type of error encountered by Clay</summary>
    public ErrorType ErrorType { get; private set; }

    //TODO Test, this is probably very volatile :)
    /// <summary>String contents of the error message written by Clay</summary>
    public string ErrorText => Marshal.PtrToStringUTF8(_errorText.chars, _errorText.length);

    private ClayString _errorText;

    /// <summary>Points to the raw user data value set when assigning the error function handler</summary>
    public UIntPtr UserData { get; private set; }
}