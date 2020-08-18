using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestCharacters(AckMessageCallback callback)
        {
            RequestCharactersMessage message = new RequestCharactersMessage();
            return ClientSendRequest(MMOMessageTypes.RequestCharacters, message, callback);
        }

        public uint RequestCreateCharacter(PlayerCharacterData characterData, AckMessageCallback callback)
        {
            RequestCreateCharacterMessage message = new RequestCreateCharacterMessage();
            message.characterName = characterData.CharacterName;
            message.dataId = characterData.DataId;
            message.entityId = characterData.EntityId;
            message.factionId = characterData.FactionId;
            return ClientSendRequest(MMOMessageTypes.RequestCreateCharacter, message, callback, (writer) => SerializeCreateCharacterExtra(characterData, writer));
        }

        private void SerializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataWriter writer)
        {
            this.InvokeInstanceDevExtMethods("SerializeCreateCharacterExtra", characterData, writer);
        }

        public uint RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            RequestDeleteCharacterMessage message = new RequestDeleteCharacterMessage();
            message.characterId = characterId;
            return ClientSendRequest(MMOMessageTypes.RequestDeleteCharacter, message, callback);
        }

        public uint RequestSelectCharacter(string characterId, AckMessageCallback callback)
        {
            RequestSelectCharacterMessage message = new RequestSelectCharacterMessage();
            message.characterId = characterId;
            return ClientSendRequest(MMOMessageTypes.RequestSelectCharacter, message, callback);
        }

        protected void HandleRequestCharacters(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestCharactersRoutine(messageHandler).Forget();
        }

        private async UniTaskVoid HandleRequestCharactersRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCharactersMessage message = messageHandler.ReadMessage<RequestCharactersMessage>();
            ResponseCharactersMessage.Error error = ResponseCharactersMessage.Error.None;
            List<PlayerCharacterData> characters = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseCharactersMessage.Error.NotLoggedin;
            else
            {
                CharactersResp charactersResp = await DbServiceClient.ReadCharactersAsync(new ReadCharactersReq()
                {
                    UserId = userPeerInfo.userId
                });
                characters = DatabaseServiceUtils.MakeListFromRepeatedByteString<PlayerCharacterData>(charactersResp.List);
            }
            ResponseCharactersMessage responseMessage = new ResponseCharactersMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCharactersMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.characters = characters;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseCharacters, responseMessage);
        }

        protected void HandleRequestCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestCreateCharacterRoutine(messageHandler).Forget();
        }

        private async UniTaskVoid HandleRequestCreateCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCreateCharacterMessage message = messageHandler.ReadMessage<RequestCreateCharacterMessage>();
            ResponseCreateCharacterMessage.Error error = ResponseCreateCharacterMessage.Error.None;
            string characterName = message.characterName;
            int dataId = message.dataId;
            int entityId = message.entityId;
            int factionId = message.factionId;
            CentralUserPeerInfo userPeerInfo;
            FindCharacterNameResp findCharacterNameResp = await DbServiceClient.FindCharacterNameAsync(new FindCharacterNameReq()
            {
                CharacterName = characterName
            });
            if (findCharacterNameResp.FoundAmount > 0)
                error = ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted;
            else if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseCreateCharacterMessage.Error.NotLoggedin;
            else if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooShortCharacterName;
            else if (characterName.Length > maxCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooLongCharacterName;
            else if (!GameInstance.PlayerCharacters.ContainsKey(dataId) ||
                !GameInstance.PlayerCharacterEntities.ContainsKey(entityId) ||
                (GameInstance.Factions.Count > 0 && !GameInstance.Factions.ContainsKey(factionId)))
            {
                // If there is factions, it must have faction with the id stored in faction dictionary
                error = ResponseCreateCharacterMessage.Error.InvalidData;
            }
            else
            {
                string characterId = GenericUtils.GetUniqueId();
                PlayerCharacterData characterData = new PlayerCharacterData();
                characterData.Id = characterId;
                characterData.SetNewPlayerCharacterData(characterName, dataId, entityId);
                characterData.FactionId = factionId;
                DeserializeCreateCharacterExtra(characterData, messageHandler.reader);
                await DbServiceClient.CreateCharacterAsync(new CreateCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterData = characterData.ToByteString()
                });
            }
            ResponseCreateCharacterMessage responseMessage = new ResponseCreateCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseCreateCharacter, responseMessage);
        }

        private void DeserializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataReader reader)
        {
            this.InvokeInstanceDevExtMethods("DeserializeCreateCharacterExtra", characterData, reader);
        }

        protected void HandleRequestDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestDeleteCharacterRoutine(messageHandler).Forget();
        }

        private async UniTaskVoid HandleRequestDeleteCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestDeleteCharacterMessage message = messageHandler.ReadMessage<RequestDeleteCharacterMessage>();
            ResponseDeleteCharacterMessage.Error error = ResponseDeleteCharacterMessage.Error.None;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseDeleteCharacterMessage.Error.NotLoggedin;
            else
            {
                await DbServiceClient.DeleteCharacterAsync(new DeleteCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterId = message.characterId
                });
            }
            ResponseDeleteCharacterMessage responseMessage = new ResponseDeleteCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseDeleteCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseDeleteCharacter, responseMessage);
        }

        protected void HandleRequestSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestSelectCharacterRoutine(messageHandler).Forget();
        }

        private async UniTaskVoid HandleRequestSelectCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestSelectCharacterMessage message = messageHandler.ReadMessage<RequestSelectCharacterMessage>();
            ResponseSelectCharacterMessage.Error error = ResponseSelectCharacterMessage.Error.None;
            CentralServerPeerInfo mapServerPeerInfo = default(CentralServerPeerInfo);
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseSelectCharacterMessage.Error.NotLoggedin;
            else
            {
                CharacterResp characterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterId = message.characterId
                });
                PlayerCharacterData character = characterResp.CharacterData.FromByteString<PlayerCharacterData>();
                if (character == null)
                    error = ResponseSelectCharacterMessage.Error.InvalidCharacterData;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = ResponseSelectCharacterMessage.Error.MapNotReady;
            }
            ResponseSelectCharacterMessage responseMessage = new ResponseSelectCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseSelectCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            if (error != ResponseSelectCharacterMessage.Error.MapNotReady)
            {
                responseMessage.sceneName = mapServerPeerInfo.extra;
                responseMessage.networkAddress = mapServerPeerInfo.networkAddress;
                responseMessage.networkPort = mapServerPeerInfo.networkPort;
            }
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseSelectCharacter, responseMessage);
        }

        protected void HandleResponseCharacters(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCharactersMessage message = messageHandler.ReadMessage<ResponseCharactersMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCreateCharacterMessage message = messageHandler.ReadMessage<ResponseCreateCharacterMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseDeleteCharacterMessage message = messageHandler.ReadMessage<ResponseDeleteCharacterMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseSelectCharacterMessage message = messageHandler.ReadMessage<ResponseSelectCharacterMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }
    }
}
