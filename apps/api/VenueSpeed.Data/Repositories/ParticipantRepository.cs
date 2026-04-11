using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class ParticipantRepository : IParticipantRepository
{
    private readonly SqlConnectionFactory _factory;

    public ParticipantRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<ParticipantDto?> GetByAuthProviderIdAsync(string authProviderId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT ExternalId, FirstName, DateOfBirth, Gender, City, StateCode,
                   HeadlineText, ProfilePhotoUrl, Interests, IsActive, CreatedAtUtc
            FROM dbo.vw_ActiveParticipants
            WHERE AuthProviderId = @AuthProviderId", conn);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        return new ParticipantDto(
            reader.GetGuid(0), reader.GetString(1),
            DateOnly.FromDateTime(reader.GetDateTime(2)),
            reader.GetString(3)[0],
            reader.IsDBNull(4) ? null : reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetBoolean(9), reader.GetDateTime(10)
        );
    }

    public async Task UpdateAsync(string authProviderId, UpdateParticipantRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.Participant
            SET FirstName       = @FirstName,
                DateOfBirth     = @DateOfBirth,
                Gender          = @Gender,
                City            = @City,
                StateCode       = @StateCode,
                HeadlineText    = @HeadlineText,
                ProfilePhotoUrl = @ProfilePhotoUrl,
                Interests       = @Interests,
                UpdatedAtUtc    = GETUTCDATE()
            WHERE AuthProviderId = @AuthProviderId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        cmd.Parameters.AddWithValue("@FirstName", request.FirstName);
        cmd.Parameters.AddWithValue("@DateOfBirth", request.DateOfBirth.ToDateTime(TimeOnly.MinValue));
        cmd.Parameters.AddWithValue("@Gender", request.Gender.ToString());
        cmd.Parameters.AddWithValue("@City", (object?)request.City ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@StateCode", (object?)request.StateCode ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@HeadlineText", (object?)request.HeadlineText ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@ProfilePhotoUrl", (object?)request.ProfilePhotoUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Interests", (object?)request.Interests ?? DBNull.Value);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task GdprDeleteAsync(string authProviderId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.Participant
            SET IsDeleted    = 1,
                IsActive     = 0,
                DeletedAtUtc = GETUTCDATE(),
                FirstName    = 'Deleted',
                DateOfBirth  = '1900-01-01',
                HeadlineText    = NULL,
                ProfilePhotoUrl = NULL,
                Interests       = NULL,
                City            = NULL,
                StateCode       = NULL,
                UpdatedAtUtc    = GETUTCDATE()
            WHERE AuthProviderId = @AuthProviderId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@AuthProviderId", authProviderId);
        await cmd.ExecuteNonQueryAsync();
    }
}
