using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BlogKrafter.Data;

/// <summary>
/// SQLite and MySQL hand dates back with <see cref="DateTimeKind.Unspecified"/>. Every date is stored
/// in UTC, so it is marked as such on the way out; otherwise feeds and JSON would show local offsets.
/// </summary>
public sealed class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    value => value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime(),
    value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
{
    public static UtcDateTimeConverter Instance { get; } = new();
}

/// <inheritdoc cref="UtcDateTimeConverter"/>
public sealed class UtcNullableDateTimeConverter() : ValueConverter<DateTime?, DateTime?>(
    value => value.HasValue ? (value.Value.Kind == DateTimeKind.Utc ? value.Value : value.Value.ToUniversalTime()) : value,
    value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value)
{
    public static UtcNullableDateTimeConverter Instance { get; } = new();
}