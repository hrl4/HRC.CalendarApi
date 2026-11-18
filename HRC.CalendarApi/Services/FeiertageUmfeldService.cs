using System.Globalization;
using HRC.CalendarApi.Contracts.Feiertage;
using HRC.CalendarApi.Data;

namespace HRC.CalendarApi.Services;

public sealed class FeiertageUmfeldService
{
    private static readonly DateTimeFormatInfo DeutscheDatumsFormate =
        CultureInfo.GetCultureInfo("de-DE").DateTimeFormat;

    private readonly FeiertageRepository _repository;

    public FeiertageUmfeldService(FeiertageRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<FeiertagUmfeldDto>> GetUmfeldAsync(
        int jahr,
        string isoCode,
        int region,
        int tageVorher,
        int tageNachher,
        CancellationToken cancellationToken = default)
    {
        var feiertage =
            await _repository.GetFeiertageAsync(jahr, isoCode, region, cancellationToken);

        if (feiertage.Count == 0)
        {
            return [];
        }

        // Feiertage aller betroffenen Jahre laden, damit z. B. Silvester + 1 als Neujahr erkannt wird.
        var alleFeiertage = new Dictionary<DateOnly, string>();
        AddFeiertage(alleFeiertage, feiertage);

        var ersterTag = feiertage.Min(f => f.Datum).AddDays(-tageVorher);
        var letzterTag = feiertage.Max(f => f.Datum).AddDays(tageNachher);

        for (var weiteresJahr = ersterTag.Year; weiteresJahr <= letzterTag.Year; weiteresJahr++)
        {
            if (weiteresJahr == jahr)
            {
                continue;
            }

            var weitere = await _repository.GetFeiertageAsync(
                weiteresJahr, isoCode, region, cancellationToken);

            AddFeiertage(alleFeiertage, weitere);
        }

        return feiertage
            .Select(feiertag => new FeiertagUmfeldDto
            {
                Name = feiertag.Name,
                Datum = feiertag.Datum,
                Tage = Enumerable
                    .Range(-tageVorher, tageVorher + tageNachher + 1)
                    .Where(abstand => abstand != 0)
                    .Select(abstand => CreateUmfeldTag(feiertag.Datum, abstand, alleFeiertage))
                    .ToList()
            })
            .ToList();
    }

    private static void AddFeiertage(
        Dictionary<DateOnly, string> ziel,
        IEnumerable<FeiertagDto> feiertage)
    {
        foreach (var feiertag in feiertage)
        {
            ziel.TryAdd(feiertag.Datum, feiertag.Name);
        }
    }

    private static UmfeldTagDto CreateUmfeldTag(
        DateOnly feiertagDatum,
        int abstand,
        IReadOnlyDictionary<DateOnly, string> alleFeiertage)
    {
        var datum = feiertagDatum.AddDays(abstand);
        var istFeiertag = alleFeiertage.TryGetValue(datum, out var name);

        return new UmfeldTagDto
        {
            Datum = datum,
            Abstand = abstand,
            Wochentag = DeutscheDatumsFormate.GetDayName(datum.DayOfWeek),
            IstWochenende = datum.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
            IstFeiertag = istFeiertag,
            FeiertagName = name
        };
    }
}
