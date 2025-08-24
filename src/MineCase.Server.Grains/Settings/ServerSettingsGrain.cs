using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Orleans;
using Orleans.Concurrency;
using Orleans.Runtime;

namespace MineCase.Server.Settings
{
    [StatelessWorker]
    internal class ServerSettingsGrain : Grain, IServerSettings
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;
        private ServerSettings _serverSettings;

        public ServerSettingsGrain(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ServerSettingsGrain>();
            _configuration = configuration;
        }

        // read settings from file. EDIT: This shouldnt be needed since the server.json is loaded from the Host DI IConfiguration instead (why were we doing file IO in a grain?)
        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            _serverSettings = _configuration.GetSection("ServerSettings").Get<ServerSettings>();

            if (_serverSettings is null)
                return Task.FromException(new ArgumentNullException(nameof(_serverSettings)));
            return Task.CompletedTask;
        }

        // get settings
        public Task<ServerSettings> GetSettings()
        {
            if (_serverSettings is null)
                return Task.FromException<ServerSettings>(new ArgumentNullException(nameof(_serverSettings)));
            return Task.FromResult(_serverSettings);
        }

        // set settings
        public Task SetSettings(ServerSettings settings)
        {
            _serverSettings = settings;
            return Task.CompletedTask;
        }
    }
}
