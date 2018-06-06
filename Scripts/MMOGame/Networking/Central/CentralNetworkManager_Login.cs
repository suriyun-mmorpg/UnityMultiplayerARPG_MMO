using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager
    {
        public uint RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserLoginMessage();
            message.username = username;
            message.password = password;
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestUserLogin, message, callback);
        }

        public uint RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserRegisterMessage();
            message.username = username;
            message.password = password;
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestUserRegister, message, callback);
        }

        public uint RequestUserLogout(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestUserLogout, message, callback);
        }

        protected void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var userId = string.Empty;
            if (!database.ValidateUserLogin(message.username, message.password, out userId))
                error = ResponseUserLoginMessage.Error.InvalidUsernameOrPassword;
            else if (userPeersByUserId.ContainsKey(userId))
                error = ResponseUserLoginMessage.Error.AlreadyLogin;
            else
            {
                var userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.userId = userId;
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[peer.ConnectId] = userPeerInfo;
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
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
            else if (database.FindUsername(username) > 0)
                error = ResponseUserRegisterMessage.Error.UsernameAlreadyExisted;
            else
                database.CreateUserLogin(username, password);
            var responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseUserRegister, responseMessage);
        }

        protected void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var responseMessage = new BaseAckMessage();
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(peer.ConnectId);
            }
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseUserLogout, responseMessage);
        }

        protected void HandleResponseUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseUserLoginMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseUserRegisterMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }
    }
}
