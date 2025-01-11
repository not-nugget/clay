namespace Clay.Types.Internal;

/// <summary>Interop type used by Clay for all things Text related</summary>
internal struct ClayString
{
    public int    Length { get; internal set; }
    public IntPtr Chars  { get; internal set; }
}