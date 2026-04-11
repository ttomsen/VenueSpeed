using Microsoft.Data.SqlClient;
using VenueSpeed.Core.DTOs;
using VenueSpeed.Core.Enums;
using VenueSpeed.Core.Interfaces;

namespace VenueSpeed.Data.Repositories;

public class DiscoveryRepository : IDiscoveryRepository
{
    private readonly SqlConnectionFactory _factory;

    public DiscoveryRepository(SqlConnectionFactory factory) => _factory = factory;

    public async Task<IReadOnlyList<NearbyEventDto>> GetNearbyEventsAsync(double lat, double lng, double radiusMiles)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        // Haversine approximation — swap for geography type in production for accuracy
        using var cmd = new SqlCommand(@"
            SELECT e.ExternalId, e.EventName, e.EventDateUtc,
                   v.VenueName, v.Slug, v.City, v.StateCode, v.Latitude, v.Longitude,
                   3958.8 * 2 * ASIN(SQRT(
                       POWER(SIN(RADIANS(v.Latitude  - @Lat) / 2), 2) +
                       COS(RADIANS(@Lat)) * COS(RADIANS(v.Latitude)) *
                       POWER(SIN(RADIANS(v.Longitude - @Lng) / 2), 2)
                   )) AS DistanceMiles
            FROM dbo.vw_ActiveEvents e
            INNER JOIN dbo.vw_ActiveVenues v ON e.VenueId = v.Id
            WHERE v.Latitude IS NOT NULL AND v.Longitude IS NOT NULL
              AND e.EventDateUtc >= GETUTCDATE()
              AND e.Status IN ('Published','Active')
              AND 3958.8 * 2 * ASIN(SQRT(
                      POWER(SIN(RADIANS(v.Latitude  - @Lat) / 2), 2) +
                      COS(RADIANS(@Lat)) * COS(RADIANS(v.Latitude)) *
                      POWER(SIN(RADIANS(v.Longitude - @Lng) / 2), 2)
                  )) <= @RadiusMiles
            ORDER BY DistanceMiles, e.EventDateUtc", conn);
        cmd.Parameters.AddWithValue("@Lat", lat);
        cmd.Parameters.AddWithValue("@Lng", lng);
        cmd.Parameters.AddWithValue("@RadiusMiles", radiusMiles);
        using var reader = await cmd.ExecuteReaderAsync();
        return await ReadNearbyEventsAsync(reader);
    }

    public async Task<IReadOnlyList<NearbyEventDto>> GetEventsByCityAsync(string city)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT e.ExternalId, e.EventName, e.EventDateUtc,
                   v.VenueName, v.Slug, v.City, v.StateCode, v.Latitude, v.Longitude,
                   NULL AS DistanceMiles
            FROM dbo.vw_ActiveEvents e
            INNER JOIN dbo.vw_ActiveVenues v ON e.VenueId = v.Id
            WHERE v.City = @City
              AND e.EventDateUtc >= GETUTCDATE()
              AND e.Status IN ('Published','Active')
            ORDER BY e.EventDateUtc", conn);
        cmd.Parameters.AddWithValue("@City", city);
        using var reader = await cmd.ExecuteReaderAsync();
        return await ReadNearbyEventsAsync(reader);
    }

    public async Task<PublicVenueDto?> GetPublicVenueBySlugAsync(string slug)
    {
        using var conn = _factory.Create();
        await conn.OpenAsync();
        using var cmd = new SqlCommand(@"
            SELECT v.ExternalId, v.VenueName, v.Slug, v.City, v.StateCode,
                   s.Description, s.LogoUrl, s.CoverPhotoUrl, s.GoogleMapsUrl, v.TokenPriceUsd
            FROM dbo.vw_ActiveVenues v
            LEFT JOIN dbo.VenueSettings s ON v.Id = s.VenueId
            WHERE v.Slug = @Slug", conn);
        cmd.Parameters.AddWithValue("@Slug", slug);
        using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return null;
        var venue = new PublicVenueDto(
            reader.GetGuid(0), reader.GetString(1), reader.GetString(2),
            reader.GetString(3), reader.GetString(4),
            reader.IsDBNull(5) ? null : reader.GetString(5),
            reader.IsDBNull(6) ? null : reader.GetString(6),
            reader.IsDBNull(7) ? null : reader.GetString(7),
            reader.IsDBNull(8) ? null : reader.GetString(8),
            reader.GetDecimal(9),
            []
        );
        return venue;
    }

    private static async Task<List<NearbyEventDto>> ReadNearbyEventsAsync(SqlDataReader reader)
    {
        var results = new List<NearbyEventDto>();
        while (await reader.ReadAsync())
        {
            results.Add(new NearbyEventDto(
                reader.GetGuid(0), reader.GetString(1), reader.GetDateTime(2),
                reader.GetString(3), reader.GetString(4), reader.GetString(5), reader.GetString(6),
                reader.IsDBNull(7) ? null : reader.GetDecimal(7),
                reader.IsDBNull(8) ? null : reader.GetDecimal(8),
                reader.IsDBNull(9) ? null : reader.GetDouble(9),
                []
            ));
        }
        return results;
    }
}
