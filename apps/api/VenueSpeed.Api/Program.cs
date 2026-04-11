using FluentValidation;
using Microsoft.Identity.Web;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using VenueSpeed.Api.Middleware;
using VenueSpeed.Api.Services;
using VenueSpeed.Core.Interfaces;
using VenueSpeed.Data.Extensions;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting VenueSpeed API");

    var builder = WebApplication.CreateBuilder(args);

    // -----------------------------------------------------------------------
    // Serilog
    // -----------------------------------------------------------------------
    builder.Host.UseSerilog((ctx, services, config) =>
    {
        config
            .ReadFrom.Configuration(ctx.Configuration)
            .ReadFrom.Services(services)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate:
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");

        var aiConnection = ctx.Configuration["ApplicationInsights:ConnectionString"];
        if (!string.IsNullOrEmpty(aiConnection))
            config.WriteTo.ApplicationInsights(aiConnection, TelemetryConverter.Traces);
    });

    // -----------------------------------------------------------------------
    // Authentication — Azure AD B2C (skipped locally when not configured)
    // -----------------------------------------------------------------------
    var b2cClientId = builder.Configuration["AzureAdB2C:ClientId"];
    if (!string.IsNullOrEmpty(b2cClientId))
    {
        builder.Services.AddMicrosoftIdentityWebApiAuthentication(
            builder.Configuration, "AzureAdB2C");
    }
    else
    {
        builder.Services.AddAuthentication();
        builder.Services.AddAuthorization();
    }

    // -----------------------------------------------------------------------
    // MVC / Controllers
    // -----------------------------------------------------------------------
    builder.Services.AddControllers();

    // -----------------------------------------------------------------------
    // OpenAPI (built-in .NET 10) + Scalar UI
    // -----------------------------------------------------------------------
    builder.Services.AddOpenApi();

    // -----------------------------------------------------------------------
    // Application Insights — only wire up when a connection string is present
    // -----------------------------------------------------------------------
    var aiConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    if (!string.IsNullOrEmpty(aiConnectionString))
        builder.Services.AddApplicationInsightsTelemetry();

    // -----------------------------------------------------------------------
    // MediatR — scan Api assembly for all handlers
    // -----------------------------------------------------------------------
    builder.Services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

    // -----------------------------------------------------------------------
    // FluentValidation — scan Api assembly for all validators
    // -----------------------------------------------------------------------
    builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

    // -----------------------------------------------------------------------
    // Data layer (SqlConnectionFactory + repositories)
    // -----------------------------------------------------------------------
    builder.Services.AddDataServices();

    // -----------------------------------------------------------------------
    // ITenantContext — scoped so the middleware can populate it per-request
    // -----------------------------------------------------------------------
    builder.Services.AddScoped<TenantContext>();
    builder.Services.AddScoped<ITenantContext>(sp => sp.GetRequiredService<TenantContext>());

    // -----------------------------------------------------------------------
    var app = builder.Build();
    // -----------------------------------------------------------------------

    // Global error handler must be first
    app.UseMiddleware<ErrorHandlingMiddleware>();

    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();                         // /openapi/v1.json
        app.MapScalarApiReference();              // /scalar/v1
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    // In Development (no real B2C token), inject a fixed test identity
    if (app.Environment.IsDevelopment())
        app.UseMiddleware<DevAuthMiddleware>();

    // Populate ITenantContext from JWT claims after auth middleware runs
    app.UseMiddleware<TenantContextMiddleware>();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "VenueSpeed API terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Expose Program for test project WebApplicationFactory
public partial class Program { }
