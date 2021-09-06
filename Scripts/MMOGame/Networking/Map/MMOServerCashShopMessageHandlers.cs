using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int cash = 0;
            List<int> cashShopItemIds = new List<int>();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Cannot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = playerCharacter.UserId
                });
                cash = getCashResp.Cash;
                // Set cash shop item ids
                cashShopItemIds.AddRange(GameInstance.CashShopItems.Keys);
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashShopInfoMessage()
                {
                    message = message,
                    cash = cash,
                    cashShopItemIds = cashShopItemIds.ToArray(),
                });
#endif
            await UniTask.Yield();
        }
        public async UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {

#if UNITY_STANDALONE && !CLIENT_BUILD
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int dataId = request.dataId;
            int userCash = 0;
            int userGold = 0;
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Cannot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = playerCharacter.UserId
                });
                userCash = getCashResp.Cash;
                // Get user gold amount
                GoldResp getGoldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
                {
                    UserId = playerCharacter.UserId
                });
                userGold = getGoldResp.Gold;
                CashShopItem cashShopItem;
                if (!GameInstance.CashShopItems.TryGetValue(dataId, out cashShopItem))
                {
                    // Cannot find item
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND;
                }
                else if (userCash < cashShopItem.SellPriceCash)
                {
                    // Not enough cash
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH;
                }
                else if (userGold < cashShopItem.SellPriceGold)
                {
                    // Not enough cash
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD;
                }
                else if (playerCharacter.IncreasingItemsWillOverwhelming(cashShopItem.ReceiveItems))
                {
                    // Cannot carry all rewards
                    message = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                }
                else
                {
                    // Decrease cash amount
                    CashResp changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
                    {
                        UserId = playerCharacter.UserId,
                        ChangeAmount = -cashShopItem.SellPriceCash
                    });
                    userCash = changeCashResp.Cash;
                    playerCharacter.UserCash = userCash;
                    // Decrease gold amount
                    GoldResp changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
                    {
                        UserId = playerCharacter.UserId,
                        ChangeAmount = -cashShopItem.SellPriceGold
                    });
                    userGold = changeGoldResp.Gold;
                    playerCharacter.UserGold = userGold;
                    // Increase character gold
                    playerCharacter.Gold = playerCharacter.Gold.Increase(cashShopItem.ReceiveGold);
                    // Increase currencies
                    playerCharacter.IncreaseCurrencies(cashShopItem.ReceiveCurrencies);
                    // Increase character item
                    if (cashShopItem.ReceiveItems != null && cashShopItem.ReceiveItems.Length > 0)
                    {
                        foreach (ItemAmount receiveItem in cashShopItem.ReceiveItems)
                        {
                            if (receiveItem.item == null || receiveItem.amount <= 0) continue;
                            playerCharacter.AddOrSetNonEquipItems(CharacterItem.Create(receiveItem.item, 1, receiveItem.amount));
                        }
                        playerCharacter.FillEmptySlots();
                    }
                }
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashShopBuyMessage()
                {
                    message = message,
                    dataId = dataId,
                });
#endif
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int cash = 0;
            List<int> cashPackageIds = new List<int>();
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Cannot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = playerCharacter.UserId
                });
                cash = getCashResp.Cash;
                // Set cash package ids
                cashPackageIds.AddRange(GameInstance.CashPackages.Keys);
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageInfoMessage()
                {
                    message = message,
                    cash = cash,
                    cashPackageIds = cashPackageIds.ToArray(),
                });
#endif
            await UniTask.Yield();
        }

        public async UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // TODO: Validate purchasing at server side
            // Set response data
            UITextKeys message = UITextKeys.NONE;
            int dataId = request.dataId;
            int cash = 0;
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Cannot find user
                message = UITextKeys.UI_ERROR_NOT_LOGGED_IN;
            }
            else
            {
                // Get user cash amount
                CashResp getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
                {
                    UserId = playerCharacter.UserId
                });
                cash = getCashResp.Cash;
                CashPackage cashPackage;
                if (!GameInstance.CashPackages.TryGetValue(dataId, out cashPackage))
                {
                    // Cannot find package
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND;
                }
                else
                {
                    // Increase cash amount
                    CashResp changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
                    {
                        UserId = playerCharacter.UserId,
                        ChangeAmount = cashPackage.CashAmount
                    });
                    cash = changeCashResp.Cash;
                    playerCharacter.UserCash = cash;
                }
            }
            // Send response message
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseCashPackageBuyValidationMessage()
                {
                    message = message,
                    dataId = dataId,
                    cash = cash,
                });
#endif
            await UniTask.Yield();
        }
    }
}
