using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override string GetCurrentMapId(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!IsInstanceMap())
                return base.GetCurrentMapId(playerCharacterEntity);
            return instanceMapCurrentLocations[playerCharacterEntity.ObjectId].Key;
#else
            return string.Empty;
#endif
        }

        public override Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!IsInstanceMap())
                return base.GetCurrentPosition(playerCharacterEntity);
            return instanceMapCurrentLocations[playerCharacterEntity.ObjectId].Value;
#else
            return Vector3.zero;
#endif
        }

        protected override bool IsInstanceMap()
        {
            return !string.IsNullOrEmpty(MapInstanceId);
        }

        protected override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName) || (mapName.Equals(CurrentMapInfo.Id) && !IsInstanceMap()))
            {
                if (overrideRotation)
                    playerCharacterEntity.CurrentRotation = rotation;
                playerCharacterEntity.Teleport(position);
                return;
            }

            WarpCharacterRoutine(playerCharacterEntity, mapName, position, overrideRotation, rotation).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        /// <summary>
        /// Warp to different map.
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private async UniTaskVoid WarpCharacterRoutine(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            // If warping to different map
            long connectionId = playerCharacterEntity.ConnectionId;
            CentralServerPeerInfo peerInfo;
            BaseMapInfo mapInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                ServerPlayerCharacterHandlers.TryGetPlayerCharacter(connectionId, out _) &&
                mapServerConnectionIdsBySceneName.TryGetValue(mapName, out peerInfo) &&
                GameInstance.MapInfos.TryGetValue(mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Add this character to warping list
                playerCharacterEntity.IsWarping = true;
                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                // Clone character data to save
                PlayerCharacterData savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                if (overrideRotation)
                    savingCharacterData.CurrentRotation = rotation;
                while (savingCharacters.Contains(savingCharacterData.Id))
                {
                    await UniTask.Yield();
                }
                await SaveCharacterRoutine(savingCharacterData, playerCharacterEntity.UserId);
                // Remove this character from warping list
                playerCharacterEntity.IsWarping = false;
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Send message to client to warp
                MMOWarpMessage message = new MMOWarpMessage();
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Warp, message);
            }
        }
