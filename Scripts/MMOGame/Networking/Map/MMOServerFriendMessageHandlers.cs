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

        public async UniTaskVoid HandleRequestFindCharacters(RequestHandlerData requestHandler, RequestFindCharactersMessage request, RequestProceedResultDelegate<ResponseFindCharactersMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out _))
            {
                result.Invoke(AckResponseCode.Error, new ResponseFindCharactersMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
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

        public async UniTaskVoid HandleRequestGetFriends(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseGetFriendsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseGetFriendsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.ReadFriendsAsync(new ReadFriendsReq()
            {
                CharacterId = playerCharacter.Id,
            });
            result.Invoke(AckResponseCode.Success, new ResponseGetFriendsMessage()
            {
                friends = resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>(),
            });
#endif
        }

        public async UniTaskVoid HandleRequestAddFriend(RequestHandlerData requestHandler, RequestAddFriendMessage request, RequestProceedResultDelegate<ResponseAddFriendMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseAddFriendMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.CreateFriendAsync(new CreateFriendReq()
            {
                Character1Id = playerCharacter.Id,
                Character2Id = request.friendId,
            });
            GameInstance.ServerGameMessageHandlers.SendSetFriends(requestHandler.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
            result.Invoke(AckResponseCode.Success, new ResponseAddFriendMessage()
            {
                message = UITextKeys.UI_FRIEND_ADDED,
            });
#endif
        }

        public async UniTaskVoid HandleRequestRemoveFriend(RequestHandlerData requestHandler, RequestRemoveFriendMessage request, RequestProceedResultDelegate<ResponseRemoveFriendMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseRemoveFriendMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            ReadFriendsResp resp = await DbServiceClient.DeleteFriendAsync(new DeleteFriendReq()
            {
                Character1Id = playerCharacter.Id,
                Character2Id = request.friendId
            });
            GameInstance.ServerGameMessageHandlers.SendSetFriends(requestHandler.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
            result.Invoke(AckResponseCode.Success, new ResponseRemoveFriendMessage()
            {
                message = UITextKeys.UI_FRIEND_REMOVED,
            });
#endif
        }
    }
}
