using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MineCase.Protocol.Handshaking;
using MineCase.Protocol.Login;
using MineCase.Server.Game;
using MineCase.Server.Settings;
using MineCase.Server.User;
using Orleans;
using Orleans.Concurrency;

namespace MineCase.Server.Network.Login
{
    [Reentrant]
    internal class LoginFlowGrain : Grain, ILoginFlow
    {
        // private bool _useAuthentication = false;
        private const uint CompressPacketThreshold = 256;

        public async Task DispatchPacket(LoginStart packet)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Login Flow", ActivityKind.Server, parentId: Activity.Current?.ParentId);
            if (Activity.Current is not null && Activity.Current.IsAllDataRequested)
            {
                Activity.Current.DisplayName = nameof(LoginStart);
                Activity.Current.SetTag("ClientName", packet.Name);
            }

            var settingsGrain = GrainFactory.GetGrain<IServerSettings>(0);
            var settings = await settingsGrain.GetSettings();
            if (settings.OnlineMode)
            {
                // TODO auth and compression
                var user = GrainFactory.GetGrain<INonAuthenticatedUser>(packet.Name);

                if (await user.GetProtocolVersion() > MineCase.Protocol.Protocol.Version)
                {
                    await SendLoginDisconnect($"{{\"text\":\"Outdated server!I'm still on {Protocol.Protocol.VersionName}\"}}");
                }
                else if (await user.GetProtocolVersion() < MineCase.Protocol.Protocol.Version)
                {
                    await SendLoginDisconnect($"{{\"text\":\"Outdated client!Please use {Protocol.Protocol.VersionName}\"}}");
                }
                else
                {
                    RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                    Random verifyRandom = new Random();
                    Byte[] verifyToken = new byte[64];

                    verifyRandom.NextBytes(verifyToken);
                    var keys = rsa.ExportParameters(false);

                    await SendEncryptionRequest("", keys.Exponent, verifyToken);
                }
            }
            else
            {
                var nonAuthenticatedUser = GrainFactory.GetGrain<INonAuthenticatedUser>(packet.Name);

                if (await nonAuthenticatedUser.GetProtocolVersion() > MineCase.Protocol.Protocol.Version)
                {
                    await SendLoginDisconnect($"{{\"text\":\"Outdated server!I'm still on {Protocol.Protocol.VersionName}\"}}");
                }
                else if (await nonAuthenticatedUser.GetProtocolVersion() < MineCase.Protocol.Protocol.Version)
                {
                    await SendLoginDisconnect($"{{\"text\":\"Outdated client!Please use {Protocol.Protocol.VersionName}\"}}");
                }
                else
                {
                    // TODO refuse him if server is full
                    var user = await nonAuthenticatedUser.GetUser();
                    var world = await user.GetWorld();
                    var gameSession = GrainFactory.GetGrain<IGameSession>(world.GetPrimaryKeyString());

                    await SendSetCompression();

                    var uuid = user.GetPrimaryKey();
                    await SendLoginSuccess(packet.Name, uuid);

                    await user.SetClientPacketSink(GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey()));
                    var packetRouter = GrainFactory.GetGrain<IPacketRouter>(this.GetPrimaryKey());
                    await user.SetPacketRouter(packetRouter);
                    await packetRouter.BindToUser(user);

                    var game = GrainFactory.GetGrain<IGameSession>(world.GetPrimaryKeyString());
                    await game.JoinGame(user);
                }
            }
        }

        public async Task DispatchPacket(EncryptionResponse packet)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Encryption Response", ActivityKind.Server, parentId: Activity.Current?.ParentId);
            if (Activity.Current is not null && Activity.Current.IsAllDataRequested)
            {
                Activity.Current.DisplayName = nameof(EncryptionResponse);
                Activity.Current?.SetTag("SharedSecretLength", packet.SharedSecretLength);
            }

            var settingsGrain = GrainFactory.GetGrain<IServerSettings>(0);
            var settings = await settingsGrain.GetSettings();

            // TODO auth and compression
            var packetRouter = GrainFactory.GetGrain<IPacketRouter>(this.GetPrimaryKey());
            var userName = await packetRouter.GetUserName();

            // mojang request url
            var mojangURL = String.Format("https://sessionserver.mojang.com/session/minecraft/hasJoined?username={0}&serverId={1}&ip={2}", userName, "", settings.ServerIp);

            // success
            var user = await GrainFactory.GetGrain<INonAuthenticatedUser>(userName).GetUser();
            var world = await user.GetWorld();
            var gameSession = GrainFactory.GetGrain<IGameSession>(world.GetPrimaryKeyString());

            var uuid = user.GetPrimaryKey();
            await SendLoginSuccess(userName, uuid);

            await user.SetClientPacketSink(GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey()));
            await user.SetPacketRouter(packetRouter);

            var game = GrainFactory.GetGrain<IGameSession>(world.GetPrimaryKeyString());
            await game.JoinGame(user);
        }

        private async Task SendSetCompression()
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Send SetCompression", ActivityKind.Server, parentId: Activity.Current?.ParentId);

            var sink = GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey());

            SetCompression compressionPacket = new SetCompression
            {
                Threshold = CompressPacketThreshold
            };

            Activity.Current?.AddTag("ServerPacketType", nameof(SetCompression));
            Activity.Current?.AddTag("Threshold", compressionPacket.Threshold);

            await sink.SendPacket(compressionPacket);
        }

        private async Task SendLoginSuccess(string userName, Guid uuid)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Send LoginSuccess", ActivityKind.Server, parentId: Activity.Current?.ParentId);

            var sink = GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey());
            await GrainFactory.GetGrain<IPacketRouter>(this.GetPrimaryKey()).Configuration();

            LoginSuccess loginSuccessPacket = new LoginSuccess
            {
                Username = userName,
                UUID = uuid.ToString()
            };

            Activity.Current?.AddTag("ServerPacketType", nameof(LoginSuccess));
            Activity.Current?.AddTag("Username", loginSuccessPacket.Username);
            Activity.Current?.AddTag("UUID", loginSuccessPacket.UUID);

            await sink.SendPacket(loginSuccessPacket);
        }

        private async Task SendLoginDisconnect(string reason)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Send LoginDisconnect", ActivityKind.Server, parentId: Activity.Current?.ParentId);

            var sink = GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey());

            LoginDisconnect loginDisconnectPacket = new LoginDisconnect
            {
                Reason = reason
            };

            Activity.Current?.AddTag("ServerPacketType", nameof(LoginDisconnect));
            Activity.Current?.AddTag("Reason", loginDisconnectPacket.Reason);

            await sink.SendPacket(loginDisconnectPacket);
        }

        private async Task SendEncryptionRequest(String serverID, byte[] publicKey, byte[] verifyToken)
        {
            using var activity = ActivitySources.NetworkActivitySource.StartActivity("Send EncryptionRequest", ActivityKind.Server, parentId: Activity.Current?.ParentId);

            var sink = GrainFactory.GetGrain<IClientboundPacketSink>(this.GetPrimaryKey());

            EncryptionRequest encryptionRequestPacket = new EncryptionRequest
            {
                ServerID = serverID,
                PublicKeyLength = (uint)publicKey.Length,
                PublicKey = publicKey,
                VerifyTokenLength = (uint)verifyToken.Length,
                VerifyToken = verifyToken
            };

            Activity.Current?.AddTag("ServerPacketType", nameof(EncryptionRequest));
            Activity.Current?.AddTag("ServerID", encryptionRequestPacket.ServerID);
            Activity.Current?.AddTag("PublicKeyLength", encryptionRequestPacket.PublicKeyLength);
            Activity.Current?.AddTag("PublicKey", encryptionRequestPacket.PublicKey);
            Activity.Current?.AddTag("VerifyTokenLength", encryptionRequestPacket.VerifyTokenLength);
            Activity.Current?.AddTag("VerifyToken", encryptionRequestPacket.VerifyToken);

            await sink.SendPacket(encryptionRequestPacket);
        }
    }
}