#endif

        protected override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            // Generate instance id
            string instanceId = GenericUtils.GetUniqueId();
            // Prepare data for warp character later when instance map server registered to this map server
            HashSet<uint> instanceMapWarpingCharacters = new HashSet<uint>();
            PartyData party;
            if (ServerPartyHandlers.TryGetParty(playerCharacterEntity.PartyId, out party))
            {
                // If character is party member, will bring party member to join instance
                if (party.IsLeader(playerCharacterEntity.Id))
                {
                    List<BasePlayerCharacterEntity> aliveAllies = playerCharacterEntity.FindAliveCharacters<BasePlayerCharacterEntity>(CurrentGameInstance.joinInstanceMapDistance, true, false, false);
                    foreach (BasePlayerCharacterEntity aliveAlly in aliveAllies)
                    {
                        if (!party.IsMember(aliveAlly.Id))
                            continue;
                        instanceMapWarpingCharacters.Add(aliveAlly.ObjectId);
                        aliveAlly.IsWarping = true;
                    }
                    instanceMapWarpingCharacters.Add(playerCharacterEntity.ObjectId);
                    playerCharacterEntity.IsWarping = true;
                }
            }
            else
            {
                // If no party enter instance alone
                instanceMapWarpingCharacters.Add(playerCharacterEntity.ObjectId);
                playerCharacterEntity.IsWarping = true;
            }
            instanceMapWarpingCharactersByInstanceId.TryAdd(instanceId, instanceMapWarpingCharacters);
            instanceMapWarpingLocations.TryAdd(instanceId, new InstanceMapWarpingLocation()
            {
                mapName = mapName,
                position = position,
                overrideRotation = overrideRotation,
                rotation = rotation,
            });
            CentralAppServerRegister.SendRequest(MMORequestTypes.RequestSpawnMap, new RequestSpawnMapMessage()
            {
                mapId = mapName,
                instanceId = instanceId,
                instanceWarpPosition = position,
                instanceWarpOverrideRotation = overrideRotation,
                instanceWarpRotation = rotation,
            }, responseDelegate: (responseHandler, responseCode, response) => OnRequestSpawnMap(responseHandler, responseCode, response, instanceId), duration: mapSpawnDuration);
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid WarpCharacterToInstanceRoutine(BasePlayerCharacterEntity playerCharacterEntity, string instanceId)
        {
            // If warping to different map
            long connectionId = playerCharacterEntity.ConnectionId;
            CentralServerPeerInfo peerInfo;
            InstanceMapWarpingLocation warpingLocation;
            BaseMapInfo mapInfo;
            if (ServerPlayerCharacterHandlers.TryGetPlayerCharacter(connectionId, out _) &&
                instanceMapWarpingLocations.TryGetValue(instanceId, out warpingLocation) &&
                instanceMapServerConnectionIdsByInstanceId.TryGetValue(instanceId, out peerInfo) &&
                GameInstance.MapInfos.TryGetValue(warpingLocation.mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Add this character to warping list
                playerCharacterEntity.IsWarping = true;
                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                // Clone character data to save
                PlayerCharacterData savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                // Wait to save character before move to instance map
                while (savingCharacters.Contains(savingCharacterData.Id))
                {
                    await UniTask.Yield();
                }
                await SaveCharacterRoutine(savingCharacterData, playerCharacterEntity.UserId);
                // Remove this character from warping list
                playerCharacterEntity.IsWarping = false;
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Send message to client to warp
                MMOWarpMessage message = new MMOWarpMessage();
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MsgTypes.Warp, message);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid OnRequestSpawnMap(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response, string instanceId)
        {
            await UniTask.Yield();
            if (responseCode == AckResponseCode.Error ||
                responseCode == AckResponseCode.Timeout)
            {
                // Remove warping characters who warping to instance map
                HashSet<uint> instanceMapWarpingCharacters;
                if (instanceMapWarpingCharactersByInstanceId.TryGetValue(instanceId, out instanceMapWarpingCharacters))
                {
                    BasePlayerCharacterEntity playerCharacterEntity;
                    foreach (uint warpingCharacter in instanceMapWarpingCharacters)
                    {
                        if (Assets.TryGetSpawnedObject(warpingCharacter, out playerCharacterEntity))
                        {
                            playerCharacterEntity.IsWarping = false;
                            SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.ServiceNotAvailable);
                        }
                    }
                    instanceMapWarpingCharactersByInstanceId.TryRemove(instanceId, out _);
                }
                instanceMapWarpingLocations.TryRemove(instanceId, out _);
            }
        }
#endif

        public override void DepositGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            DepositGoldRoutine(playerCharacterEntity, amount).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid DepositGoldRoutine(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            if (playerCharacterEntity.Gold - amount >= 0)
            {
                // Update gold amount
                GoldResp changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
                {
                    UserId = playerCharacterEntity.UserId,
                    ChangeAmount = amount
                });
                playerCharacterEntity.UserGold = changeGoldResp.Gold;
                playerCharacterEntity.Gold -= amount;
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
        }
#endif

        public override void WithdrawGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            WithdrawGoldRoutine(playerCharacterEntity, amount).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid WithdrawGoldRoutine(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            // Get gold amount
            GoldResp goldResp = await DbServiceClient.GetGoldAsync(new GetGoldReq()
            {
                UserId = playerCharacterEntity.UserId
            });
            int gold = goldResp.Gold - amount;
            if (gold >= 0)
            {
                // Update gold amount
                GoldResp changeGoldResp = await DbServiceClient.ChangeGoldAsync(new ChangeGoldReq()
                {
                    UserId = playerCharacterEntity.UserId,
                    ChangeAmount = -amount
                });
                playerCharacterEntity.UserGold = changeGoldResp.Gold;
                playerCharacterEntity.Gold = playerCharacterEntity.Gold.Increase(amount);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
        }
#endif

        public override void DepositGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            DepositGuildGoldRoutine(playerCharacterEntity, amount).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid DepositGuildGoldRoutine(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            GuildData guild;
            if (ServerGuildHandlers.TryGetGuild(playerCharacterEntity.GuildId, out guild))
            {
                if (playerCharacterEntity.Gold - amount >= 0)
                {
                    // Update gold amount
                    GuildGoldResp changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
                    {
                        GuildId = playerCharacterEntity.GuildId,
                        ChangeAmount = amount
                    });
                    guild.gold = changeGoldResp.GuildGold;
                    playerCharacterEntity.Gold -= amount;
                    ServerGuildHandlers.SetGuild(playerCharacterEntity.GuildId, guild);
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToDeposit);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }
#endif

        public override void WithdrawGuildGold(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            WithdrawGuildGoldRoutine(playerCharacterEntity, amount).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid WithdrawGuildGoldRoutine(BasePlayerCharacterEntity playerCharacterEntity, int amount)
        {
            GuildData guild;
            if (ServerGuildHandlers.TryGetGuild(playerCharacterEntity.GuildId, out guild))
            {
                // Get gold amount
                GuildGoldResp goldResp = await DbServiceClient.GetGuildGoldAsync(new GetGuildGoldReq()
                {
                    GuildId = playerCharacterEntity.GuildId
                });
                int gold = goldResp.GuildGold - amount;
                if (gold >= 0)
                {
                    // Update gold amount
                    GuildGoldResp changeGoldResp = await DbServiceClient.ChangeGuildGoldAsync(new ChangeGuildGoldReq()
                    {
                        GuildId = playerCharacterEntity.GuildId,
                        ChangeAmount = -amount
                    });
                    guild.gold = changeGoldResp.GuildGold;
                    playerCharacterEntity.Gold = playerCharacterEntity.Gold.Increase(amount);
                    ServerGuildHandlers.SetGuild(playerCharacterEntity.GuildId, guild);
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }
#endif
    }
}
