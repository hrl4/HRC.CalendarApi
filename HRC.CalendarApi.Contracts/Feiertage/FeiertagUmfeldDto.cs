namespace HRC.CalendarApi.Contracts.Feiertage;

public sealed class FeiertagUmfeldDto
{
    public string Name { get; init; } = string.Empty;
    public DateOnly Datum { get; init; }
    public IReadOnlyList<UmfeldTagDto> Tage { get; init; } = [];
}

public sealed class UmfeldTagDto
{
    public DateOnly Datum { get; init; }

    /// <summary>Abstand zum Feiertag in Tagen (negativ = davor, positiv = danach).</summary>
    public int Abstand { get; init; }

    public string Wochentag { get; init; } = string.Empty;
    public bool IstWochenende { get; init; }
    public bool IstFeiertag { get; init; }
    public string? FeiertagName { get; init; }
}
