using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using VenueSpeed.Data;

namespace VenueSpeed.Api.Controllers;

[ApiController]
[Route("health")]
public class HealthController : ControllerBase
{
    private readonly SqlConnectionFactory _factory;
    private readonly ILogger<HealthController> _logger;

    public HealthController(SqlConnectionFactory factory, ILogger<HealthController> logger)
    {
        _factory = factory;
        _logger = logger;
    }

    /// <summary>Checks SQL connectivity. Returns 200 when healthy, 503 when degraded.</summary>
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        try
        {
            using var conn = _factory.Create();
            await conn.OpenAsync();
            using var cmd = new SqlCommand("SELECT 1", conn);
            await cmd.ExecuteScalarAsync();
            return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(503, new { status = "degraded", reason = "SQL connectivity check failed." });
        }
    }
}
