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
using MineCase.Server.Settings;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
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
        public static int GetAvailablePort()
        {
            using var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            return port;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHealthChecks()
                .AddCheck<BasicOrleansHealthCheck>("basicOrleans");
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/healthz", new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
                endpoints.MapHealthChecks("/health");
                endpoints.MapHealthChecks("/alive");
            });
            app.Map("/dashboard", x => x.UseOrleansDashboard());
        }

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

            hostBuilder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            hostBuilder.Host.ConfigureContainer<ContainerBuilder>(ConfigureAutofac);

            hostBuilder.Services.AddOptions();
            hostBuilder.Services.AddLogging();
            hostBuilder.Services.AddSingleton<RecyclableMemoryStreamManager>();

            hostBuilder.Services.AddControllers();
            hostBuilder.Services.AddHealthChecks();
                                //.AddCheck<BasicOrleansHealthCheck>("basicOrleans");

            hostBuilder.Services.Configure<PersistenceOptions>(hostBuilder.Configuration.GetSection("persistenceOptions"));

            hostBuilder.UseOrleans((siloBuilder) =>
            {
                //siloBuilder.UseDashboard();
                siloBuilder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MineCaseService";
                });
                siloBuilder.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);
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