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
            return ClientSendRequest(MMORequestTypes.RequestCreateCharacter, new RequestCreateCharacterMessage()
            {
                characterName = characterData.CharacterName,
                dataId = characterData.DataId,
                entityId = characterData.EntityId,
                factionId = characterData.FactionId,
            }, callback, extraRequestSerializer: (writer) => SerializeCreateCharacterExtra(characterData, writer));
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

        protected async UniTaskVoid HandleRequestCharacters(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<ResponseCharactersMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            long connectionId = requestHandler.ConnectionId;
            if (!userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseCharactersMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Get characters from server
            AsyncResponseData<CharactersResp> charactersResp = await DbServiceClient.ReadCharactersAsync(new ReadCharactersReq()
            {
                UserId = userPeerInfo.userId
            });
            if (!charactersResp.IsSuccess)
            {
                result.InvokeError(new ResponseCharactersMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseCharactersMessage()
            {
                characters = charactersResp.Response.List,
            });
#endif
        }

        protected async UniTaskVoid HandleRequestCreateCharacter(
            RequestHandlerData requestHandler,
            RequestCreateCharacterMessage request,
            RequestProceedResultDelegate<ResponseCreateCharacterMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            long connectionId = requestHandler.ConnectionId;
            NetDataReader reader = requestHandler.Reader;
            string characterName = request.characterName.Trim();
            int dataId = request.dataId;
            int entityId = request.entityId;
            int factionId = request.factionId;
            if (!NameValidating.ValidateCharacterName(characterName))
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_CHARACTER_NAME
                });
                return;
            }
            
            int minCharacterNameLength = GameInstance.Singleton.minCharacterNameLength;
            int maxCharacterNameLength = GameInstance.Singleton.maxCharacterNameLength;
            if (characterName.Length < minCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT
                });
                return;
            }

            if (characterName.Length > maxCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG
                });
                return;
            }

            // Validate character name
            AsyncResponseData<FindCharacterNameResp> findCharacterNameResp = await DbServiceClient.FindCharacterNameAsync(new FindCharacterNameReq()
            {
                CharacterName = characterName
            });
            if (!findCharacterNameResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (findCharacterNameResp.Response.FoundAmount > 0)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_EXISTED,
                });
                return;
            }
            if (!userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT,
                });
                return;
            }
            if (characterName.Length > maxCharacterNameLength)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG,
                });
                return;
            }
            if (!GameInstance.PlayerCharacters.ContainsKey(dataId) ||
                !GameInstance.PlayerCharacterEntities.ContainsKey(entityId) ||
                (GameInstance.Factions.Count > 0 && !GameInstance.Factions.ContainsKey(factionId)))
            {
                // If there is factions, it must have faction with the id stored in faction dictionary
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }
            string characterId = GenericUtils.GetUniqueId();
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = characterId;
            characterData.SetNewPlayerCharacterData(characterName, dataId, entityId);
            characterData.FactionId = factionId;
            DeserializeCreateCharacterExtra(characterData, reader);
            AsyncResponseData<CharacterResp> createResp = await DbServiceClient.CreateCharacterAsync(new CreateCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterData = characterData,
            });
            if (!createResp.IsSuccess)
            {
                result.InvokeError(new ResponseCreateCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseCreateCharacterMessage());
#endif
        }

        private void DeserializeCreateCharacterExtra(PlayerCharacterData characterData, NetDataReader reader)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            this.InvokeInstanceDevExtMethods("DeserializeCreateCharacterExtra", characterData, reader);
#endif
        }

        protected async UniTaskVoid HandleRequestDeleteCharacter(
            RequestHandlerData requestHandler,
            RequestDeleteCharacterMessage request,
            RequestProceedResultDelegate<ResponseDeleteCharacterMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            long connectionId = requestHandler.ConnectionId;
            if (!userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseDeleteCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            AsyncResponseData<EmptyMessage> deleteResp = await DbServiceClient.DeleteCharacterAsync(new DeleteCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterId = request.characterId
            });
            if (!deleteResp.IsSuccess)
            {
                result.InvokeError(new ResponseDeleteCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseDeleteCharacterMessage());
#endif
        }

        protected async UniTaskVoid HandleRequestSelectCharacter(
            RequestHandlerData requestHandler,
            RequestSelectCharacterMessage request,
            RequestProceedResultDelegate<ResponseSelectCharacterMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            long connectionId = requestHandler.ConnectionId;
            if (!userPeers.TryGetValue(connectionId, out CentralUserPeerInfo userPeerInfo))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            AsyncResponseData<CharacterResp> characterResp = await DbServiceClient.ReadCharacterAsync(new ReadCharacterReq()
            {
                UserId = userPeerInfo.userId,
                CharacterId = request.characterId
            });
            if (!characterResp.IsSuccess)
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            PlayerCharacterData character = characterResp.Response.CharacterData;
            if (character == null)
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA,
                });
                return;
            }
            if (!ClusterServer.MapServerPeersByMapId.TryGetValue(character.CurrentMapName, out CentralServerPeerInfo mapServerPeerInfo))
            {
                result.InvokeError(new ResponseSelectCharacterMessage()
                {
                    message = UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY,
                });
                return;
            }
            // Response
            result.InvokeSuccess(new ResponseSelectCharacterMessage()
            {
                sceneName = mapServerPeerInfo.extra,
                networkAddress = mapServerPeerInfo.networkAddress,
                networkPort = mapServerPeerInfo.networkPort,
            });
#endif
        }
    }
}
