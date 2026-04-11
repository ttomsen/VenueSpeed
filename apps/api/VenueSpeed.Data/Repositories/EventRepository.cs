using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class EventRepository : IEventRepository
{
    private readonly SqlConnectionFactory _factory;

    public EventRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<EventDto>> GetAllByVenueAsync(long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT ExternalId, EventName, EventDateUtc, DoorsOpenUtc, Status, CreatedAtUtc, UpdatedAtUtc
            FROM dbo.vw_ActiveEvents
            WHERE VenueId = @VenueId
            ORDER BY EventDateUtc DESC", conn);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<EventDto>();
        while (await reader.ReadAsync())
            results.Add(MapEventDto(reader));
        return results;
    }

    public async Task<EventDto?> GetByExternalIdAsync(Guid externalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT ExternalId, EventName, EventDateUtc, DoorsOpenUtc, Status, CreatedAtUtc, UpdatedAtUtc
            FROM dbo.vw_ActiveEvents
            WHERE ExternalId = @ExternalId AND VenueId = @VenueId", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapEventDto(reader) : null;
    }

    public async Task<Guid> CreateAsync(long venueId, CreateEventRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            INSERT INTO dbo.Event (VenueId, EventName, EventDateUtc, DoorsOpenUtc, Status)
            OUTPUT INSERTED.ExternalId
            VALUES (@VenueId, @EventName, @EventDateUtc, @DoorsOpenUtc, 'Draft')", conn);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        cmd.Parameters.AddWithValue("@EventName", request.EventName);
        cmd.Parameters.AddWithValue("@EventDateUtc", request.EventDateUtc);
        cmd.Parameters.AddWithValue("@DoorsOpenUtc", request.DoorsOpenUtc);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task UpdateAsync(Guid externalId, long venueId, UpdateEventRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.Event
            SET EventName    = @EventName,
                EventDateUtc = @EventDateUtc,
                DoorsOpenUtc = @DoorsOpenUtc,
                Status       = @Status,
                UpdatedAtUtc = GETUTCDATE()
            WHERE ExternalId = @ExternalId AND VenueId = @VenueId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        cmd.Parameters.AddWithValue("@EventName", request.EventName);
        cmd.Parameters.AddWithValue("@EventDateUtc", request.EventDateUtc);
        cmd.Parameters.AddWithValue("@DoorsOpenUtc", request.DoorsOpenUtc);
        cmd.Parameters.AddWithValue("@Status", request.Status.ToString());
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SoftDeleteAsync(Guid externalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.Event
            SET IsDeleted = 1, DeletedAtUtc = GETUTCDATE(), Status = 'Cancelled', UpdatedAtUtc = GETUTCDATE()
            WHERE ExternalId = @ExternalId AND VenueId = @VenueId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static EventDto MapEventDto(SqlDataReader r) => new(
        r.GetGuid(0), r.GetString(1), r.GetDateTime(2), r.GetDateTime(3),
        Enum.Parse<EventStatus>(r.GetString(4)),
        r.GetDateTime(5), r.GetDateTime(6)
    );
}
