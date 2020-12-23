using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerFriendMessageHandlers : MonoBehaviour, IServerFriendMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        public async UniTaskVoid HandleRequestGetFriends(RequestHandlerData requestHandler, RequestGetFriendsMessage request, RequestProceedResultDelegate<ResponseGetFriendsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseGetFriendsMessage()
                {
                    error = ResponseGetFriendsMessage.Error.NotLoggedIn,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.ReadFriendsAsync(new ReadFriendsReq()
            {
                CharacterId = request.characterId
            });
            result.Invoke(AckResponseCode.Success, new ResponseGetFriendsMessage()
            {
                friends = resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>(),
            });
#endif
        }

        public async UniTaskVoid HandleRequestFindCharacters(RequestHandlerData requestHandler, RequestFindCharactersMessage request, RequestProceedResultDelegate<ResponseFindCharactersMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseFindCharactersMessage()
                {
                    error = ResponseFindCharactersMessage.Error.NotLoggedIn,
                });
                return;
            }
            FindCharactersResp resp = await DbServiceClient.FindCharactersAsync(new FindCharactersReq()
            {
                CharacterName = request.characterName
            });
            result.Invoke(AckResponseCode.Success, new ResponseFindCharactersMessage()
            {
                characters = resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>(),
            });
#endif
        }

        public async UniTaskVoid HandleRequestAddFriend(RequestHandlerData requestHandler, RequestAddFriendMessage request, RequestProceedResultDelegate<ResponseAddFriendMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseAddFriendMessage()
                {
                    error = ResponseAddFriendMessage.Error.NotLoggedIn,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.CreateFriendAsync(new CreateFriendReq()
            {
                Character1Id = playerCharacter.Id,
                Character2Id = request.friendId,
            });
            BaseGameNetworkManager.Singleton.SendUpdateFriendsToClient(requestHandler.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
            result.Invoke(AckResponseCode.Success, new ResponseAddFriendMessage());
#endif
        }

        public async UniTaskVoid HandleRequestRemoveFriend(RequestHandlerData requestHandler, RequestRemoveFriendMessage request, RequestProceedResultDelegate<ResponseRemoveFriendMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseRemoveFriendMessage()
                {
                    error = ResponseRemoveFriendMessage.Error.NotLoggedIn,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.DeleteFriendAsync(new DeleteFriendReq()
            {
                Character1Id = playerCharacter.Id,
                Character2Id = request.friendId
            });
            BaseGameNetworkManager.Singleton.SendUpdateFriendsToClient(requestHandler.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
            result.Invoke(AckResponseCode.Success, new ResponseRemoveFriendMessage());
#endif
        }
    }
}
