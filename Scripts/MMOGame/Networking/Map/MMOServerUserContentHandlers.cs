using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerUserContentHandlers : MonoBehaviour, IServerUserContentHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        public async UniTask<System.ValueTuple<UITextKeys, UnlockableContent>> FillUserContentProgressionForUnlocking(string userId, UnlockableContentType type, int dataId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            var (msg, unlockableContent) = await GetUnlockContentProgression(userId, type, dataId);
            if (msg != UITextKeys.NONE)
                return (msg, default);
            if (!GameInstance.TryGetUnlockableContentRequirement(type, dataId, out UnlockRequirement requirement))
                return (UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE, default);
            if (!requirement.isLocked)
                return (UITextKeys.UI_ERROR_CONTENT_IS_UNLOCKED, default);
            if (unlockableContent.unlocked)
                return (UITextKeys.UI_ERROR_CONTENT_IS_UNLOCKED, default);
            if (unlockableContent.progression >= requirement.progression)
                return (UITextKeys.UI_ERROR_CONTENT_IS_UNLOCKED, default);
            unlockableContent.progression = unlockableContent.progression + requirement.progression;
            DatabaseApiResult resp = await DatabaseClient.UpdateUserUnlockContentAsync(new UpdateUserUnlockContentReq()
            {
                UserId = userId,
                UnlockableContent = unlockableContent,
            });
            if (!resp.IsSuccess)
                return (UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR, default);
            return (UITextKeys.NONE, unlockableContent);
#else
            await UniTask.Yield();
            return (UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE, default);
#endif
        }

        public async UniTask<System.ValueTuple<UITextKeys, UnlockableContent>> ChangeUnlockUserContentProgress(string userId, UnlockableContentType type, int dataId, int changeProgress)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            var result = await DatabaseClient.ChangeUserUnlockContentProgressionAsync(new ChangeUserUnlockContentProgressionReq()
            {
                UserId = userId,
                Type = type,
                DataId = dataId,
                Amount = changeProgress,
            });
            if (!result.IsSuccess)
                return (UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR, default);
            return (UITextKeys.NONE, result.Response.UnlockableContent);
#else
            await UniTask.Yield();
            return (UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE, default);
#endif
        }

        public async UniTask<System.ValueTuple<UITextKeys, UnlockableContent>> GetUnlockContentProgression(string userId, UnlockableContentType type, int dataId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            DatabaseApiResult<UserUnlockContentResp> resp = await DatabaseClient.GetUserUnlockContentAsync(new GetUserUnlockContentReq()
            {
                UserId = userId,
                Type = type,
                DataId = dataId,
            });
            if (!resp.IsSuccess)
                return (UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR, default);
            UnlockableContent unlockableContent = resp.Response.UnlockableContent;
            unlockableContent.type = type;
            unlockableContent.dataId = dataId;
            return (UITextKeys.NONE, unlockableContent);
#else
            await UniTask.Yield();
            return (UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE, default);
#endif
        }

        public async UniTask<System.ValueTuple<UITextKeys, UnlockableContent[]>> GetAvailableContents(string userId, UnlockableContentType type)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            DatabaseApiResult<UserUnlockContentsResp> resp = await DatabaseClient.GetUserUnlockContentsAsync(new GetUserUnlockContentsReq()
            {
                UserId = userId,
                Type = type,
            });
            if (!resp.IsSuccess)
                return (UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR, System.Array.Empty<UnlockableContent>());

            Dictionary<int, UnlockableContent> contents = new Dictionary<int, UnlockableContent>();
            for (int i = 0; i < resp.Response.List.Count; ++i)
            {
                contents[resp.Response.List[i].dataId] = resp.Response.List[i];
            }

            // Filter available contents
            Dictionary<int, UnlockRequirement> unlockableContents = GameInstance.GetUnlockableContentRequirements(type);
            foreach (var kv in unlockableContents)
            {
                if (kv.Value.isLocked && contents.TryGetValue(kv.Key, out UnlockableContent content) && !content.unlocked)
                {
                    contents.Remove(kv.Key);
                }
                else if (!kv.Value.isLocked && !contents.ContainsKey(kv.Key))
                {
                    contents.Add(kv.Key, new UnlockableContent()
                    {
                        type = type,
                        dataId = kv.Key,
                        progression = 0,
                        unlocked = true,
                    });
                }
            }
            unlockableContents.Clear();
            unlockableContents = null;

            return (UITextKeys.NONE, contents.Values.ToArray());
#else
            await UniTask.Yield();
            return (UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE, System.Array.Empty<UnlockableContent>());
#endif
        }
    }
}
