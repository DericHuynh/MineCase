using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using MineCase.Buffers;
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
        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddLogging();
            services.AddSingleton<ConnectionRouter>();
            services.AddSingleton<IPacketCompress, PacketCompress>();
            services.AddTransient<ClientSession>();
            services.AddHostedService<ConnectionRouter>();
            services.AddOrleansClient(c =>
            {
                c.Configure<ClusterOptions>(configure =>
                {
                    configure.ClusterId = "dev";
                    configure.ServiceId = "MineCaseService";
                });
                c.AddActivityPropagation();
                c.UseMongoDBClient(context.Configuration.GetSection("persistenceOptions")["connectionString"]);
                c.UseMongoDBClustering(options =>
                {
                    options.DatabaseName = context.Configuration.GetSection("persistenceOptions")["databaseName"];
                });

            });

            ConfigureObjectPools(services);
        }

        private static void ConfigureObjectPools(IServiceCollection services)
        {
            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            services.AddSingleton<ObjectPool<UncompressedPacket>>(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<UncompressedPacket>();
            });
            services.AddSingleton<IBufferPool<byte>>(s => new BufferPool<byte>(ArrayPool<byte>.Shared));
        }

        private static void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false, false)
                .AddEnvironmentVariables();
        }

        private static void ConfigureLogging(ILoggingBuilder loggingBuilder)
        {
            //loggingBuilder.ConfigureOpenTelemetryLogging();
        }

        static async Task Main(string[] args)
        {
            var webHostBuilder = WebApplication.CreateBuilder();

            webHostBuilder.AddServiceDefaults();

            webHostBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                                        .AddJsonFile("config.json", false, false)
                                        .AddEnvironmentVariables();

            webHostBuilder.Services.AddSingleton<ConnectionRouter>();
            webHostBuilder.Services.AddSingleton<IPacketCompress, PacketCompress>();
            webHostBuilder.Services.AddTransient<ClientSession>();
            webHostBuilder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            webHostBuilder.Services.AddSingleton<ObjectPool<UncompressedPacket>>(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<UncompressedPacket>();
            });
            webHostBuilder.Services.AddSingleton<IBufferPool<byte>>(s => new BufferPool<byte>(ArrayPool<byte>.Shared));

            webHostBuilder.Services.AddControllers();

            webHostBuilder.Services.AddOrleansClient(c =>
            {
                c.Configure<ClusterOptions>(configure =>
                {
                    configure.ClusterId = "dev";
                    configure.ServiceId = "MineCaseService";
                });
                c.UseMongoDBClient(webHostBuilder.Configuration.GetSection("persistenceOptions")["connectionString"]);
                c.UseMongoDBClustering(options =>
                {
                    options.DatabaseName = webHostBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                });

            });
            webHostBuilder.Services.AddHostedService<ConnectionRouter>();

            var host = webHostBuilder.Build();

            if (host.Environment.IsDevelopment())
            {
                host.UseDeveloperExceptionPage();
            }

            host.UseRouting();

            host.MapHealthChecks("/health");
            host.MapHealthChecks("/alive");

            await host.RunAsync();
        }
    }
}