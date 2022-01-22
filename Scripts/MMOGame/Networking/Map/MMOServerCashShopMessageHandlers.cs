using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string userId;
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                result.InvokeError(new ResponseCashShopInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            AsyncResponseData<CashResp> getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
            {
                UserId = userId
            });
            if (!getCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseCashShopInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.InvokeSuccess(new ResponseCashShopInfoMessage()
            {
                cash = getCashResp.Response.Cash,
                cashShopItemIds = new List<int>(GameInstance.CashShopItems.Keys),
            });
#endif
        }

        public async UniTaskVoid HandleRequestCashShopBuy(
            RequestHandlerData requestHandler, RequestCashShopBuyMessage request,
            RequestProceedResultDelegate<ResponseCashShopBuyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (request.amount <= 0)
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_DATA,
                });
                return;
            }

            CashShopItem cashShopItem;
            if (!GameInstance.CashShopItems.TryGetValue(request.dataId, out cashShopItem))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_ITEM_NOT_FOUND,
                });
                return;
            }

            if ((request.currencyType == CashShopItemCurrencyType.CASH && cashShopItem.SellPriceCash <= 0) ||
                (request.currencyType == CashShopItemCurrencyType.GOLD && cashShopItem.SellPriceGold <= 0))
            {
                result.InvokeError(new ResponseCashShopBuyMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_ITEM_DATA,
                });
                return;
            }

            int characterGold = playerCharacter.Gold;
            int userCash = playerCharacter.UserCash;
            int priceGold = 0;
            int priceCash = 0;
            int changeCharacterGold = 0;
            int changeUserCash = 0;

            // Validate cash
            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                priceCash = cashShopItem.SellPriceCash * request.amount;
                if (userCash < priceCash)
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_CASH,
                    });
                    return;
                }
                changeUserCash -= priceCash;
            }

            // Validate gold
            if (request.currencyType == CashShopItemCurrencyType.GOLD)
            {
                priceGold = cashShopItem.SellPriceGold * request.amount;
                if (characterGold < priceGold)
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                    });
                    return;
                }
                changeCharacterGold -= priceGold;
            }

            // Increase gold
            if (cashShopItem.ReceiveGold > 0)
                changeCharacterGold += cashShopItem.ReceiveGold * request.amount;

            // Increase items
            List<RewardedItem> rewardItems = new List<RewardedItem>();
            if (cashShopItem.ReceiveItems != null &&
                cashShopItem.ReceiveItems.Length > 0)
            {
                foreach (ItemAmount itemAmount in cashShopItem.ReceiveItems)
                {
                    for (int i = 0; i < request.amount; ++i)
                    {
                        rewardItems.Add(new RewardedItem()
                        {
                            item = itemAmount.item,
                            level = 1,
                            amount = itemAmount.amount,
                            randomSeed = (short)Random.Range(short.MinValue, short.MaxValue),
                        });
                    }
                }
                if (playerCharacter.IncreasingItemsWillOverwhelming(rewardItems))
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_WILL_OVERWHELMING,
                    });
                    return;
                }
            }

            // Update currency
            characterGold += priceGold;
            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                AsyncResponseData<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
                {
                    UserId = playerCharacter.UserId,
                    ChangeAmount = -priceCash
                });
                if (!changeCashResp.IsSuccess)
                {
                    result.InvokeError(new ResponseCashShopBuyMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
                userCash = changeCashResp.Response.Cash;
            }
            playerCharacter.Gold = characterGold;
            playerCharacter.UserCash = userCash;
            playerCharacter.IncreaseItems(rewardItems);
            playerCharacter.FillEmptySlots();

            // Response to client
            result.InvokeSuccess(new ResponseCashShopBuyMessage()
            {
                dataId = request.dataId,
                rewardGold = cashShopItem.ReceiveGold,
                rewardItems = rewardItems,
            });
#endif
        }

        public async UniTaskVoid HandleRequestCashPackageInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashPackageInfoMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string userId;
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                result.InvokeError(new ResponseCashPackageInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            AsyncResponseData<CashResp> getCashResp = await DbServiceClient.GetCashAsync(new GetCashReq()
            {
                UserId = userId
            });
            if (!getCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseCashPackageInfoMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            result.InvokeSuccess(new ResponseCashPackageInfoMessage()
            {
                cash = getCashResp.Response.Cash,
                cashPackageIds = new List<int>(GameInstance.CashPackages.Keys),
            });
#endif
        }

        public async UniTaskVoid HandleRequestCashPackageBuyValidation(
            RequestHandlerData requestHandler, RequestCashPackageBuyValidationMessage request,
            RequestProceedResultDelegate<ResponseCashPackageBuyValidationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // TODO: Validate purchasing at server side
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            CashPackage cashPackage;
            if (!GameInstance.CashPackages.TryGetValue(request.dataId, out cashPackage))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND,
                });
                return;
            }

            AsyncResponseData<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = cashPackage.CashAmount
            });
            if (!changeCashResp.IsSuccess)
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }

            // Sync cash to game clients
            playerCharacter.UserCash = changeCashResp.Response.Cash;

            result.InvokeSuccess(new ResponseCashPackageBuyValidationMessage()
            {
                dataId = request.dataId,
                cash = changeCashResp.Response.Cash,
            });
#endif
        }
    }
}
