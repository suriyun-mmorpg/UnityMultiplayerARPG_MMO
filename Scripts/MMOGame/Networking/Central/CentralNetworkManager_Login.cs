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

        protected void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestUserRegisterMessage>();
            var responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseUserRegister, responseMessage);
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
    }
}
