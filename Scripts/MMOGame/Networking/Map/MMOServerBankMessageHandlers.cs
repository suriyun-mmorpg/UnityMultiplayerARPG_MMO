using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerBankMessageHandlers : MonoBehaviour, IServerBankMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        public async UniTaskVoid HandleRequestDepositGuildGold(RequestHandlerData requestHandler, RequestDepositGuildGoldMessage request, RequestProceedResultDelegate<ResponseDepositGuildGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.GuildNotFound,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
                result.Invoke(AckResponseCode.Error, new ResponseDepositGuildGoldMessage()
                {
                    error = ResponseDepositGuildGoldMessage.Error.GoldNotEnough,
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
            BaseGameNetworkManager.Singleton.SendSetGuildGoldToClients(guild);
            result.Invoke(AckResponseCode.Success, new ResponseDepositGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestDepositUserGold(RequestHandlerData requestHandler, RequestDepositUserGoldMessage request, RequestProceedResultDelegate<ResponseDepositUserGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    error = ResponseDepositUserGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (playerCharacter.Gold - request.gold < 0)
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
                result.Invoke(AckResponseCode.Error, new ResponseDepositUserGoldMessage()
                {
                    error = ResponseDepositUserGoldMessage.Error.GoldNotEnough,
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
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.NotLoggedIn,
                });
                return;
            }
            GuildData guild;
            if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out guild))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.GuildNotFound,
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
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawGuildGoldMessage()
                {
                    error = ResponseWithdrawGuildGoldMessage.Error.GoldNotEnough,
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
            BaseGameNetworkManager.Singleton.SendSetGuildGoldToClients(guild);
            result.Invoke(AckResponseCode.Success, new ResponseWithdrawGuildGoldMessage());
#endif
        }

        public async UniTaskVoid HandleRequestWithdrawUserGold(RequestHandlerData requestHandler, RequestWithdrawUserGoldMessage request, RequestProceedResultDelegate<ResponseWithdrawUserGoldMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            BasePlayerCharacterEntity playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.NotFoundCharacter);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    error = ResponseWithdrawUserGoldMessage.Error.NotLoggedIn,
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
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
                result.Invoke(AckResponseCode.Error, new ResponseWithdrawUserGoldMessage()
                {
                    error = ResponseWithdrawUserGoldMessage.Error.GoldNotEnough,
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
