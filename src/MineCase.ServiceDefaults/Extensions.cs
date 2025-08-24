using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ServiceDiscovery;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.ConfigureOpenTelemetryServices(builder.Environment.ApplicationName);

        builder.Services.AddServiceDiscovery();

        builder.Services.AddStartupHealthCheck();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Turn on resilience by default
            http.AddStandardResilienceHandler();

            // Turn on service discovery by default
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static IHostBuilder AddServiceDefaults(this IHostBuilder builder, string applicationName)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ConfigureOpenTelemetryLogging();
        });

        builder.ConfigureServices(services =>
        {
            services.AddServiceDiscovery();
            services.ConfigureOpenTelemetryServices(applicationName);
            services.AddStartupHealthCheck();
            services.ConfigureHttpClientDefaults(http =>
            {
                // Turn on resilience by default
                http.AddStandardResilienceHandler();

                // Turn on service discovery by default
                http.AddServiceDiscovery();
            });
        });

        return builder;
    }

    private static IServiceCollection ConfigureOpenTelemetryServices(this IServiceCollection services, string applicationName)
    {
        services.AddOpenTelemetry()
                .WithMetrics(metrics =>
                {
                    metrics.AddAspNetCoreInstrumentation()
                           .AddRuntimeInstrumentation();
                })
                .WithTracing(tracing =>
                {
                    tracing.AddSource(applicationName)
                           .AddAspNetCoreInstrumentation(tracing =>
                           // Exclude health check requests from tracing
                           tracing.Filter = context =>
                                !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                                && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                            )
                            // Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
                            //.AddGrpcClientInstrumentation()
                            .AddHttpClientInstrumentation()
                            .AddSource("MineCase");
                })
                .UseOtlpExporter();
        return services;
    }

    private static ILoggingBuilder ConfigureOpenTelemetryLogging(this ILoggingBuilder logging)
    {
        logging.ClearProviders();

        logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        return logging;
    }

    private static IServiceCollection AddStartupHealthCheck(this IServiceCollection services)
    {
        services.AddHealthChecks()
                // Add a default liveness check to ensure app is responsive
                .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return services;
    }
}
