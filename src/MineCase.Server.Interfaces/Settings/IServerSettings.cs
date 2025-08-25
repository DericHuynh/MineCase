using System.Threading.Tasks;
using MineCase.Server.Health_Checks;
using Orleans;

namespace MineCase.Server.Settings
{
    public interface IServerSettings : IGrainWithIntegerKey, IGrainHealthCheck
    {
        // get settings
        Task<ServerSettings> GetSettings();

        // set settings
        Task SetSettings(ServerSettings settings);
    }
}
