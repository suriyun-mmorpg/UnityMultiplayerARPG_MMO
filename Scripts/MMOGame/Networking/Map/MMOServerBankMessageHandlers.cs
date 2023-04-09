using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
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

        public async UniTaskVoid HandleRequestDepositGuildGold(RequestHandlerData requestHandler, RequestDepositGuildGoldMessage request, RequestProceedResultDelegate<ResponseDepositGuildGoldMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guild))
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GuildGoldResp> changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = request.gold
            });
            if (!changeGoldResp.IsSuccess)
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            guild.gold = changeGoldResp.Response.GuildGold;
            playerCharacter.Gold -= request.gold;
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildGold(MMOMessageTypes.UpdateGuild, guild.id, guild.gold);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.InvokeSuccess(new ResponseDepositGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDepositUserGold(RequestHandlerData requestHandler, RequestDepositUserGoldMessage request, RequestProceedResultDelegate<ResponseDepositUserGoldMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GoldResp> changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = request.gold
            });
            if (!changeGoldResp.IsSuccess)
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            playerCharacter.UserGold = changeGoldResp.Response.Gold;
            playerCharacter.Gold -= request.gold;
            result.InvokeSuccess(new ResponseDepositUserGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawGuildGold(RequestHandlerData requestHandler, RequestWithdrawGuildGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawGuildGoldMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guild))
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_JOINED_GUILD,
                });
                return;
            }
            // Get gold
            DatabaseApiResult<GuildGoldResp> goldResp = await DbServiceClient.GetGuildGoldAsync(new GetGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId
            });
            if (!goldResp.IsSuccess)
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (goldResp.Response.GuildGold - request.gold < 0)
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GuildGoldResp> changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = -request.gold
            });
            if (!changeGoldResp.IsSuccess)
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            guild.gold = changeGoldResp.Response.GuildGold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            GameInstance.ServerGuildHandlers.SetGuild(playerCharacter.GuildId, guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildGold(MMOMessageTypes.UpdateGuild, guild.id, guild.gold);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildGoldToMembers(guild);
            result.InvokeSuccess(new ResponseWithdrawGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawUserGold(RequestHandlerData requestHandler, RequestWithdrawUserGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawUserGoldMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Get gold
            DatabaseApiResult<GoldResp> goldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
            {
                UserId = playerCharacter.UserId
            });
            if (!goldResp.IsSuccess)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (goldResp.Response.Gold - request.gold < 0)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GoldResp> changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = -request.gold
            });
            if (!changeGoldResp.IsSuccess)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            playerCharacter.UserGold = changeGoldResp.Response.Gold;
            playerCharacter.Gold = playerCharacter.Gold.Increase(request.gold);
            result.InvokeSuccess(new ResponseWithdrawUserGoldMessage());
#endif
        }
    }
}
