using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class VenueRepository : IVenueRepository
{
    private readonly SqlConnectionFactory _factory;

    public VenueRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<VenueDto?> GetByExternalIdAsync(Guid externalId, long venueId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT v.ExternalId, v.VenueName, v.Slug, v.Email, v.PhoneNumber,
                   v.AddressLine1, v.AddressLine2, v.City, v.StateCode, v.PostalCode,
                   v.Latitude, v.Longitude, v.VenueType, v.Capacity,
                   v.StripeOnboardingComplete, v.TokenPriceUsd, v.IsActive,
                   v.CreatedAtUtc, v.UpdatedAtUtc
            FROM dbo.vw_ActiveVenues v
            WHERE v.ExternalId = @ExternalId AND v.Id = @VenueId", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapVenueDto(reader) : null;
    }

    public async Task<VenueDto?> GetBySlugAsync(string slug)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT v.ExternalId, v.VenueName, v.Slug, v.Email, v.PhoneNumber,
                   v.AddressLine1, v.AddressLine2, v.City, v.StateCode, v.PostalCode,
                   v.Latitude, v.Longitude, v.VenueType, v.Capacity,
                   v.StripeOnboardingComplete, v.TokenPriceUsd, v.IsActive,
                   v.CreatedAtUtc, v.UpdatedAtUtc
            FROM dbo.vw_ActiveVenues v
            WHERE v.Slug = @Slug", conn);
        cmd.Parameters.AddWithValue("@Slug", slug);
        using var reader = await cmd.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapVenueDto(reader) : null;
    }

    public async Task UpdateAsync(long venueId, UpdateVenueRequest request)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            UPDATE dbo.Venue
            SET VenueName    = @VenueName,
                PhoneNumber  = @PhoneNumber,
                AddressLine1 = @AddressLine1,
                AddressLine2 = @AddressLine2,
                City         = @City,
                StateCode    = @StateCode,
                PostalCode   = @PostalCode,
                Latitude     = @Latitude,
                Longitude    = @Longitude,
                VenueType    = @VenueType,
                Capacity     = @Capacity,
                TokenPriceUsd = @TokenPriceUsd,
                UpdatedAtUtc = GETUTCDATE()
            WHERE Id = @VenueId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@VenueId", venueId);
        cmd.Parameters.AddWithValue("@VenueName", request.VenueName);
        cmd.Parameters.AddWithValue("@PhoneNumber", (object?)request.PhoneNumber ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@AddressLine1", request.AddressLine1);
        cmd.Parameters.AddWithValue("@AddressLine2", (object?)request.AddressLine2 ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@City", request.City);
        cmd.Parameters.AddWithValue("@StateCode", request.StateCode);
        cmd.Parameters.AddWithValue("@PostalCode", request.PostalCode);
        cmd.Parameters.AddWithValue("@Latitude", (object?)request.Latitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@Longitude", (object?)request.Longitude ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@VenueType", request.VenueType);
        cmd.Parameters.AddWithValue("@Capacity", (object?)request.Capacity ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TokenPriceUsd", request.TokenPriceUsd);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<AdminVenueDto>> GetAllAsync()
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT ExternalId, VenueName, Slug, Email, City, StateCode,
                   IsActive, StripeOnboardingComplete, CreatedAtUtc
            FROM dbo.Venue
            WHERE IsDeleted = 0
            ORDER BY VenueName", conn);
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<AdminVenueDto>();
        while (await reader.ReadAsync())
            results.Add(new AdminVenueDto(
                reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5),
                reader.GetBoolean(6), reader.GetBoolean(7), reader.GetDateTime(8)));
        return results;
    }

    public async Task SuspendAsync(Guid externalId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Venue SET IsActive = 0, UpdatedAtUtc = GETUTCDATE() WHERE ExternalId = @ExternalId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task ReinstateAsync(Guid externalId)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(
            "UPDATE dbo.Venue SET IsActive = 1, UpdatedAtUtc = GETUTCDATE() WHERE ExternalId = @ExternalId AND IsDeleted = 0", conn);
        cmd.Parameters.AddWithValue("@ExternalId", externalId);
        await cmd.ExecuteNonQueryAsync();
    }

    private static VenueDto MapVenueDto(SqlDataReader r) => new(
        r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetString(3),
        r.IsDBNull(4) ? null : r.GetString(4),
        r.GetString(5),
        r.IsDBNull(6) ? null : r.GetString(6),
        r.GetString(7), r.GetString(8), r.GetString(9),
        r.IsDBNull(10) ? null : r.GetDecimal(10),
        r.IsDBNull(11) ? null : r.GetDecimal(11),
        r.GetString(12),
        r.IsDBNull(13) ? null : r.GetInt32(13),
        r.GetBoolean(14), r.GetDecimal(15), r.GetBoolean(16),
        r.GetDateTime(17), r.GetDateTime(18)
    );
}
