using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using MineCase.Serialization.Serializers;
using MineCase.Server.Health_Checks;
using MineCase.Server.Sampling;
using MineCase.Server.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using OpenTelemetry.Trace;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Placement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Threading.Tasks;

namespace MineCase.Server
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var createShardKey = false;
            Serializers.RegisterAll();

            var hostBuilder = WebApplication.CreateBuilder();

            hostBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                                     .AddJsonFile("config.json", false, false)
                                     .AddJsonFile("server.json", false, false)
                                     .AddEnvironmentVariables();

            // Set GUID representation for MongoDB otherwise it throws an exception
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

            hostBuilder.AddServiceDefaults();
            hostBuilder.Services.AddOpenTelemetry();
                                //.WithTracing(x => x.AddProcessor(new TailSamplingProcessor()));

            hostBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            hostBuilder.Host.ConfigureContainer<ContainerBuilder>(ConfigureAutofac);

            hostBuilder.Services.AddOptions();
            hostBuilder.Services.AddSingleton<RecyclableMemoryStreamManager>();

            hostBuilder.Services.AddControllers();
            hostBuilder.Services.AddHealthChecks()
                                .AddCheck<SettingsHealthCheck>("settingsHealthCheck");

            hostBuilder.Services.Configure<PersistenceOptions>(hostBuilder.Configuration.GetSection("persistenceOptions"));

            var siloPort = GetAvailablePort();
            var gatewayPort = GetAvailablePort();

            hostBuilder.UseOrleans((siloBuilder) =>
            {
                //siloBuilder.UseDashboard();
                //siloBuilder.AddActivityPropagation();
                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MineCaseService";
                });
#pragma warning disable ORLEANSEXP001
                siloBuilder.AddActivationRepartitioner(); // Doesnt work, but it should, so there's something wrong with the current grain persistence and state mechanisms.
                siloBuilder.Configure<ActivationRepartitionerOptions>(o =>
                {
                    o.MinRoundPeriod = TimeSpan.FromSeconds(5000);
                    o.MaxRoundPeriod = TimeSpan.FromSeconds(15000);
                    o.RecoveryPeriod = TimeSpan.FromSeconds(2000);
                });
                siloBuilder.ConfigureServices(services => services.AddSingleton<PlacementStrategy, PreferLocalPlacement>());
#pragma warning restore ORLEANSEXP001
                siloBuilder.ConfigureEndpoints(siloPort: siloPort, gatewayPort: gatewayPort);
                siloBuilder.UseMongoDBClient(hostBuilder.Configuration.GetSection("persistenceOptions")["connectionString"]);
                siloBuilder.AddMemoryStreams("JobsProvider");
                siloBuilder.AddMemoryStreams("TransientProvider");
                siloBuilder.UseMongoDBReminders(options =>
                {
                    options.DatabaseName = hostBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                    options.CreateShardKeyForCosmos = createShardKey;
                });
                siloBuilder.UseMongoDBClustering(c =>
                {
                    c.DatabaseName = hostBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                    c.CreateShardKeyForCosmos = createShardKey;
                });
                siloBuilder.AddMongoDBGrainStorageAsDefault(c => c.Configure(options =>
                {
                    options.DatabaseName = hostBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                    options.CreateShardKeyForCosmos = createShardKey;
                }));
                siloBuilder.AddMongoDBGrainStorage("PubSubStore", options =>
                {
                    options.DatabaseName = hostBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                    options.CreateShardKeyForCosmos = createShardKey;
                });
            });
            
            var host = hostBuilder.Build();
            Serializers.RegisterAll(host.Services);

            if (host.Environment.IsDevelopment())
            {
                host.UseDeveloperExceptionPage();
            }

            host.UseRouting();

            host.MapHealthChecks("/health", new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
            host.MapHealthChecks("/alive", new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

            //host.Map("/dashboard", x => x.UseOrleansDashboard());

            await host.RunAsync();
        }

        public static int GetAvailablePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }

        private static void ConfigureAutofac(ContainerBuilder builder)
        {
            var assemblies = new List<Assembly>();
            assemblies
                .AddEngine()
                .AddInterfaces()
                .AddGrains();
            builder.RegisterAssemblyModules(assemblies.ToArray());
        }
    }
}