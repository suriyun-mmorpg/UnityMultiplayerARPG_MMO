using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestCharacters(AckMessageCallback callback)
        {
            var message = new RequestCharactersMessage();
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestCharacters, message, callback);
        }

        public uint RequestCreateCharacter(string characterName, int dataId, AckMessageCallback callback)
        {
            var message = new RequestCreateCharacterMessage();
            message.characterName = characterName;
            message.dataId = dataId;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestCreateCharacter, message, callback);
        }

        public uint RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            var message = new RequestDeleteCharacterMessage();
            message.characterId = characterId;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestDeleteCharacter, message, callback);
        }

        public uint RequestSelectCharacter(string characterId, AckMessageCallback callback)
        {
            var message = new RequestSelectCharacterMessage();
            message.characterId = characterId;
            return Client.SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, MMOMessageTypes.RequestSelectCharacter, message, callback);
        }

        protected async void HandleRequestCharacters(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCharactersMessage>();
            var error = ResponseCharactersMessage.Error.None;
            List<PlayerCharacterData> characters = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
                error = ResponseCharactersMessage.Error.NotLoggedin;
            else
                characters = await Database.ReadCharacters(userPeerInfo.userId);
            var responseMessage = new ResponseCharactersMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCharactersMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.characters = characters;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseCharacters, responseMessage);
        }

        protected async void HandleRequestCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCreateCharacterMessage>();
            var error = ResponseCreateCharacterMessage.Error.None;
            var characterName = message.characterName;
            var databaseId = message.dataId;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
                error = ResponseCreateCharacterMessage.Error.NotLoggedin;
            else if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooShortCharacterName;
            else if (characterName.Length > maxCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooLongCharacterName;
            else if (!GameInstance.PlayerCharacters.ContainsKey(databaseId))
                error = ResponseCreateCharacterMessage.Error.InvalidData;
            else if (await Database.FindCharacterName(characterName) > 0)
                error = ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted;
            else
            {
                var characterId = GenericUtils.GetUniqueId();
                var characterData = new PlayerCharacterData();
                characterData.Id = characterId;
                characterData.SetNewCharacterData(characterName, databaseId);
                await Database.CreateCharacter(userPeerInfo.userId, characterData);
            }
            var responseMessage = new ResponseCreateCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseCreateCharacter, responseMessage);
        }

        protected async void HandleRequestDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestDeleteCharacterMessage>();
            var error = ResponseDeleteCharacterMessage.Error.None;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
                error = ResponseDeleteCharacterMessage.Error.NotLoggedin;
            else
                await Database.DeleteCharacter(userPeerInfo.userId, message.characterId);
            var responseMessage = new ResponseDeleteCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseDeleteCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseDeleteCharacter, responseMessage);
        }

        protected async void HandleRequestSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestSelectCharacterMessage>();
            var error = ResponseSelectCharacterMessage.Error.None;
            CentralServerPeerInfo mapServerPeerInfo = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
                error = ResponseSelectCharacterMessage.Error.NotLoggedin;
            else
            {
                var character = await Database.ReadCharacter(userPeerInfo.userId, message.characterId, false, false, false, false, false, false, false, false);
                if (character == null)
                    error = ResponseSelectCharacterMessage.Error.InvalidCharacterData;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = ResponseSelectCharacterMessage.Error.MapNotReady;
            }
            var responseMessage = new ResponseSelectCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseSelectCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            if (mapServerPeerInfo != null)
            {
                responseMessage.sceneName = mapServerPeerInfo.extra;
                responseMessage.networkAddress = mapServerPeerInfo.networkAddress;
                responseMessage.networkPort = mapServerPeerInfo.networkPort;
                responseMessage.connectKey = mapServerPeerInfo.connectKey;
            }
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableUnordered, peer, MMOMessageTypes.ResponseSelectCharacter, responseMessage);
        }

        protected void HandleResponseCharacters(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCharactersMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCreateCharacterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseDeleteCharacterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peerHandler = messageHandler.peerHandler;
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseSelectCharacterMessage>();
            var ackId = message.ackId;
            peerHandler.TriggerAck(ackId, message.responseCode, message);
        }
    }
}
