using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestCashShopInfo(string userId, string accessToken, AckMessageCallback callback)
        {
            var message = new RequestCashShopInfoMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestCashShopInfo, message, callback);
        }

        public uint RequestCashShopBuy(string userId, string accessToken, int dataId, AckMessageCallback callback)
        {
            var message = new RequestCashShopBuyMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            message.dataId = dataId;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestCashShopBuy, message, callback);
        }

        protected async void HandleRequestCashShopInfo(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashShopInfoMessage>();
            var error = ResponseCashShopInfoMessage.Error.None;
            var userId = message.userId;
            var accessToken = message.accessToken;
            if (!await Database.ValidateAccessToken(userId, accessToken))
                error = ResponseCashShopInfoMessage.Error.InvalidAccessToken;
            else
            {
                // Request cash, send item info messages to map server
            }
            var responseMessage = new ResponseCashShopInfoMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopInfoMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseCashShopInfo, responseMessage);
        }

        protected async void HandleRequestCashShopBuy(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCashShopBuyMessage>();
            var error = ResponseCashShopBuyMessage.Error.None;
            var userId = message.userId;
            var accessToken = message.accessToken;
            if (!await Database.ValidateAccessToken(userId, accessToken))
                error = ResponseCashShopBuyMessage.Error.InvalidAccessToken;
            else
            {
                // Request cash, reduce, send item info messages to map server
            }
            var responseMessage = new ResponseCashShopBuyMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCashShopBuyMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseCashShopBuy, responseMessage);
        }
    }
}
