using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
#if ENABLE_PURCHASING && UNITY_PURCHASING
using UnityEngine.Purchasing.Security;
#endif

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerCashShopMessageHandlers : MonoBehaviour, IServerCashShopMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        public async UniTaskVoid HandleRequestCashShopInfo(
            RequestHandlerData requestHandler, EmptyMessage request,
            RequestProceedResultDelegate<ResponseCashShopInfoMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseCashShopInfoMessage()
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
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
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

            if (!GameInstance.CashShopItems.TryGetValue(request.dataId, out CashShopItem cashShopItem))
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
                            randomSeed = Random.Range(int.MinValue, int.MaxValue),
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

            // Increase custom currencies
            List<CharacterCurrency> customCurrencies = new List<CharacterCurrency>();
            if (cashShopItem.ReceiveCurrencies != null &&
                cashShopItem.ReceiveCurrencies.Length > 0)
            {
                foreach (CurrencyAmount currencyAmount in cashShopItem.ReceiveCurrencies)
                {
                    for (int i = 0; i < request.amount; ++i)
                    {
                        customCurrencies.Add(CharacterCurrency.Create(currencyAmount.currency, currencyAmount.amount));
                    }
                }
            }

            // Update currency
            characterGold += changeCharacterGold;
            if (request.currencyType == CashShopItemCurrencyType.CASH)
            {
                DatabaseApiResult<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
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
            playerCharacter.IncreaseCurrencies(customCurrencies);
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
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseCashPackageInfoMessage()
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
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            if (!GameInstance.CashPackages.TryGetValue(request.dataId, out CashPackage cashPackage))
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_CASH_PACKAGE_NOT_FOUND,
                });
                return;
            }

            IIAPReceiptValidator receiptValidator = GetComponentInChildren<IIAPReceiptValidator>();
            if (receiptValidator == null)
                receiptValidator = gameObject.AddComponent<DefaultIAPReceiptValidator>();
            IAPReceiptValidateResult validateResult = await receiptValidator.ValidateIAPReceipt(request.receipt);
            if (!validateResult.IsSuccess)
            {
                result.InvokeError(new ResponseCashPackageBuyValidationMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_GET_CASH_PACKAGE_INFO,
                });
                return;
            }

            int resultUserCash = 0;
            for (int i = 0; i < validateResult.CashPackages.Count; ++i)
            {
                // TODO: Money thing is very important, it should have better data handling
                DatabaseApiResult<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
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
                resultUserCash = changeCashResp.Response.Cash;
            }

            // Sync cash to game clients
            playerCharacter.UserCash = resultUserCash;

            result.InvokeSuccess(new ResponseCashPackageBuyValidationMessage()
            {
                dataId = request.dataId,
                cash = resultUserCash,
            });
#endif
        }
    }
}
