using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerUserContentMessageHandlers : MonoBehaviour, IServerUserContentMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        public async UniTaskVoid HandleRequestUnlockContentProgression(RequestHandlerData requestHandler, RequestUnlockContentProgressionMessage request, RequestProceedResultDelegate<ResponseUnlockContentProgressionMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseUnlockContentProgressionMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            var (msg, unlockableContent) = await GameInstance.ServerUserContentHandlers.GetUnlockContentProgression(userId, request.type, request.dataId);
            if (msg != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseUnlockContentProgressionMessage()
                {
                    message = msg,
                });
                return;
            }

            result.InvokeSuccess(new ResponseUnlockContentProgressionMessage()
            {
                unlockableContent = unlockableContent,
            });
#endif
        }

        public async UniTaskVoid HandleRequestAvailableContents(RequestHandlerData requestHandler, RequestAvailableContentsMessage request, RequestProceedResultDelegate<ResponseAvailableContentsMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseAvailableContentsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            var (msg, availableContents) = await GameInstance.ServerUserContentHandlers.GetAvailableContents(userId, request.type);
            if (msg != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseAvailableContentsMessage()
                {
                    message = msg,
                });
                return;
            }

            result.InvokeSuccess(new ResponseAvailableContentsMessage()
            {
                contents = availableContents,
            });
#endif
        }

        public async UniTaskVoid HandleRequestUnlockContent(RequestHandlerData requestHandler, RequestUnlockContentMessage request, RequestProceedResultDelegate<ResponseUnlockContentMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseUnlockContentMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            // TODO: Implement content unlock conditions checking here
            result.InvokeError(new ResponseUnlockContentMessage()
            {
                message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
            });
#endif
        }
    }
}
