using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public partial class CentralNetworkManager
    {
        public uint RequestCharacters(AckMessageCallback callback)
        {
            var message = new RequestCharactersMessage();
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestCharacters, message, callback);
        }

        public uint RequestCreateCharacter(string characterName, string databaseId, AckMessageCallback callback)
        {
            var message = new RequestCreateCharacterMessage();
            message.characterName = characterName;
            message.databaseId = databaseId;
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestCreateCharacter, message, callback);
        }

        public uint RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            var message = new RequestDeleteCharacterMessage();
            message.characterId = characterId;
            return SendAckPacket(SendOptions.ReliableUnordered, Client.Peer, CentralMsgTypes.RequestDeleteCharacter, message, callback);
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
                characters = await database.ReadCharacters(userPeerInfo.userId);
            var responseMessage = new ResponseCharactersMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCharactersMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.characters = characters;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseCharacters, responseMessage);
        }

        protected async void HandleRequestCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<RequestCreateCharacterMessage>();
            var error = ResponseCreateCharacterMessage.Error.None;
            var characterName = message.characterName;
            var databaseId = message.databaseId;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(peer.ConnectId, out userPeerInfo))
                error = ResponseCreateCharacterMessage.Error.NotLoggedin;
            else if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooShortCharacterName;
            else if (characterName.Length > maxCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooLongCharacterName;
            else if (!GameInstance.PlayerCharacters.ContainsKey(databaseId))
                error = ResponseCreateCharacterMessage.Error.InvalidData;
            else if (await database.FindCharacterName(characterName) > 0)
                error = ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted;
            else
            {
                var characterId = System.Guid.NewGuid().ToString();
                var characterData = new PlayerCharacterData();
                characterData.Id = characterId;
                characterData.SetNewCharacterData(characterName, databaseId);
                await database.CreateCharacter(userPeerInfo.userId, characterData);
            }
            var responseMessage = new ResponseCreateCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseCreateCharacter, responseMessage);
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
                await database.DeleteCharacter(userPeerInfo.userId, message.characterId);
            var responseMessage = new ResponseDeleteCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseDeleteCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            SendPacket(SendOptions.ReliableUnordered, peer, CentralMsgTypes.ResponseDeleteCharacter, responseMessage);
        }

        protected void HandleResponseCharacters(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCharactersMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseCreateCharacterMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ResponseDeleteCharacterMessage>();
            var ackId = message.ackId;
            TriggerAck(ackId, message.responseCode, message);
        }
    }
}
