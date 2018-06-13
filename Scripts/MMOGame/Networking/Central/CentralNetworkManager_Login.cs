using LiteNetLib;
using LiteNetLibManager;
using System;
using System.Text.RegularExpressions;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager
    {
        public uint RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserLoginMessage();
            message.username = username;
            message.password = password;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestUserLogin, message, callback);
        }

        public uint RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserRegisterMessage();
            message.username = username;
            message.password = password;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestUserRegister, message, callback);
        }

        public uint RequestUserLogout(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestUserLogout, message, callback);
        }

        public uint RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback callback)
        {
            var message = new RequestValidateAccessTokenMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestValidateAccessToken, message, callback);
        }

        protected async void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var userId = await Database.ValidateUserLogin(message.username, message.password);
            var accessToken = string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                error = ResponseUserLoginMessage.Error.InvalidUsernameOrPassword;
                userId = string.Empty;
            }
            else if (userPeersByUserId.ContainsKey(userId) || MapContainsUser(userId))
            {
                error = ResponseUserLoginMessage.Error.AlreadyLogin;
                userId = string.Empty;
            }
            else
            {
                var userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.peer = peer;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[peer.ConnectId] = userPeerInfo;
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected async void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestUserRegisterMessage>();
            var error = ResponseUserRegisterMessage.Error.None;
            var username = message.username;
            var password = message.password;
            if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooShortUsername;
            else if (username.Length > maxUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooLongUsername;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                error = ResponseUserRegisterMessage.Error.TooShortPassword;
            else if (await Database.FindUsername(username) > 0)
                error = ResponseUserRegisterMessage.Error.UsernameAlreadyExisted;
            else
                await Database.CreateUserLogin(username, password);
            var responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseUserRegister, responseMessage);
        }

        protected async void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(peer.ConnectId);
                await Database.UpdateAccessToken(userPeerInfo.userId, string.Empty);
            }
            var responseMessage = new BaseAckMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseUserLogout, responseMessage);
        }

        protected async void HandleRequestValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestValidateAccessTokenMessage>();
            var error = ResponseValidateAccessTokenMessage.Error.None;
            var userId = message.userId;
            var accessToken = message.accessToken;
            if (!await Database.ValidateAccessToken(userId, accessToken))
            {
                error = ResponseValidateAccessTokenMessage.Error.InvalidAccessToken;
                userId = string.Empty;
                accessToken = string.Empty;
            }
            else
            {
                CentralUserPeerInfo userPeerInfo;
                if (userPeersByUserId.TryGetValue(userId, out userPeerInfo))
                {
                    userPeersByUserId.Remove(userPeerInfo.userId);
                    userPeers.Remove(userPeerInfo.peer.ConnectId);
                }
                userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.peer = peer;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[peer.ConnectId] = userPeerInfo;
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseValidateAccessTokenMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseValidateAccessToken, responseMessage);
        }

        protected void HandleResponseUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseUserLoginMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseUserRegisterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseValidateAccessTokenMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }
    }
}
