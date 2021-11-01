using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerMailMessageHandlers : MonoBehaviour, IServerMailMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTaskVoid HandleRequestMailList(RequestHandlerData requestHandler, RequestMailListMessage request, RequestProceedResultDelegate<ResponseMailListMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            List<MailListEntry> mails = new List<MailListEntry>();
            string userId;
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                MailListResp resp = await DbServiceClient.MailListAsync(new MailListReq()
                {
                    UserId = userId,
                    OnlyNewMails = request.onlyNewMails,
                });
                mails.AddRange(resp.List);
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
            string userId;
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                UpdateReadMailStateResp resp = await DbServiceClient.UpdateReadMailStateAsync(new UpdateReadMailStateReq()
                {
                    MailId = request.id,
                    UserId = userId,
                });
                UITextKeys message = resp.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseReadMailMessage()
                    {
                        message = message,
                        mail = resp.Mail,
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

        private async UniTask<UITextKeys> ClaimMailItems(string mailId, IPlayerCharacterData playerCharacter)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GetMailResp mailResp = await DbServiceClient.GetMailAsync(new GetMailReq()
            {
                MailId = mailId,
                UserId = playerCharacter.UserId,
            });
            Mail mail = mailResp.Mail;
            if (mail.IsClaim)
            {
                return UITextKeys.UI_ERROR_MAIL_CLAIM_ALREADY_CLAIMED;
            }
            else if (mail.IsDelete)
            {
                return UITextKeys.UI_ERROR_MAIL_CLAIM_NOT_ALLOWED;
            }
            else
            {
                if (mail.Items.Count > 0)
                {
                    if (playerCharacter.IncreasingItemsWillOverwhelming(mail.Items))
                        return UITextKeys.UI_ERROR_WILL_OVERWHELMING;
                    else
                        playerCharacter.IncreaseItems(mail.Items);
                }
                if (mail.Currencies.Count > 0)
                {
                    playerCharacter.IncreaseCurrencies(mail.Currencies);
                }
                if (mail.Gold > 0)
                {
                    playerCharacter.Gold = playerCharacter.Gold.Increase(mail.Gold);
                }
                if (mail.Cash > 0)
                {
                    CashResp changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
                    {
                        UserId = playerCharacter.UserId,
                        ChangeAmount = -mail.Cash
                    });
                    playerCharacter.UserCash = changeCashResp.Cash;
                }
            }
            UpdateClaimMailItemsStateResp resp = await DbServiceClient.UpdateClaimMailItemsStateAsync(new UpdateClaimMailItemsStateReq()
            {
                MailId = mailId,
                UserId = playerCharacter.UserId,
            });
            return UITextKeys.NONE;
#else
            return UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
#endif
        }

        public async UniTaskVoid HandleRequestClaimMailItems(RequestHandlerData requestHandler, RequestClaimMailItemsMessage request, RequestProceedResultDelegate<ResponseClaimMailItemsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys message = await ClaimMailItems(request.id, playerCharacter);
            if (message != UITextKeys.NONE)
            {
                result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                {
                    message = message,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseClaimMailItemsMessage());
#endif
        }

        private async UniTask<UITextKeys> DeleteMail(string mailId, string userId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            UpdateDeleteMailStateResp resp = await DbServiceClient.UpdateDeleteMailStateAsync(new UpdateDeleteMailStateReq()
            {
                MailId = mailId,
                UserId = userId,
            });
            return resp.Error;
#else
            return UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
#endif
        }

        public async UniTaskVoid HandleRequestDeleteMail(RequestHandlerData requestHandler, RequestDeleteMailMessage request, RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string userId;
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys message = await DeleteMail(request.id, userId);
            if (message != UITextKeys.NONE)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
                {
                    message = message,
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new ResponseDeleteMailMessage());
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
                    Mail = mail,
                });
                UITextKeys message = resp.Error;
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
            int notificationCount = 0;
            string userId;
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                GetMailNotificationCountResp resp = await DbServiceClient.GetMailsCountAsync(new GetMailNotificationCountReq()
                {
                    UserId = userId,
                });
                notificationCount = resp.NotificationCount;
            }
            result.Invoke(AckResponseCode.Success, new ResponseMailNotificationMessage()
            {
                notificationCount = notificationCount,
            });
#endif
        }

        public async UniTaskVoid HandleRequestClaimAllMailsItems(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseClaimAllMailsItemsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseClaimAllMailsItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            MailListResp resp = await DbServiceClient.MailListAsync(new MailListReq()
            {
                UserId = playerCharacter.UserId,
                OnlyNewMails = true,
            });
            foreach (MailListEntry entry in resp.List)
            {
                await ClaimMailItems(entry.Id, playerCharacter);
            }
            result.Invoke(AckResponseCode.Success, new ResponseClaimAllMailsItemsMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDeleteAllMails(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseDeleteAllMailsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string userId;
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out userId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeleteAllMailsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
            }
            MailListResp resp = await DbServiceClient.MailListAsync(new MailListReq()
            {
                UserId = userId,
                OnlyNewMails = false,
            });
            foreach (MailListEntry entry in resp.List)
            {
                await DeleteMail(entry.Id, userId);
            }
            result.Invoke(AckResponseCode.Success, new ResponseDeleteAllMailsMessage());
#endif
        }
    }
}
