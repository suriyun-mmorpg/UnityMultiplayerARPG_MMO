using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public abstract class BaseAppNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(MMOMessageTypes.GenericResponse, HandleGenericResponse);
        }

        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(MMOMessageTypes.GenericResponse, HandleGenericResponse);
        }

        protected void HandleGenericResponse(LiteNetLibMessageHandler messageHandler)
        {
            messageHandler.ReadResponse();
        }

        public void ClientSendResponse<TResponse>(TResponse response, System.Action<NetDataWriter> extraSerializer = null)
            where TResponse : BaseAckMessage, new()
        {
            Client.SendPacket(MMOMessageTypes.GenericResponse, response, extraSerializer);
        }

        public void ServerSendResponse<TResponse>(long connectionId, TResponse response, System.Action<NetDataWriter> extraSerializer = null)
            where TResponse : BaseAckMessage, new()
        {
            Server.SendResponse(connectionId, MMOMessageTypes.GenericResponse, response, extraSerializer);
        }
    }
}
