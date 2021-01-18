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
            UITextKeys error = UITextKeys.NONE;
            List<PlayerCharacterData> characters = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
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
                error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
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
            UITextKeys error = UITextKeys.NONE;
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
                error = UITextKeys.UI_ERROR_CHARACTER_NAME_EXISTED;
            else if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            else if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
                error = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT;
            else if (characterName.Length > maxCharacterNameLength)
                error = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG;
            else if (!GameInstance.PlayerCharacters.ContainsKey(dataId) ||
                !GameInstance.PlayerCharacterEntities.ContainsKey(entityId) ||
                (GameInstance.Factions.Count > 0 && !GameInstance.Factions.ContainsKey(factionId)))
            {
                // If there is factions, it must have faction with the id stored in faction dictionary
                error = UITextKeys.UI_ERROR_INVALID_DATA;
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
                error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
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
            UITextKeys error = UITextKeys.NONE;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
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
                error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
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
            UITextKeys error = UITextKeys.NONE;
            CentralServerPeerInfo mapServerPeerInfo = default;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            else
            {
                CharacterResp characterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
                {
                    UserId = userPeerInfo.userId,
                    CharacterId = request.characterId
                });
                PlayerCharacterData character = characterResp.CharacterData.FromByteString<PlayerCharacterData>();
                if (character == null)
                    error = UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY;
            }
            AckResponseCode responseCode = error == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error;
            ResponseSelectCharacterMessage response = new ResponseSelectCharacterMessage();
            response.error = error;
            if (error != UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY)
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
