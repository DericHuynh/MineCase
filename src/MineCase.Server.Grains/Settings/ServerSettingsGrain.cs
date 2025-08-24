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
        private ServerSettings _settings;
        private readonly ILogger _logger;

        public ServerSettingsGrain(ILoggerFactory loggerFactory, IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<ServerSettingsGrain>();
            _settings = configuration.GetSection("ServerSettings").Get<ServerSettings>();
        }

        // read settings from file. EDIT: This shouldnt be needed since the server.json is loaded from the Host DI IConfiguration instead (why were we doing file IO in a grain?)
        // public override async Task OnActivateAsync()
        // {
        //    string settingsFile = await ReadSettingsAsString("server.json");
        //    try
        //    {
        //        _settings = JsonConvert.DeserializeObject<ServerSettings>(settingsFile);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError(default(EventId), e, e.Message);
        //    }
        // }

        // get settings
        public Task<ServerSettings> GetSettings() => Task.FromResult(_settings);

        // set settings
        public Task SetSettings(ServerSettings settings)
        {
            _settings = settings;
            return Task.CompletedTask;
        }
    }
}
