using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerMailMessageHandlers : MonoBehaviour, IServerMailMessageHandlers
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
            if (GameInstance.ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
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
            if (GameInstance.ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                UpdateReadMailStateResp resp = await DbServiceClient.UpdateReadMailStateAsync(new UpdateReadMailStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                ResponseReadMailMessage.Error error = (ResponseReadMailMessage.Error)resp.Error;
                result.Invoke(
                    error == ResponseReadMailMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseReadMailMessage()
                    {
                        error = error,
                        mail = resp.Mail.FromByteString<Mail>(),
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseReadMailMessage()
                {
                    error = ResponseReadMailMessage.Error.NotAvailable,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestClaimMailItems(RequestHandlerData requestHandler, RequestClaimMailItemsMessage request, RequestProceedResultDelegate<ResponseClaimMailItemsMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                ResponseClaimMailItemsMessage.Error error = ResponseClaimMailItemsMessage.Error.None;
                GetMailResp mailResp = await DbServiceClient.GetMailAsync(new GetMailReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                Mail mail = mailResp.Mail.FromByteString<Mail>();
                if (mail.IsClaim)
                {
                    error = ResponseClaimMailItemsMessage.Error.AlreadyClaimed;
                }
                else if (mail.IsDelete)
                {
                    error = ResponseClaimMailItemsMessage.Error.NotAllowed;
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
                            error = ResponseClaimMailItemsMessage.Error.CannotCarryAllItems;
                        else
                            playerCharacter.IncreaseItems(increasingItems);
                    }
                    if (error == ResponseClaimMailItemsMessage.Error.None && mail.Currencies.Count > 0)
                    {
                        List<CharacterCurrency> increasingCurrencies = new List<CharacterCurrency>();
                        foreach (KeyValuePair<int, int> mailCurrency in mail.Currencies)
                        {
                            increasingCurrencies.Add(CharacterCurrency.Create(mailCurrency.Key, amount: mailCurrency.Value));
                        }
                        playerCharacter.IncreaseCurrencies(increasingCurrencies);
                    }
                    if (error == ResponseClaimMailItemsMessage.Error.None && mail.Gold > 0)
                    {
                        playerCharacter.Gold = playerCharacter.Gold.Increase(mail.Gold);
                    }
                }
                if (error != ResponseClaimMailItemsMessage.Error.None)
                {
                    result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                    {
                        error = error,
                    });
                    return;
                }
                UpdateClaimMailItemsStateResp resp = await DbServiceClient.UpdateClaimMailItemsStateAsync(new UpdateClaimMailItemsStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                error = (ResponseClaimMailItemsMessage.Error)resp.Error;
                result.Invoke(
                    error == ResponseClaimMailItemsMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseClaimMailItemsMessage()
                    {
                        error = error,
                        mail = resp.Mail.FromByteString<Mail>(),
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseClaimMailItemsMessage()
                {
                    error = ResponseClaimMailItemsMessage.Error.NotAvailable,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestDeleteMail(RequestHandlerData requestHandler, RequestDeleteMailMessage request, RequestProceedResultDelegate<ResponseDeleteMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                UpdateDeleteMailStateResp resp = await DbServiceClient.UpdateDeleteMailStateAsync(new UpdateDeleteMailStateReq()
                {
                    MailId = request.id,
                    UserId = playerCharacter.UserId,
                });
                ResponseDeleteMailMessage.Error error = (ResponseDeleteMailMessage.Error)resp.Error;
                result.Invoke(
                    error == ResponseDeleteMailMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseDeleteMailMessage()
                    {
                        error = error,
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseDeleteMailMessage()
                {
                    error = ResponseDeleteMailMessage.Error.NotAvailable,
                });
            }
#endif
        }

        public async UniTaskVoid HandleRequestSendMail(RequestHandlerData requestHandler, RequestSendMailMessage request, RequestProceedResultDelegate<ResponseSendMailMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (GameInstance.ServerPlayerCharacterHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
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
                        error = ResponseSendMailMessage.Error.NotEnoughGold,
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
                        error = ResponseSendMailMessage.Error.NoReceiver,
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
                ResponseSendMailMessage.Error error = (ResponseSendMailMessage.Error)resp.Error;
                result.Invoke(
                    error == ResponseSendMailMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                    new ResponseSendMailMessage()
                    {
                        error = error,
                    });
            }
            else
            {
                result.Invoke(AckResponseCode.Error, new ResponseSendMailMessage()
                {
                    error = ResponseSendMailMessage.Error.NotAvailable,
                });
            }
#endif
        }
    }
}
