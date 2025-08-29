using System;
using System.Threading.Tasks;
using MineCase.Protocol;
using MineCase.Server.User;
using Orleans;
using Orleans.Runtime;

namespace MineCase.Server.Network
{
    public interface IPacketRouter : IGrainWithGuidKey
    {
        /// <summary>
        /// Informs the router which stream to use for sending clientbound packets.
        /// </summary>
        /// <param name="streamId">The GUID of the stream.</param>
        /// <param name="streamNamespace">The namespace of the stream.</param>
        Task SetClientStream(StreamId streamId);

        Task Close();

        Task Configuration();

        Task Play();

        Task BindToUser(IUser user);

        Task<IUser> GetUser();

        Task SetUserName(string name);

        Task<string> GetUserName();
    }
}