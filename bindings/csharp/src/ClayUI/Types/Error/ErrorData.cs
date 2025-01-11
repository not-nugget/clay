using System.Runtime.InteropServices;

namespace Clay.Types.Error;

/// <summary>Data sent to the error function handler when Clay encounters an internal error</summary>
public struct ErrorData
{
    /// <summary>Type of error encountered by Clay</summary>
    public ErrorType ErrorType { get; private set; }

    //TODO Test, this is probably very volatile :)
    /// <summary>String contents of the error message written by Clay</summary>
    public string ErrorText => Marshal.PtrToStringUTF8(_errorText.Chars, _errorText.Length);

    #pragma warning disable CS0649 // Field is never assigned to, and will always have its default value - Data is assigned when marshaled
    private ClayString _errorText;
    #pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    /// <summary>Points to the raw user data value set when assigning the error function handler</summary>
    public UIntPtr UserData { get; private set; }
}