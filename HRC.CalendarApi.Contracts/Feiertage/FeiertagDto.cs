namespace HRC.CalendarApi.Contracts.Feiertage;

public sealed class FeiertagDto
{
    public string Name { get; init; } = string.Empty;
    public DateOnly Datum { get; init; }
}
