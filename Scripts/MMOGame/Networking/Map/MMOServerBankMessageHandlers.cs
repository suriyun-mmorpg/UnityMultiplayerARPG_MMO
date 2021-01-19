using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
    {
        public ChatNetworkManager ChatNetworkManager { get; private set; }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        public async UniTaskVoid HandleRequestDepositGuildGold(RequestHandlerData requestHandler, RequestDepositGuildGoldMessage request, RequestProceedResultDelegate<ResponseDepositGuildGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            GuildGoldResp changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = request.gold
            });
            guild.gold = changeGoldResp.GuildGold;
            playerCharacter.Gold -= request.gold;
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendSetGuildGold(null, MMOMessageTypes.UpdateGuild, guild.id, guild.gold);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.Invoke(AckResponseCode.Success, new ResponseDepositGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDepositUserGold(RequestHandlerData requestHandler, RequestDepositUserGoldMessage request, RequestProceedResultDelegate<ResponseDepositUserGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            GoldResp changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = request.gold
            });
            playerCharacter.UserGold = changeGoldResp.Gold;
            playerCharacter.Gold -= request.gold;
            result.Invoke(AckResponseCode.Success, new ResponseDepositUserGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawGuildGold(RequestHandlerData requestHandler, RequestWithdrawGuildGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawGuildGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            // Get gold
            GuildGoldResp goldResp = await DbServiceClient.GetGuildGoldAsync(new GetGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId
            });
            int gold = goldResp.GuildGold - request.gold;
            if (gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            GuildGoldResp changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = -request.gold
            });
            guild.gold = changeGoldResp.GuildGold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendSetGuildGold(null, MMOMessageTypes.UpdateGuild, guild.id, guild.gold);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawUserGold(RequestHandlerData requestHandler, RequestWithdrawUserGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawUserGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Get gold
            GoldResp goldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
            {
                UserId = playerCharacter.UserId
            });
            int gold = goldResp.Gold - request.gold;
            if (gold < 0)
            {
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            GoldResp changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = -request.gold
            });
            playerCharacter.UserGold = changeGoldResp.Gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawUserGoldMessage());
#endif
        }
    }
}
