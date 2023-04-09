using ConcurrentCollections;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerMailMessageHandlers : MonoBehaviour, IServerMailMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }

        private ConcurrentHashSet<string> busyMailBoxes = new ConcurrentHashSet<string>();
#endif

        public async UniTaskVoid HandleRequestMailList(RequestHandlerData requestHandler, RequestMailListMessage request, RequestProceedResultDelegate<ResponseMailListMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            List<MailListEntry> mails = new List<MailListEntry>();
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                DatabaseApiResult<MailListResp> resp = await DbServiceClient.MailListAsync(new MailListReq()
                {
                    UserId = userId,
                    OnlyNewMails = request.onlyNewMails,
                });
                if (resp.IsSuccess)
                    mails.AddRange(resp.Response.List);
            }
            result.InvokeSuccess(new ResponseMailListMessage()
            {
                onlyNewMails = request.onlyNewMails,
                mails = mails,
            });
#endif
        }

        public async UniTaskVoid HandleRequestReadMail(RequestHandlerData requestHandler, RequestReadMailMessage request, RequestProceedResultDelegate<ResponseReadMailMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                DatabaseApiResult<UpdateReadMailStateResp> resp = await DbServiceClient.UpdateReadMailStateAsync(new UpdateReadMailStateReq()
                {
                    MailId = request.id,
                    UserId = userId,
                });
                if (!resp.IsSuccess)
                {
                    result.InvokeError(new ResponseReadMailMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
                UITextKeys message = resp.Response.Error;
                result.Invoke(
                    message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseReadMailMessage()
                    {
                        message = message,
                        mail = resp.Response.Mail,
                    });
            }
            else
            {
                result.InvokeError(new ResponseReadMailMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
            }
#endif
        }

        private async UniTask<UITextKeys> ClaimMailItems(string mailId, IPlayerCharacterData playerCharacter)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            DatabaseApiResult<GetMailResp> mailResp = await DbServiceClient.GetMailAsync(new GetMailReq()
            {
                MailId = mailId,
                UserId = playerCharacter.UserId,
            });
            if (!mailResp.IsSuccess)
                return UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR;
            Mail mail = mailResp.Response.Mail;
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
                    DatabaseApiResult<CashResp> changeCashResp = await DbServiceClient.ChangeCashAsync(new ChangeCashReq()
                    {
                        UserId = playerCharacter.UserId,
                        ChangeAmount = mail.Cash
                    });
                    if (!changeCashResp.IsSuccess)
                        return UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR;
                    playerCharacter.UserCash = changeCashResp.Response.Cash;
                }
            }
            DatabaseApiResult<UpdateClaimMailItemsStateResp> resp = await DbServiceClient.UpdateClaimMailItemsStateAsync(new UpdateClaimMailItemsStateReq()
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
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (busyMailBoxes.Contains(request.id))
            {
                result.InvokeError(new ResponseClaimMailItemsMessage());
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseClaimMailItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            busyMailBoxes.Add(request.id);
            UITextKeys message = await ClaimMailItems(request.id, playerCharacter);
            busyMailBoxes.TryRemove(request.id);
            if (message != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseClaimMailItemsMessage()
                {
                    message = message,
                });
                return;
            }
            result.InvokeSuccess(new ResponseClaimMailItemsMessage());
#endif
        }

        private async UniTask<UITextKeys> DeleteMail(string mailId, string userId)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            DatabaseApiResult<UpdateDeleteMailStateResp> resp = await DbServiceClient.UpdateDeleteMailStateAsync(new UpdateDeleteMailStateReq()
            {
                MailId = mailId,
                UserId = userId,
            });
            if (!resp.IsSuccess)
                return UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR;
            return resp.Response.Error;
#else
            return UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE;
#endif
        }

        public async UniTaskVoid HandleRequestDeleteMail(RequestHandlerData requestHandler, RequestDeleteMailMessage request, RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseDeleteMailMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            UITextKeys message = await DeleteMail(request.id, userId);
            if (message != UITextKeys.NONE)
            {
                result.InvokeError(new ResponseDeleteMailMessage()
                {
                    message = message,
                });
                return;
            }
            result.InvokeSuccess(new ResponseDeleteMailMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSendMail(RequestHandlerData requestHandler, RequestSendMailMessage request, RequestProceedResultDelegate<ResponseSendMailMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSendMailMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
                return;
            }
            // Validate gold
            if (request.gold < 0)
                request.gold = 0;
            if (playerCharacter.Gold >= request.gold)
            {
                playerCharacter.Gold -= request.gold;
            }
            else
            {
                result.InvokeError(new ResponseSendMailMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                });
                return;
            }
            // Find receiver
            DatabaseApiResult<GetUserIdByCharacterNameResp> userIdResp = await DbServiceClient.GetUserIdByCharacterNameAsync(new GetUserIdByCharacterNameReq()
            {
                CharacterName = request.receiverName,
            });
            if (!userIdResp.IsSuccess)
            {
                result.InvokeError(new ResponseSendMailMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            string receiverId = userIdResp.Response.UserId;
            if (string.IsNullOrEmpty(receiverId))
            {
                result.InvokeError(new ResponseSendMailMessage()
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
            DatabaseApiResult<SendMailResp> resp = await DbServiceClient.SendMailAsync(new SendMailReq()
            {
                Mail = mail,
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseSendMailMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            UITextKeys message = resp.Response.Error;
            result.Invoke(
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseSendMailMessage()
                {
                    message = message,
                });
#endif
        }

        public async UniTaskVoid HandleRequestMailNotification(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseMailNotificationMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            int notificationCount = 0;
            if (GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                DatabaseApiResult<GetMailNotificationResp> resp = await DbServiceClient.GetMailNotificationAsync(new GetMailNotificationReq()
                {
                    UserId = userId,
                });
                if (resp.IsSuccess)
                    notificationCount = resp.Response.NotificationCount;
            }
            result.InvokeSuccess(new ResponseMailNotificationMessage()
            {
                notificationCount = notificationCount,
            });
#endif
        }

        public async UniTaskVoid HandleRequestClaimAllMailsItems(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseClaimAllMailsItemsMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseClaimAllMailsItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            DatabaseApiResult<MailListResp> resp = await DbServiceClient.MailListAsync(new MailListReq()
            {
                UserId = playerCharacter.UserId,
                OnlyNewMails = true,
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseClaimAllMailsItemsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            foreach (MailListEntry entry in resp.Response.List)
            {
                if (busyMailBoxes.Contains(entry.Id))
                    continue;
                busyMailBoxes.Add(entry.Id);
                await ClaimMailItems(entry.Id, playerCharacter);
                busyMailBoxes.TryRemove(entry.Id);
            }
            result.InvokeSuccess(new ResponseClaimAllMailsItemsMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDeleteAllMails(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseDeleteAllMailsMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetUserId(requestHandler.ConnectionId, out string userId))
            {
                result.InvokeError(new ResponseDeleteAllMailsMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
            }
            DatabaseApiResult<MailListResp> resp = await DbServiceClient.MailListAsync(new MailListReq()
            {
                UserId = userId,
                OnlyNewMails = false,
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseDeleteAllMailsMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            foreach (MailListEntry entry in resp.Response.List)
            {
                await DeleteMail(entry.Id, userId);
            }
            result.InvokeSuccess(new ResponseDeleteAllMailsMessage());
#endif
        }
    }
}
