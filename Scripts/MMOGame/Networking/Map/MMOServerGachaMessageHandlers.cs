using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerGachaMessageHandlers : MonoBehaviour, IServerGachaMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }

        public ClusterClient ClusterClient
        {
            get { return (BaseGameNetworkManager.Singleton as MapNetworkManager).ClusterClient; }
        }
#endif

        public async UniTaskVoid HandleRequestGachaInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseGachaInfoMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseGachaInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            DatabaseApiResult<CashResp> getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
            {
                UserId = userId
            });
            if (!getCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseGachaInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.InvokeSuccess(new ResponseGachaInfoMessage()
            {
                cash = getCashResp.Response.Cash,
                gachaIds = new List<int>(GameInstance.Gachas.Keys),
            });
#endif
        }

        public async UniTaskVoid HandleRequestOpenGacha(RequestHandlerData requestHandler, RequestOpenGachaMessage request, RequestProceedResultDelegate<ResponseOpenGachaMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (!GameInstance.Gachas.TryGetValue(request.dataId, out Gacha gacha))
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }

            int price = request.openMode == GachaOpenMode.Multiple ? gacha.MultipleModeOpenPrice : gacha.SingleModeOpenPrice;
            // Get user cash amount
            DatabaseApiResult<CashResp> getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
            {
                UserId = playerCharacter.UserId
            });
            if (!getCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            int cash = getCashResp.Response.Cash;
            if (cash < price)
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH,
                });
                return;
            }

            int openCount = request.openMode == GachaOpenMode.Multiple ? gacha.MultipleModeOpenCount : 1;
            List<RewardedItem> rewardItems = gacha.GetRandomedItems(openCount);
            if (playerCharacter.IncreasingItemsWillOverwhelming(rewardItems))
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_WILL_OVERWHELMING,
                });
                return;
            }
            // Decrease cash amount
            DatabaseApiResult<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = -price
            });
            if (!changeCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseOpenGachaMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            playerCharacter.UserCash = changeCashResp.Response.Cash;
            // Increase character items
            playerCharacter.IncreaseItems(rewardItems);
            playerCharacter.FillEmptySlots();
            // Send response message
            result.InvokeSuccess(new ResponseOpenGachaMessage()
            {
                dataId = request.dataId,
                rewardItems = rewardItems,
            });
#endif
        }
    }
}
