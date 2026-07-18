namespace Pondhawk.Sample.Api.Domain;

/// <summary>
/// The trivial reference entity. Addressed over the API by its <see cref="Uid"/> (a ULID); the numeric
/// <see cref="Id"/> is the EF primary key and is never exposed on the wire.
/// </summary>
public class Widget
{
    public int Id { get; set; }

    public string Uid { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
