using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Options;
using MineCase.Buffers;
using MineCase.Gateway.Network;
using MineCase.Protocol;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Buffers;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace MineCase.Gateway
{
    internal static partial class Program
    {
        private static async Task Main(string[] args)
        {
            var appBuilder = Host.CreateApplicationBuilder(args);

            // Access configuration, services, and logging directly from the builder
            appBuilder.Configuration.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json", false, false);

            appBuilder.Services.AddLogging();
            appBuilder.Services.AddSingleton<ConnectionRouter>();
            appBuilder.Services.AddSingleton<IPacketCompress, PacketCompress>();
            appBuilder.Services.AddTransient<ClientSession>();
            appBuilder.Services.AddHostedService<ConnectionRouter>();
            appBuilder.Services.AddMongoDBGatewayListProvider(op =>
            {
                op.DatabaseName = appBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
            });

            appBuilder.Services.AddOrleansClient(builder =>
            {
                // The client builder still uses a context parameter, but the
                // configuration is now available directly on appBuilder.
                builder.UseMongoDBClient(appBuilder.Configuration.GetConnectionString("mongodb"));
                builder.UseMongoDBClustering(options =>
                {
                    options.DatabaseName = appBuilder.Configuration.GetSection("persistenceOptions")["databaseName"];
                });
                builder.Configure<ClusterOptions>(options =>
                {
                    options.ClusterId = "dev";
                    options.ServiceId = "MineCaseService";
                });
            });

            // Object pools
            appBuilder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
            appBuilder.Services.AddSingleton<ObjectPool<UncompressedPacket>>(s =>
            {
                var provider = s.GetRequiredService<ObjectPoolProvider>();
                return provider.Create<UncompressedPacket>();
            });
            appBuilder.Services.AddSingleton<IBufferPool<byte>>(s => new BufferPool<byte>(ArrayPool<byte>.Shared));

            var app = appBuilder.Build();
            await app.RunAsync();
        }
    }
}