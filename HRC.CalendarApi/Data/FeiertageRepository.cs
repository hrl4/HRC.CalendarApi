using HRC.CalendarApi.Contracts.Feiertage;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HRC.CalendarApi.Data;

public sealed class FeiertageRepository
{
    private readonly string _connectionString;

    public FeiertageRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration["DB_MSS_HRX"]
            ?? configuration.GetConnectionString("DB_MSS_HRX")
            ?? throw new InvalidOperationException(
                "ConnectionString 'DB_MSS_HRX' wurde nicht gefunden.");
    }


    public async Task<IReadOnlyList<FeiertagDto>> GetFeiertageAsync(
        int jahr,
        string isoCode,
        int region,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                [EintragName],
                [Datum]
            FROM [hrc].[fn_FeiertageFuerJahr](
                @Jahr,
                @IsoCode,
                @Parameter
            )
            ORDER BY [Datum];
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Jahr", jahr);
        command.Parameters.AddWithValue("@IsoCode", isoCode);
        command.Parameters.AddWithValue("@Parameter", region);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var nameOrdinal = reader.GetOrdinal("EintragName");
        var datumOrdinal = reader.GetOrdinal("Datum");

        var result = new List<FeiertagDto>();

        while (await reader.ReadAsync(cancellationToken))
        {
            result.Add(new FeiertagDto
            {
                Name = reader.GetString(nameOrdinal),
                Datum = DateOnly.FromDateTime(
                    reader.GetDateTime(datumOrdinal))
            });
        }

        return result;
    }

    public async Task<OstersonntagDto?> GetOstersonntagAsync(
        int jahr,
        string isoCode,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                @Jahr AS [Jahr],
                [Datum]
            FROM [hrc].[fn_FeiertageFuerJahr](
                @Jahr,
                @IsoCode,
                @Parameter
            )
            WHERE [EintragName] = N'Ostersonntag';
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection);

        command.Parameters.AddWithValue("@Jahr", jahr);
        command.Parameters.AddWithValue("@IsoCode", isoCode);
        command.Parameters.AddWithValue("@Parameter", 0);

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new OstersonntagDto
        {
            Jahr = reader.GetInt32(reader.GetOrdinal("Jahr")),
            Datum = DateOnly.FromDateTime(
                reader.GetDateTime(reader.GetOrdinal("Datum")))
        };
    }
}