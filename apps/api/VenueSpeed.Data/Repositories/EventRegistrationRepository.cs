using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class EventRegistrationRepository : IEventRegistrationRepository
{
    private readonly SqlConnectionFactory _factory;

    public EventRegistrationRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<Guid> CreateAsync(CreateRegistrationRequest request, string authProviderId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            INSERT INTO dbo.EventRegistration
                (VenueId, EventBracketId, ParticipantId, TicketPriceUsd, StripePaymentIntentId, PaymentStatus, CheckInStatus)
            OUTPUT INSERTED.ExternalId
            SELECT b.VenueId, b.Id, p.Id, b.TicketPriceUsd, @StripePaymentIntentId, 'Pending', 'NotCheckedIn'
            FROM dbo.vw_ActiveEventBrackets b
            CROSS JOIN dbo.vw_ActiveParticipants p
            WHERE b.ExternalId = @BracketExternalId
              AND p.AuthProviderId = @AuthProviderId", conn);
        cmd.Parameters.AddWithValue("@BracketExternalId", request.BracketExternalId);
        cmd.Parameters.AddWithValue("@StripePaymentIntentId", request.StripePaymentIntentId);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        return (Guid)(await cmd.ExecuteScalarAsync())!;
    }

    public async Task CancelAsync(Guid externalId, string authProviderId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE r
            SET r.RemovalReason = 'ParticipantCancelled',
                r.RemovedAtUtc  = GETUTCDATE(),
                r.UpdatedAtUtc  = GETUTCDATE()
            FROM dbo.EventRegistration r
            INNER JOIN dbo.Participant p ON r.ParticipantId = p.Id
            WHERE r.ExternalId = @ExternalId
              AND p.AuthProviderId = @AuthProviderId", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<MyEventDto>> GetUpcomingByParticipantAsync(string authProviderId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT r.ExternalId, e.EventName, v.VenueName, v.City, b.BracketName,
                   e.EventDateUtc, b.StartTimeUtc, r.PaymentStatus, r.CheckInStatus
            FROM dbo.EventRegistration r
            INNER JOIN dbo.Participant p         ON r.ParticipantId    = p.Id
            INNER JOIN dbo.vw_ActiveEventBrackets b ON r.EventBracketId = b.Id
            INNER JOIN dbo.vw_ActiveEvents e     ON b.EventId          = e.Id
            INNER JOIN dbo.vw_ActiveVenues v     ON e.VenueId          = v.Id
            WHERE p.AuthProviderId = @AuthProviderId
              AND e.EventDateUtc   >= GETUTCDATE()
              AND r.RemovalReason IS NULL
            ORDER BY e.EventDateUtc", conn);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<MyEventDto>();
        while (await reader.ReadAsync())
            results.Add(new MyEventDto(
                reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4),
                reader.GetDateTime(5), reader.GetDateTime(6),
                Enum.Parse<PaymentStatus>(reader.GetString(7)),
                Enum.Parse<CheckInStatus>(reader.GetString(8))
            ));
        return results;
    }
}
