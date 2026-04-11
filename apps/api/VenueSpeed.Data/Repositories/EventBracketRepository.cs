using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class EventBracketRepository : IEventBracketRepository
{
    private readonly SqlConnectionFactory _factory;

    public EventBracketRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<BracketDto>> GetByEventAsync(Guid eventExternalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT b.ExternalId, e.ExternalId, b.BracketName, b.AgeRangeMin, b.AgeRangeMax,
                   b.GenderComposition, b.StartTimeUtc, b.EndTimeUtc, b.RoundDurationSeconds,
                   b.MaxParticipantsPerSide, b.TicketPriceUsd, b.Status,
                   b.CheckInOpenUtc, b.ActualStartUtc, b.ActualEndUtc, b.CurrentRoundNumber
            FROM dbo.vw_ActiveEventBrackets b
            INNER JOIN dbo.Event e ON b.EventId = e.Id
            WHERE e.ExternalId = @EventExternalId AND b.VenueId = @VenueId
            ORDER BY b.StartTimeUtc", conn);
        cmd.Parameters.AddWithValue("@EventExternalId", eventExternalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<BracketDto>();
        while (await reader.ReadAsync())
            results.Add(MapBracketDto(reader));
        return results;
    }

    public async Task<BracketDto?> GetByExternalIdAsync(Guid bracketExternalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT b.ExternalId, e.ExternalId, b.BracketName, b.AgeRangeMin, b.AgeRangeMax,
                   b.GenderComposition, b.StartTimeUtc, b.EndTimeUtc, b.RoundDurationSeconds,
                   b.MaxParticipantsPerSide, b.TicketPriceUsd, b.Status,
                   b.CheckInOpenUtc, b.ActualStartUtc, b.ActualEndUtc, b.CurrentRoundNumber
            FROM dbo.vw_ActiveEventBrackets b
            INNER JOIN dbo.Event e ON b.EventId = e.Id
            WHERE b.ExternalId = @ExternalId AND b.VenueId = @VenueId", conn);
        cmd.Parameters.AddWithValue("@ExternalId", bracketExternalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapBracketDto(reader) : null;
    }

    public async Task<Guid> CreateAsync(Guid eventExternalId, long venueId, CreateBracketRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            INSERT INTO dbo.EventBracket
                (VenueId, EventId, BracketName, AgeRangeMin, AgeRangeMax, GenderComposition,
                 StartTimeUtc, EndTimeUtc, RoundDurationSeconds, MaxParticipantsPerSide, TicketPriceUsd, Status)
            OUTPUT INSERTED.ExternalId
            SELECT @VenueId, e.Id, @BracketName, @AgeRangeMin, @AgeRangeMax, @GenderComposition,
                   @StartTimeUtc, @EndTimeUtc, @RoundDurationSeconds, @MaxParticipantsPerSide, @TicketPriceUsd, 'Scheduled'
            FROM dbo.Event e
            WHERE e.ExternalId = @EventExternalId AND e.VenueId = @VenueId AND e.IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        cmd.Parameters.AddWithValue("@EventExternalId", eventExternalId);
        cmd.Parameters.AddWithValue("@BracketName", request.BracketName);
        cmd.Parameters.AddWithValue("@AgeRangeMin", (object?)request.AgeRangeMin ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AgeRangeMax", (object?)request.AgeRangeMax ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GenderComposition", request.GenderComposition.ToString());
        cmd.Parameters.AddWithValue("@StartTimeUtc", request.StartTimeUtc);
        cmd.Parameters.AddWithValue("@EndTimeUtc", request.EndTimeUtc);
        cmd.Parameters.AddWithValue("@RoundDurationSeconds", request.RoundDurationSeconds);
        cmd.Parameters.AddWithValue("@MaxParticipantsPerSide", request.MaxParticipantsPerSide);
        cmd.Parameters.AddWithValue("@TicketPriceUsd", request.TicketPriceUsd);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Guid bracketExternalId, long venueId, UpdateBracketRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.EventBracket
            SET BracketName            = @BracketName,
                AgeRangeMin            = @AgeRangeMin,
                AgeRangeMax            = @AgeRangeMax,
                GenderComposition      = @GenderComposition,
                StartTimeUtc           = @StartTimeUtc,
                EndTimeUtc             = @EndTimeUtc,
                RoundDurationSeconds   = @RoundDurationSeconds,
                MaxParticipantsPerSide = @MaxParticipantsPerSide,
                TicketPriceUsd         = @TicketPriceUsd,
                Status                 = @Status,
                UpdatedAtUtc           = GETUTCDATE()
            WHERE ExternalId = @ExternalId AND VenueId = @VenueId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", bracketExternalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        cmd.Parameters.AddWithValue("@BracketName", request.BracketName);
        cmd.Parameters.AddWithValue("@AgeRangeMin", (object?)request.AgeRangeMin ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AgeRangeMax", (object?)request.AgeRangeMax ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@GenderComposition", request.GenderComposition.ToString());
        cmd.Parameters.AddWithValue("@StartTimeUtc", request.StartTimeUtc);
        cmd.Parameters.AddWithValue("@EndTimeUtc", request.EndTimeUtc);
        cmd.Parameters.AddWithValue("@RoundDurationSeconds", request.RoundDurationSeconds);
        cmd.Parameters.AddWithValue("@MaxParticipantsPerSide", request.MaxParticipantsPerSide);
        cmd.Parameters.AddWithValue("@TicketPriceUsd", request.TicketPriceUsd);
        cmd.Parameters.AddWithValue("@Status", request.Status.ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SoftDeleteAsync(Guid bracketExternalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.EventBracket
            SET IsDeleted = 1, DeletedAtUtc = GETUTCDATE(), Status = 'Cancelled', UpdatedAtUtc = GETUTCDATE()
            WHERE ExternalId = @ExternalId AND VenueId = @VenueId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", bracketExternalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static BracketDto MapBracketDto(SqlDataReader r) => new(
        r.GetGuid(0), r.GetGuid(1), r.GetString(2),
        r.IsDBNull(3) ? null : r.GetInt32(3),
        r.IsDBNull(4) ? null : r.GetInt32(4),
        Enum.Parse<GenderComposition>(r.GetString(5)),
        r.GetDateTime(6), r.GetDateTime(7),
        r.GetInt32(8), r.GetInt32(9), r.GetDecimal(10),
        Enum.Parse<BracketStatus>(r.GetString(11)),
        r.IsDBNull(12) ? null : r.GetDateTime(12),
        r.IsDBNull(13) ? null : r.GetDateTime(13),
        r.IsDBNull(14) ? null : r.GetDateTime(14),
        r.IsDBNull(15) ? null : r.GetInt32(15)
    );
}
