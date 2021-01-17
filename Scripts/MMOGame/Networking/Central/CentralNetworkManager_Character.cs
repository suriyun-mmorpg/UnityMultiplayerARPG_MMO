using System.Collections.Generic;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestCharacters, EmptyMessage.Value, responseDelegate: callback);
        }

        public bool RequestCreateCharacter(PlayerCharacterData characterData, ResponseDelegate<ResponseCreateCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestCreateCharacter,
                new RequestCreateCharacterMessage()
                {
                    characterName = characterData.CharacterName,
                    dataId = characterData.DataId,
                    entityId = characterData.EntityId,
                    factionId = characterData.FactionId,
                }, (writer) => SerializeCreateCharacterExtra(characterData, writer), responseDelegate: callback);
        }

        private void SerializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataWriter writer)
        {
            this.InvokeInstanceDevExtMethods("SerializeCreateCharacterExtra", characterData, writer);
        }

        public bool RequestDeleteCharacter(string characterId, ResponseDelegate<ResponseDeleteCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestDeleteCharacter, new RequestDeleteCharacterMessage()
            {
                characterId = characterId,
            }, responseDelegate: callback);
        }

        public bool RequestSelectCharacter(string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestSelectCharacter, new RequestSelectCharacterMessage()
            {
                characterId = characterId,
            }, responseDelegate: callback);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestCharacters(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<ResponseCharactersMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
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
            // Response
            result.Invoke(
                error == ResponseCharactersMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCharactersMessage()
                {
                    error = error,
                    characters = characters,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestCreateCharacter(
            RequestHandlerData requestHandler,
            RequestCreateCharacterMessage request,
            RequestProceedResultDelegate<ResponseCreateCharacterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            NetDataReader reader = requestHandler.Reader;
            ResponseCreateCharacterMessage.Error error = ResponseCreateCharacterMessage.Error.None;
            string characterName = request.characterName;
            int dataId = request.dataId;
            int entityId = request.entityId;
            int factionId = request.factionId;
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
                DeserializeCreateCharacterExtra(characterData, reader);
                await DbServiceClient.CreateCharacterAsync(new CreateCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterData = characterData.ToByteString()
                });
            }
            // Response
            result.Invoke(
                error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCreateCharacterMessage()
                {
                    error = error,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void DeserializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataReader reader)
        {
            this.InvokeInstanceDevExtMethods("DeserializeCreateCharacterExtra", characterData, reader);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestDeleteCharacter(
            RequestHandlerData requestHandler,
            RequestDeleteCharacterMessage request,
            RequestProceedResultDelegate<ResponseDeleteCharacterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            ResponseDeleteCharacterMessage.Error error = ResponseDeleteCharacterMessage.Error.None;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseDeleteCharacterMessage.Error.NotLoggedin;
            else
            {
                await DbServiceClient.DeleteCharacterAsync(new DeleteCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterId = request.characterId
                });
            }
            // Response
            result.Invoke(
                error == ResponseDeleteCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseDeleteCharacterMessage()
                {
                    error = error,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestSelectCharacter(
            RequestHandlerData requestHandler,
            RequestSelectCharacterMessage request,
            RequestProceedResultDelegate<ResponseSelectCharacterMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            ResponseSelectCharacterMessage.Error error = ResponseSelectCharacterMessage.Error.None;
            CentralServerPeerInfo mapServerPeerInfo = default;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseSelectCharacterMessage.Error.NotLoggedin;
            else
            {
                CharacterResp characterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterId = request.characterId
                });
                PlayerCharacterData character = characterResp.CharacterData.FromByteString<PlayerCharacterData>();
                if (character == null)
                    error = ResponseSelectCharacterMessage.Error.InvalidCharacterData;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = ResponseSelectCharacterMessage.Error.MapNotReady;
            }
            AckResponseCode responseCode = error == ResponseSelectCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            ResponseSelectCharacterMessage response = new ResponseSelectCharacterMessage();
            response.error = error;
            if (error != ResponseSelectCharacterMessage.Error.MapNotReady)
            {
                response.sceneName = mapServerPeerInfo.extra;
                response.networkAddress = mapServerPeerInfo.networkAddress;
                response.networkPort = mapServerPeerInfo.networkPort;
            }
            // Response
            result.Invoke(responseCode, response);
        }
#endif
    }
}
