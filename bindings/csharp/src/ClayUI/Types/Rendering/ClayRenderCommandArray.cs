namespace Clay.Types.Rendering;

/// <summary>Internal representation of an array of <see cref="ClayRenderCommand"/>s. Should be converted to a .NET Array and never shown to the user</summary>
internal struct ClayRenderCommandArray
{
    public int Capacity { get; private set; }

    public int Length { get; private set; }

    public unsafe void* InternalArray { get; private set; }
}