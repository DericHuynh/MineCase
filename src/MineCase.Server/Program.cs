using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Microsoft.IO;
using MineCase.Abstractions.Constants;
using MineCase.Buffers;
using MineCase.Protocol;
using MineCase.Protocol.Play;
using MineCase.Serialization.Serializers;
using MineCase.Server.Network;
using MineCase.Server.Settings;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Providers;
using Orleans.Providers.MongoDB.Configuration;
using Orleans.Serialization;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;

namespace MineCase.Server;

internal static partial class Program
{
    private static async Task Main(string[] args)
    {
        const bool createShardKey = false;
        Serializers.RegisterAll();

        var appBuilder = Host.CreateApplicationBuilder(args);
        string dbname = appBuilder.Configuration.GetSection("PersistenceSettings")["DatabaseName"];
        string mongoConnectionStr = appBuilder.Configuration.GetConnectionString("minecase");

        appBuilder.AddServiceDefaults();

        appBuilder.ConfigureContainer(new AutofacServiceProviderFactory(), builder =>
        {
            var assemblies = new List<Assembly>();
            assemblies
                .AddEngine()
                .AddInterfaces()
                .AddGrains();
            builder.RegisterAssemblyModules(assemblies.ToArray());
        });

        // Add both Silo and Gateway services to the same container
        appBuilder.Services.AddOptions();
        appBuilder.Services.AddSingleton<RecyclableMemoryStreamManager>();
        appBuilder.Services.Configure<PersistenceOptions>(bOptions => bOptions.ConnectionString = mongoConnectionStr);

        // Gateway specific services
        appBuilder.Services.AddSingleton<ConnectionRouter>();
        appBuilder.Services.AddSingleton<IPacketCompress, PacketCompress>();
        appBuilder.Services.AddTransient<ClientSession>();
        appBuilder.Services.AddHostedService<ConnectionRouter>();
        appBuilder.Services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();
        appBuilder.Services.AddSingleton<ObjectPool<UncompressedPacket>>(s =>
        {
            var provider = s.GetRequiredService<ObjectPoolProvider>();
            return provider.Create<UncompressedPacket>();
        });
        appBuilder.Services.AddSingleton<IBufferPool<byte>>(s => new BufferPool<byte>(ArrayPool<byte>.Shared));

        //Configure MongoDB and Orleans Integrations
        appBuilder.Services.AddMongoDBClient((serviceProvider) =>
        {
            //This is to configure OpenTelemetry for the Mongo database
            var settings = MongoClientSettings.FromConnectionString(mongoConnectionStr);
            var instrumentationOptions = new InstrumentationOptions 
            { 
                CaptureCommandText = true,
                ShouldStartActivity = (@event) => !"collectionToIgnore".Equals(@event.GetCollectionName())
            };
            settings.ClusterConfigurator = cb => cb.Subscribe(new DiagnosticsActivityEventSubscriber(instrumentationOptions));
            return settings;
        });
        appBuilder.Services.AddMongoDBReminders(options =>
        {
            options.DatabaseName = dbname;
            options.CreateShardKeyForCosmos = createShardKey;
        });
        appBuilder.Services.AddMongoDBGatewayListProvider(c =>
        {
            c.DatabaseName = dbname;
            c.CreateShardKeyForCosmos = createShardKey;
        });
        appBuilder.Services.AddMongoDBMembershipTable(c =>
        {
            c.DatabaseName = dbname;
            c.CreateShardKeyForCosmos = createShardKey;
        });
        appBuilder.Services.AddMongoDBGrainStorageAsDefault(c => c.Configure(options =>
        {
            options.DatabaseName = dbname;
            options.CreateShardKeyForCosmos = createShardKey;
        }));
        appBuilder.Services.AddMongoDBGrainStorage("PubSubStore", options =>
        {
            options.DatabaseName = dbname;
            options.CreateShardKeyForCosmos = createShardKey;
        });

        appBuilder.UseOrleans(siloBuilder =>
        {
            siloBuilder.UseDashboard();
            siloBuilder.AddActivityPropagation();
            siloBuilder.Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "MineCaseService";
            });
            siloBuilder.ConfigureEndpoints(siloPort: 11111, gatewayPort: 30000);

            siloBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviders.JobsProvider, c => c.ConfigurePartitioning());
            siloBuilder.AddMemoryStreams<DefaultMemoryMessageBodySerializer>(StreamProviders.TransientProvider, c => c.ConfigurePartitioning());
        });

        var app = appBuilder.Build();

        Serializers.RegisterAll(app.Services);

        await app.RunAsync();
    }
}