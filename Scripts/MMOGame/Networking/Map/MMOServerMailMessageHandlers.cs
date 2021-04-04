using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerMailMessageHandlers : MonoBehaviour, IServerMailMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        public async UniTaskVoid HandleRequestMailList(RequestHandlerData requestHandler, RequestMailListMessage request, RequestProceedResultDelegate<ResponseMailListMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            List<MailListEntry> mails = new List<MailListEntry>();
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                MailListResp resp = await DbServiceClient.MailListAsync(new MailListReq()
                {
                    UserId = playerCharacter.UserId,
                    OnlyNewMails = request.onlyNewMails,
                });
                mails.AddRange(resp.List.MakeListFromRepeatedByteString<MailListEntry>());
            }
            result.Invoke(AckResponseCode.Success, new ResponseMailListMessage()
            {
                onlyNewMails = request.onlyNewMails,
                mails = mails.ToArray(),
            });
#endif
        }

        public async UniTaskVoid HandleRequestReadMail(RequestHandlerData requestHandler, RequestReadMailMessage request, RequestProceedResultDelegate<ResponseReadMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                UpdateReadMailStateResp resp = await DbServiceClient.UpdateReadMailStateAsync(new UpdateReadMailStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                UITextKeys message = (UITextKeys)resp.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseReadMailMessage()
                    {
                        message = message,
                        mail = resp.Mail.FromByteString<Mail>(),
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseReadMailMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestClaimMailItems(RequestHandlerData requestHandler, RequestClaimMailItemsMessage request, RequestProceedResultDelegate<ResponseClaimMailItemsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                UITextKeys message = UITextKeys.NONE;
                GetMailResp mailResp = await DbServiceClient.GetMailAsync(new GetMailReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                Mail mail = mailResp.Mail.FromByteString<Mail>();
                if (mail.IsClaim)
                {
                    message = UITextKeys.UI_ERROR_MAIL_CLAIM_ALREADY_CLAIMED;
                }
                else if (mail.IsDelete)
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE;
                }
                else
                {
                    if (mail.Items.Count > 0)
                    {
                        List<CharacterItem> increasingItems = new List<CharacterItem>();
                        foreach (KeyValuePair<int, short> mailItem in mail.Items)
                        {
                            increasingItems.Add(CharacterItem.Create(mailItem.Key, amount: mailItem.Value));
                        }
                        if (playerCharacter.IncreasingItemsWillOverwhelming(increasingItems))
                            message = UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                        else
                            playerCharacter.IncreaseItems(increasingItems);
                    }
                    if (message == UITextKeys.NONE && mail.Currencies.Count > 0)
                    {
                        List<CurrencyAmount> increasingCurrencies = new List<CurrencyAmount>();
                        Currency tempCurrency;
                        foreach (KeyValuePair<int, int> mailCurrency in mail.Currencies)
                        {
                            if (!GameInstance.Currencies.TryGetValue(mailCurrency.Key, out tempCurrency))
                                continue;
                            increasingCurrencies.Add(new CurrencyAmount()
                            {
                                currency = tempCurrency,
                                amount = mailCurrency.Value
                            });
                        }
                        playerCharacter.IncreaseCurrencies(increasingCurrencies);
                    }
                    if (message == UITextKeys.NONE && mail.Gold > 0)
                    {
                        playerCharacter.Gold = playerCharacter.Gold.Increase(mail.Gold);
                    }
                }
                if (message != UITextKeys.NONE)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                    {
                        message = message,
                    });
                    return;
                }
                UpdateClaimMailItemsStateResp resp = await DbServiceClient.UpdateClaimMailItemsStateAsync(new UpdateClaimMailItemsStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                message = (UITextKeys)resp.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseClaimMailItemsMessage()
                    {
                        message = message,
                        mail = resp.Mail.FromByteString<Mail>(),
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestDeleteMail(RequestHandlerData requestHandler, RequestDeleteMailMessage request, RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                UpdateDeleteMailStateResp resp = await DbServiceClient.UpdateDeleteMailStateAsync(new UpdateDeleteMailStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                UITextKeys message = (UITextKeys)resp.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseDeleteMailMessage()
                    {
                        message = message,
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestSendMail(RequestHandlerData requestHandler, RequestSendMailMessage request, RequestProceedResultDelegate<ResponseSendMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                // Validate gold
                if (request.gold < 0)
                    request.gold = 0;
                if (playerCharacter.Gold >= request.gold)
                {
                    playerCharacter.Gold -= request.gold;
                }
                else
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
                    {
                        message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                    });
                    return;
                }
                // Find receiver
                GetUserIdByCharacterNameResp userIdResp = await DbServiceClient.GetUserIdByCharacterNameAsync(new GetUserIdByCharacterNameReq()
                {
                    CharacterName = request.receiverName,
                });
                string receiverId = userIdResp.UserId;
                if (string.IsNullOrEmpty(receiverId))
                {
                    result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
                    {
                        message = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER,
                    });
                    return;
                }
                Mail mail = new Mail()
                {
                    SenderId = playerCharacter.UserId,
                    SenderName = playerCharacter.CharacterName,
                    ReceiverId = receiverId,
                    Title = request.title,
                    Content = request.content,
                    Gold = request.gold,
                };
                SendMailResp resp = await DbServiceClient.SendMailAsync(new SendMailReq()
                {
                    Mail = DatabaseServiceUtils.ToByteString(mail),
                });
                UITextKeys message = (UITextKeys)resp.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseSendMailMessage()
                    {
                        message = message,
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestMailNotification(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseMailNotificationMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Unimplemented, new ResponseMailNotificationMessage());
            await UniTask.Yield();
#endif
        }
    }
}
