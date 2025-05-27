using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
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
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (request.gold <= 0)
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                });
                return;
            }
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
            int requiredGold = GameInstance.Singleton.GameplayRule.GetGuildBankDepositFee(request.gold) + request.gold;
            if (playerCharacter.Gold < requiredGold)
            {
                result.InvokeError(new ResponseDepositGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GuildGoldResp> changeGoldResp = await DatabaseClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = request.gold,
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
            playerCharacter.Gold -= requiredGold;
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
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (request.gold <= 0)
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                });
                return;
            }
            if (GameInstance.Singleton.goldStoreMode == GoldStoreMode.UserGoldOnly)
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            int requiredGold = GameInstance.Singleton.GameplayRule.GetUserBankDepositFee(request.gold) + request.gold;
            if (playerCharacter.Gold < requiredGold)
            {
                result.InvokeError(new ResponseDepositUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_DEPOSIT,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GoldResp> changeGoldResp = await DatabaseClient.ChangeGoldAsync(new ChangeGoldReq()
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
            playerCharacter.Gold -= requiredGold;
            result.InvokeSuccess(new ResponseDepositUserGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawGuildGold(RequestHandlerData requestHandler, RequestWithdrawGuildGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawGuildGoldMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (request.gold <= 0)
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                });
                return;
            }
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
            DatabaseApiResult<GuildGoldResp> goldResp = await DatabaseClient.GetGuildGoldAsync(new GetGuildGoldReq()
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
            int requiredGold = GameInstance.Singleton.GameplayRule.GetGuildBankWithdrawFee(request.gold) + request.gold;
            if (goldResp.Response.GuildGold < requiredGold)
            {
                result.InvokeError(new ResponseWithdrawGuildGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GuildGoldResp> changeGoldResp = await DatabaseClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
            {
                GuildId = playerCharacter.GuildId,
                ChangeAmount = -requiredGold,
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
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (request.gold <= 0)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD,
                });
                return;
            }
            if (GameInstance.Singleton.goldStoreMode == GoldStoreMode.UserGoldOnly)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            // Get gold
            DatabaseApiResult<GoldResp> goldResp = await DatabaseClient.GetGoldAsync(new GetGoldReq()
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
            int requiredGold = GameInstance.Singleton.GameplayRule.GetUserBankWithdrawFee(request.gold) + request.gold;
            if (goldResp.Response.Gold < request.gold)
            {
                result.InvokeError(new ResponseWithdrawUserGoldMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_ENOUGH_GOLD_TO_WITHDRAW,
                });
                return;
            }
            // Update gold
            DatabaseApiResult<GoldResp> changeGoldResp = await DatabaseClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = playerCharacter.UserId,
                ChangeAmount = -requiredGold,
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
