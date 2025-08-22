using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MineCase.Serialization.Serializers;
using Orleans;
using Orleans.ApplicationParts;
using Orleans.Configuration;
using Orleans.Hosting;
using System.Net;
using System.Net.Sockets;
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

        static async Task Main(string[] args)
        {
            var createShardKey = false;
            Serializers.RegisterAll();

            var hostBuilder = new HostBuilder()
                .ConfigureWebHostDefaults(conf => { conf.UseStartup<Startup>(); })
                .UseServiceProviderFactory(x => new AutofacServiceProviderFactory(ConfigureAutofac))
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureServices(ConfigureServices)
                .ConfigureLogging(ConfigureLogging)
                .UseConsoleLifetime()
                .UseOrleans((Microsoft.Extensions.Hosting.HostBuilderContext context, ISiloBuilder builder) =>
                {
                    builder.Configure<ClusterOptions>(options =>
                    {
                        options.ClusterId = "dev";
                        options.ServiceId = "MineCaseService";
                    })
                    .Configure<SchedulingOptions>(options =>
                    {
                        options.AllowCallChainReentrancy = true;
                        options.PerformDeadlockDetection = true;
                    })
                    .ConfigureEndpoints(siloPort: GetAvailablePort(), gatewayPort: GetAvailablePort())
                    .UseMongoDBClient(context.Configuration.GetSection("persistenceOptions")["connectionString"])
                    .AddSimpleMessageStreamProvider("JobsProvider")
                    .AddSimpleMessageStreamProvider("TransientProvider")
                    .UseMongoDBReminders(options =>
                    {
                        options.DatabaseName = context.Configuration.GetSection("persistenceOptions")["databaseName"];
                        options.CreateShardKeyForCosmos = createShardKey;
                    })
                    .UseMongoDBClustering(c =>
                    {
                        c.DatabaseName = context.Configuration.GetSection("persistenceOptions")["databaseName"];
                        c.CreateShardKeyForCosmos = createShardKey;
                        // c.UseJsonFormat = true;
                    })
                    .UseDashboard(config => { config.HostSelf = false; })
                    .ConfigureApplicationParts(ConfigureApplicationParts)
                    .AddMongoDBGrainStorageAsDefault(c => c.Configure(options =>
                    {
                        options.DatabaseName = context.Configuration.GetSection("persistenceOptions")["databaseName"];
                        options.CreateShardKeyForCosmos = createShardKey;
                    }))
                    .AddMongoDBGrainStorage("PubSubStore", options =>
                    {
                        options.DatabaseName = context.Configuration.GetSection("persistenceOptions")["databaseName"];
                        options.CreateShardKeyForCosmos = createShardKey;
                    });
                });

            var host = hostBuilder.Build();
            Serializers.RegisterAll(host.Services);
            await host.RunAsync();
        }

        private static void ConfigureApplicationParts(IApplicationPartManager parts)
        {
            parts.AddFromApplicationBaseDirectory().WithReferences();
        }
    }
}