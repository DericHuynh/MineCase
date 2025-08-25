using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MineCase.Buffers;
using MineCase.Gateway.Health_Checks;
using MineCase.Gateway.Network;
using MineCase.Protocol;
using MineCase.Server;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Polly;
using System;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace MineCase.Gateway
{
    partial class Program
    {
        static async Task Main(string[] args)
        {
            var appBuilder = WebApplication.CreateBuilder();

            appBuilder.AddServiceDefaults();

            appBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("config.json", false, false)
                                        .AddEnvironmentVariables();

            appBuilder.Services.AddControllers();
            appBuilder.Services.AddHealthChecks()
                               .AddCheck<SettingsHealthCheck>("settingsHealthCheck");

            appBuilder.Services.AddSingleton<ConnectionRouter>();
            appBuilder.Services.AddSingleton<IPacketCompress, PacketCompress>();
            appBuilder.Services.AddTransient<ClientSession>();
            appBuilder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            appBuilder.Services.AddSingleton<ObjectPool<UncompressedPacket>>(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<UncompressedPacket>();
            });
            appBuilder.Services.AddSingleton<IBufferPool<byte>>(s => new BufferPool<byte>(ArrayPool<byte>.Shared));

            appBuilder.Services.AddOrleansClient(c =>
            {
                c.Configure<ClusterOptions>(configure =>
                {
                    configure.ClusterId = "dev";
                    configure.ServiceId = "MineCaseService";
                });
                c.UseMongoDBClient(appBuilder.Configuration.GetSection("persistenceOptions")["connectionString"]);
                c.UseMongoDBClustering(options =>
                {
                    options.DatabaseName = appBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                });

            });
            appBuilder.Services.AddHostedService<ConnectionRouter>();

            var host = appBuilder.Build();

            if (host.Environment.IsDevelopment())
            {
                host.UseDeveloperExceptionPage();
            }

            host.UseRouting();

            host.MapHealthChecks("/health", new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });
            host.MapHealthChecks("/alive", new() { ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse });

            await host.RunAsync();
        }
    }
}