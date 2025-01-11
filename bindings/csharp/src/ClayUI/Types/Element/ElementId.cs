namespace Clay.Types.Element;

/// <summary>Uniquely identifies an element within Clay. Supports offset IDs (within Clay) to avoid generating many dynamic IDs at runtime</summary>
/// <param name="value">Unique element identifier</param>
/// <param name="offset">Unique element offset</param>
public readonly struct ElementId(string value, uint offset = 0u)
{
    /// <summary>Managed string representation of an element's ID</summary>
    public string Value { get; init; } = value;

    /// <summary>Offset index of an element's ID</summary>
    public uint Offset { get; init; } = offset;

    /// <summary>Clay-assigned BaseId (used with offset IDs)</summary>
    public uint? BaseId { get; internal init; }

    /// <summary>Clay-generated unique hash</summary>
    public uint? IdHash { get; internal init; }

    public static implicit operator ElementId(string         id)    => new ElementId(id);
    public static implicit operator ElementId((string, uint) tuple) => new ElementId(tuple.Item1, tuple.Item2);
}