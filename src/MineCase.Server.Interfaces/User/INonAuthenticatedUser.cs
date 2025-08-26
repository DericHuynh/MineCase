using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Orleans;

namespace MineCase.Server.User
{
    public interface INonAuthenticatedUser : IGrainWithStringKey
    {
        Task<IUser> GetUser();

        Task<int> GetProtocolVersion();

        Task SetProtocolVersion(int version);
    }
}
