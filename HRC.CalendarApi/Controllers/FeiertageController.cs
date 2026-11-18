using HRC.CalendarApi.Contracts.Feiertage;
using HRC.CalendarApi.Data;
using HRC.CalendarApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HRC.CalendarApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FeiertageController : ControllerBase
{
    private const int MaxUmfeldTage = 31;

    private readonly FeiertageRepository _repository;
    private readonly FeiertageUmfeldService _umfeldService;

    public FeiertageController(
        FeiertageRepository repository,
        FeiertageUmfeldService umfeldService)
    {
        _repository = repository;
        _umfeldService = umfeldService;
    }

    [HttpGet("{jahr:int}")]
    public async Task<ActionResult<IReadOnlyList<FeiertagDto>>> GetFeiertage(
        int jahr,
        [FromQuery] string isoCode = "BB",
        [FromQuery] int region = 0,
        CancellationToken cancellationToken = default)
    {
        var feiertage =
            await _repository.GetFeiertageAsync(
                jahr,
                isoCode,
                region,
                cancellationToken);

        return Ok(feiertage);
    }

    [HttpGet("{jahr:int}/umfeld")]
    public async Task<ActionResult<IReadOnlyList<FeiertagUmfeldDto>>> GetFeiertageUmfeld(
        int jahr,
        [FromQuery] string isoCode = "BB",
        [FromQuery] int region = 0,
        [FromQuery] int tageVorher = 1,
        [FromQuery] int tageNachher = 1,
        CancellationToken cancellationToken = default)
    {
        if (tageVorher is < 0 or > MaxUmfeldTage
            || tageNachher is < 0 or > MaxUmfeldTage)
        {
            return BadRequest(
                $"tageVorher und tageNachher müssen zwischen 0 und {MaxUmfeldTage} liegen.");
        }

        var umfeld =
            await _umfeldService.GetUmfeldAsync(
                jahr,
                isoCode,
                region,
                tageVorher,
                tageNachher,
                cancellationToken);

        return Ok(umfeld);
    }

    [HttpGet("ostersonntag/{jahr:int}")]
    public async Task<ActionResult<OstersonntagDto>> GetOstersonntag(
        int jahr,
        [FromQuery] string isoCode = "BB",
        CancellationToken cancellationToken = default)
    {
        var ostersonntag =
            await _repository.GetOstersonntagAsync(
                jahr,
                isoCode,
                cancellationToken);

        if (ostersonntag is null)
        {
            return NotFound();
        }

        return Ok(ostersonntag);
    }
}