using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System;
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
                playerCharacters.ContainsKey(connectionId) &&
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
            if (parties.TryGetValue(playerCharacterEntity.PartyId, out party))
            {
                // If character is party member, will bring party member to join instance
                if (party.IsLeader(playerCharacterEntity))
                {
                    List<BasePlayerCharacterEntity> aliveAllies = playerCharacterEntity.FindAliveCharacters<BasePlayerCharacterEntity>(CurrentGameInstance.joinInstanceMapDistance, true, false, false);
                    foreach (BasePlayerCharacterEntity aliveAlly in aliveAllies)
                    {
                        if (!party.IsMember(aliveAlly))
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
            instanceMapWarpingCharactersByInstanceId.Add(instanceId, instanceMapWarpingCharacters);
            instanceMapWarpingLocations.Add(instanceId, new InstanceMapWarpingLocation()
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
            if (playerCharacters.ContainsKey(connectionId) &&
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
        private void OnRequestSpawnMap(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response, string instanceId)
        {
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
                    instanceMapWarpingCharactersByInstanceId.Remove(instanceId);
                }
                instanceMapWarpingLocations.Remove(instanceId);
            }
        }
#endif

        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanCreateParty(playerCharacterEntity))
                return;
            CreatePartyRoutine(playerCharacterEntity, shareExp, shareItem).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid CreatePartyRoutine(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            PartyResp createPartyResp = await DbServiceClient.CreatePartyAsync(new CreatePartyReq()
            {
                LeaderCharacterId = playerCharacterEntity.Id,
                ShareExp = shareExp,
                ShareItem = shareItem
            });
            PartyData party = DatabaseServiceUtils.FromByteString<PartyData>(createPartyResp.PartyData);
            // Created party, notify to players
            parties[party.id] = party;
            playerCharacterEntity.PartyId = party.id;
            SendCreatePartyToClient(playerCharacterEntity.ConnectionId, party);
            SendAddPartyMembersToClient(playerCharacterEntity.ConnectionId, party);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendCreateParty(null, MMOMessageTypes.UpdateParty, party.id, shareExp, shareItem, playerCharacterEntity.Id);
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, party.id, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
            }
        }
#endif

        public override void ChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int partyId;
            PartyData party;
            if (!CanChangePartyLeader(playerCharacterEntity, characterId, out partyId, out party))
                return;

            base.ChangePartyLeader(playerCharacterEntity, characterId);
            // Save to database
            DbServiceClient.UpdatePartyLeaderAsync(new UpdatePartyLeaderReq()
            {
                PartyId = partyId,
                LeaderCharacterId = characterId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendChangePartyLeader(null, MMOMessageTypes.UpdateParty, partyId, characterId);
#endif
        }

        public override void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int partyId;
            PartyData party;
            if (!CanPartySetting(playerCharacterEntity, out partyId, out party))
                return;

            base.PartySetting(playerCharacterEntity, shareExp, shareItem);
            // Save to database
            DbServiceClient.UpdatePartyAsync(new UpdatePartyReq()
            {
                PartyId = partyId,
                ShareExp = shareExp,
                ShareItem = shareItem
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendPartySetting(null, MMOMessageTypes.UpdateParty, partyId, shareExp, shareItem);
#endif
        }

        public override void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int partyId;
            PartyData party;
            if (!CanAddPartyMember(inviteCharacterEntity, acceptCharacterEntity, out partyId, out party))
                return;

            base.AddPartyMember(inviteCharacterEntity, acceptCharacterEntity);
            // Save to database
            DbServiceClient.UpdateCharacterPartyAsync(new UpdateCharacterPartyReq()
            {
                SocialCharacterData = DatabaseServiceUtils.ToByteString(SocialCharacterData.Create(acceptCharacterEntity)),
                PartyId = partyId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
#endif
        }

        public override void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int partyId;
            PartyData party;
            if (!CanKickFromParty(playerCharacterEntity, characterId, out partyId, out party))
                return;

            base.KickFromParty(playerCharacterEntity, characterId);
            // Save to database
            DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
            {
                CharacterId = characterId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, characterId);
#endif
        }

        public override void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int partyId;
            PartyData party;
            if (!CanLeaveParty(playerCharacterEntity, out partyId, out party))
                return;

            // If it is leader kick all members and terminate party
            if (party.IsLeader(playerCharacterEntity))
            {
                foreach (string memberId in party.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearParty();
                        SendPartyTerminateToClient(memberCharacterEntity.ConnectionId, partyId);
                    }
                    // Save to database
                    DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                    {
                        CharacterId = memberId
                    });
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, memberId);
                }
                parties.Remove(partyId);
                // Save to database
                DbServiceClient.DeletePartyAsync(new DeletePartyReq()
                {
                    PartyId = partyId
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendPartyTerminate(null, MMOMessageTypes.UpdateParty, partyId);
            }
            else
            {
                playerCharacterEntity.ClearParty();
                SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, partyId);
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
                SendRemovePartyMemberToClients(party, playerCharacterEntity.Id);
                // Save to database
                DbServiceClient.ClearCharacterPartyAsync(new ClearCharacterPartyReq()
                {
                    CharacterId = playerCharacterEntity.Id
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, playerCharacterEntity.Id);
            }
#endif
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanCreateGuild(playerCharacterEntity, guildName))
                return;
            CreateGuildRoutine(playerCharacterEntity, guildName).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid CreateGuildRoutine(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            FindGuildNameResp findGuildNameResp = await DbServiceClient.FindGuildNameAsync(new FindGuildNameReq()
            {
                GuildName = guildName
            });
            if (findGuildNameResp.FoundAmount > 0)
            {
                // Cannot create guild because guild name is already existed
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.ExistedGuildName);
            }
            else
            {
                GuildResp createGuildResp = await DbServiceClient.CreateGuildAsync(new CreateGuildReq()
                {
                    LeaderCharacterId = playerCharacterEntity.Id,
                    GuildName = guildName
                });
                GuildData guild = DatabaseServiceUtils.FromByteString<GuildData>(createGuildResp.GuildData);
                // Created party, notify to players
                CurrentGameInstance.SocialSystemSetting.DecreaseCreateGuildResource(playerCharacterEntity);
                guilds[guild.id] = guild;
                playerCharacterEntity.GuildId = guild.id;
                playerCharacterEntity.GuildName = guildName;
                playerCharacterEntity.GuildRole = guild.GetMemberRole(playerCharacterEntity.Id);
                playerCharacterEntity.SharedGuildExp = 0;
                SendCreateGuildToClient(playerCharacterEntity.ConnectionId, guild);
                SendAddGuildMembersToClient(playerCharacterEntity.ConnectionId, guild);
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                {
                    ChatNetworkManager.SendCreateGuild(null, MMOMessageTypes.UpdateGuild, guild.id, guildName, playerCharacterEntity.Id);
                    ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, guild.id, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
                }
            }
        }
#endif

        public override void ChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanChangeGuildLeader(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            base.ChangeGuildLeader(playerCharacterEntity, characterId);
            // Save to database
            DbServiceClient.UpdateGuildLeaderAsync(new UpdateGuildLeaderReq()
            {
                GuildId = guildId,
                LeaderCharacterId = characterId
            });
            DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                MemberCharacterId = characterId,
                GuildRole = guild.GetMemberRole(characterId)
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendChangeGuildLeader(null, MMOMessageTypes.UpdateGuild, guildId, characterId);
#endif
        }

        public override void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanSetGuildMessage(playerCharacterEntity, guildMessage, out guildId, out guild))
                return;

            base.SetGuildMessage(playerCharacterEntity, guildMessage);
            // Save to database
            DbServiceClient.UpdateGuildMessageAsync(new UpdateGuildMessageReq()
            {
                GuildId = guildId,
                GuildMessage = guildMessage
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildMessage(null, MMOMessageTypes.UpdateGuild, guildId, guildMessage);
#endif
        }

        public override void SetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanSetGuildRole(playerCharacterEntity, guildRole, roleName, out guildId, out guild))
                return;

            guild.SetRole(guildRole, roleName, canInvite, canKick, shareExpPercentage);
            guilds[guildId] = guild;
            // Change characters guild role
            foreach (string memberId in guild.GetMemberIds())
            {
                BasePlayerCharacterEntity memberCharacterEntity;
                if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                {
                    memberCharacterEntity.GuildRole = guildRole;
                    // Save to database
                    DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
                    {
                        MemberCharacterId = memberId,
                        GuildRole = guildRole
                    });
                }
            }
            SendSetGuildRoleToClients(guild, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            // Save to database
            DbServiceClient.UpdateGuildRoleAsync(new UpdateGuildRoleReq()
            {
                GuildId = guildId,
                GuildRole = guildRole,
                RoleName = roleName,
                CanInvite = canInvite,
                CanKick = canKick,
                ShareExpPercentage = shareExpPercentage
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildRole(null, MMOMessageTypes.UpdateGuild, guildId, guildRole, roleName, canInvite, canKick, shareExpPercentage);
#endif
        }

        public override void SetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, string characterId, byte guildRole)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanSetGuildMemberRole(playerCharacterEntity, out guildId, out guild))
                return;

            base.SetGuildMemberRole(playerCharacterEntity, characterId, guildRole);
            // Save to database
            DbServiceClient.UpdateGuildMemberRoleAsync(new UpdateGuildMemberRoleReq()
            {
                MemberCharacterId = characterId,
                GuildRole = guildRole
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendSetGuildMemberRole(null, MMOMessageTypes.UpdateGuild, guildId, characterId, guildRole);
#endif
        }

        public override void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanAddGuildMember(inviteCharacterEntity, acceptCharacterEntity, out guildId, out guild))
                return;

            base.AddGuildMember(inviteCharacterEntity, acceptCharacterEntity);
            // Save to database
            DbServiceClient.UpdateCharacterGuildAsync(new UpdateCharacterGuildReq()
            {
                SocialCharacterData = DatabaseServiceUtils.ToByteString(SocialCharacterData.Create(acceptCharacterEntity)),
                GuildId = guildId,
                GuildRole = guild.GetMemberRole(acceptCharacterEntity.Id)
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
#endif
        }

        public override void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanKickFromGuild(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            base.KickFromGuild(playerCharacterEntity, characterId);
            // Save to database
            DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
            {
                CharacterId = characterId
            });
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, characterId);
#endif
        }

        public override void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int guildId;
            GuildData guild;
            if (!CanLeaveGuild(playerCharacterEntity, out guildId, out guild))
                return;

            // If it is leader kick all members and terminate guild
            if (guild.IsLeader(playerCharacterEntity))
            {
                foreach (string memberId in guild.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                    {
                        memberCharacterEntity.ClearGuild();
                        SendGuildTerminateToClient(memberCharacterEntity.ConnectionId, guildId);
                    }
                    // Save to database
                    DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                    {
                        CharacterId = memberId
                    });
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, memberId);
                }
                guilds.Remove(guildId);
                // Save to database
                DbServiceClient.DeleteGuildAsync(new DeleteGuildReq()
                {
                    GuildId = guildId
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendGuildTerminate(null, MMOMessageTypes.UpdateGuild, guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, guildId);
                guild.RemoveMember(playerCharacterEntity.Id);
                guilds[guildId] = guild;
                SendRemoveGuildMemberToClients(guild, playerCharacterEntity.Id);
                // Save to database
                DbServiceClient.ClearCharacterGuildAsync(new ClearCharacterGuildReq()
                {
                    CharacterId = playerCharacterEntity.Id
                });
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, playerCharacterEntity.Id);
            }
#endif
        }

        public override void IncreaseGuildExp(BasePlayerCharacterEntity playerCharacterEntity, int exp)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IncreaseGuildExpRoutine(playerCharacterEntity, exp).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid IncreaseGuildExpRoutine(BasePlayerCharacterEntity playerCharacterEntity, int exp)
        {
            int guildId;
            GuildData guild;
            if (!CanIncreaseGuildExp(playerCharacterEntity, exp, out guildId, out guild))
                return;
            // Save to database
            GuildResp resp = await DbServiceClient.IncreaseGuildExpAsync(new IncreaseGuildExpReq()
            {
                GuildId = guildId,
                Exp = exp
            });
            guild = DatabaseServiceUtils.FromByteString<GuildData>(resp.GuildData);
            guilds[guildId] = guild;
            SendGuildLevelExpSkillPointToClients(guild);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.SendGuildLevelExpSkillPoint(null, MMOMessageTypes.UpdateGuild, guildId, guild.level, guild.exp, guild.skillPoint);
        }
#endif

        public override void AddGuildSkill(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            AddGuildSkillRoutine(playerCharacterEntity, dataId).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid AddGuildSkillRoutine(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildSkill(playerCharacterEntity, dataId, out guildId, out guild))
                return;
            // Save to database
            GuildResp resp = await DbServiceClient.AddGuildSkillAsync(new AddGuildSkillReq()
            {
                GuildId = guildId,
                SkillId = dataId
            });
            guilds[guildId] = guild;
            SendSetGuildSkillLevelToClients(guild, dataId);
            SendGuildLevelExpSkillPointToClients(guild);
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.SendSetGuildSkillLevel(null, MMOMessageTypes.UpdateGuild, guildId, dataId, guild.GetSkillLevel(dataId));
                ChatNetworkManager.SendSetGuildGold(null, MMOMessageTypes.UpdateGuild, guildId, guild.gold);
                ChatNetworkManager.SendGuildLevelExpSkillPoint(null, MMOMessageTypes.UpdateGuild, guildId, guild.level, guild.exp, guild.skillPoint);
            }
        }
#endif

        public override List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (storageEntity == null)
                return new List<CharacterItem>();
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            if (!allStorageItems.ContainsKey(id))
                allStorageItems[id] = new List<CharacterItem>();
            return allStorageItems[id];
#else
            return new List<CharacterItem>();
#endif
        }

        public override void OpenStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacterEntity, playerCharacterEntity.CurrentStorageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId] = new HashSet<uint>();
            usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Add(playerCharacterEntity.ObjectId);
            OpenStorageRoutine(playerCharacterEntity).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid OpenStorageRoutine(BasePlayerCharacterEntity playerCharacterEntity)
        {
            ReadStorageItemsReq req = new ReadStorageItemsReq();
            req.StorageType = (EStorageType)playerCharacterEntity.CurrentStorageId.storageType;
            req.StorageOwnerId = playerCharacterEntity.CurrentStorageId.storageOwnerId;
            ReadStorageItemsResp resp = await DbServiceClient.ReadStorageItemsAsync(req);
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            playerCharacterEntity.StorageItems = storageItems.ToArray();
            allStorageItems[playerCharacterEntity.CurrentStorageId] = storageItems;
        }
#endif

        public override void CloseStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Remove(playerCharacterEntity.ObjectId);
            playerCharacterEntity.StorageItems = new CharacterItem[0];
#endif
        }

        public override void MoveItemToStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short nonEquipIndex, short amount, short storageItemIndex)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacterEntity, storageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            MoveItemToStorageRoutine(playerCharacterEntity, storageId, nonEquipIndex, amount, storageItemIndex).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid MoveItemToStorageRoutine(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short nonEquipIndex, short amount, short storageItemIndex)
        {
            MoveItemToStorageReq req = new MoveItemToStorageReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.CharacterId = playerCharacterEntity.Id;
            req.MapName = CurrentMapInfo.Id;
            req.InventoryItemIndex = nonEquipIndex;
            req.InventoryItemAmount = amount;
            req.StorageItemIndex = storageItemIndex;
            MoveItemToStorageResp resp = await DbServiceClient.MoveItemToStorageAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                // TODO: May push error message
                return;
            }
            playerCharacterEntity.NonEquipItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.InventoryItemItems);
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
            allStorageItems[storageId] = storageItems;
        }
#endif

        public override void MoveItemFromStorage(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short storageItemIndex, short amount, short nonEquipIndex)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacterEntity, storageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            MoveItemFromStorageRoutine(playerCharacterEntity, storageId, storageItemIndex, amount, nonEquipIndex).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid MoveItemFromStorageRoutine(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short storageItemIndex, short amount, short nonEquipIndex)
        {
            MoveItemFromStorageReq req = new MoveItemFromStorageReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.CharacterId = playerCharacterEntity.Id;
            req.MapName = CurrentMapInfo.Id;
            req.StorageItemIndex = storageItemIndex;
            req.StorageItemAmount = amount;
            req.InventoryItemIndex = nonEquipIndex;
            MoveItemFromStorageResp resp = await DbServiceClient.MoveItemFromStorageAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                // TODO: May push error message
                return;
            }
            playerCharacterEntity.NonEquipItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.InventoryItemItems);
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
            allStorageItems[storageId] = storageItems;
        }
#endif

        public override void IncreaseStorageItems(StorageId storageId, CharacterItem addingItem, Action<bool> callback)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IncreaseStorageItemsRoutine(storageId, addingItem, callback).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid IncreaseStorageItemsRoutine(StorageId storageId, CharacterItem addingItem, Action<bool> callback)
        {
            IncreaseStorageItemsReq req = new IncreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.MapName = CurrentMapInfo.Id;
            req.Item = DatabaseServiceUtils.ToByteString(addingItem);
            IncreaseStorageItemsResp resp = await DbServiceClient.IncreaseStorageItemsAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                // TODO: May push error message
                if (resp.Error == EStorageError.StorageErrorStorageWillOverwhelming && callback != null)
                    callback.Invoke(false);
                return;
            }
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
            allStorageItems[storageId] = storageItems;
            if (callback != null)
                callback.Invoke(true);
        }
#endif

        public override void DecreaseStorageItems(StorageId storageId, int dataId, short amount, Action<bool, Dictionary<int, short>> callback)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            DecreaseStorageItemsRoutine(storageId, dataId, amount, callback).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid DecreaseStorageItemsRoutine(StorageId storageId, int dataId, short amount, Action<bool, Dictionary<int, short>> callback)
        {
            DecreaseStorageItemsReq req = new DecreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.MapName = CurrentMapInfo.Id;
            req.DataId = dataId;
            req.Amount = amount;
            DecreaseStorageItemsResp resp = await DbServiceClient.DecreaseStorageItemsAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                // TODO: May push error message
                if (resp.Error == EStorageError.StorageErrorStorageWillOverwhelming && callback != null)
                    callback.Invoke(false, new Dictionary<int, short>());
                return;
            }
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
            allStorageItems[storageId] = storageItems;
            Dictionary<int, short> decreasedItems = new Dictionary<int, short>();
            foreach (ItemIndexAmountMap entry in resp.DecreasedItems)
            {
                decreasedItems.Add(entry.Index, (short)entry.Amount);
            }
            if (callback != null)
                callback.Invoke(true, decreasedItems);
        }
#endif

        public override void SwapOrMergeStorageItem(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short fromIndex, short toIndex)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacterEntity, storageId))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            SwapOrMergeStorageItemRoutine(playerCharacterEntity, storageId, fromIndex, toIndex).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid SwapOrMergeStorageItemRoutine(BasePlayerCharacterEntity playerCharacterEntity, StorageId storageId, short fromIndex, short toIndex)
        {
            SwapOrMergeStorageItemReq req = new SwapOrMergeStorageItemReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.CharacterId = playerCharacterEntity.Id;
            req.MapName = CurrentMapInfo.Id;
            req.FromIndex = fromIndex;
            req.ToIndex = toIndex;
            SwapOrMergeStorageItemResp resp = await DbServiceClient.SwapOrMergeStorageItemAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                // TODO: May push error message
                return;
            }
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            UpdateStorageItemsToCharacters(usingStorageCharacters[storageId], storageItems);
            allStorageItems[storageId] = storageItems;
        }
#endif

        public override bool IsStorageEntityOpen(StorageEntity storageEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (storageEntity == null)
                return false;
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            return usingStorageCharacters.ContainsKey(id) &&
                usingStorageCharacters[id].Count > 0;
#else
            return false;
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private void UpdateStorageItemsToCharacters(HashSet<uint> objectIds, List<CharacterItem> items)
        {
            BasePlayerCharacterEntity playerCharacterEntity;
            CharacterItem[] storageItems = items.ToArray();
            foreach (uint objectId in objectIds)
            {
                if (Assets.TryGetSpawnedObject(objectId, out playerCharacterEntity))
                {
                    // Update storage items
                    playerCharacterEntity.StorageItems = storageItems;
                }
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
                playerCharacterEntity.Gold += amount;
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
            if (guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
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
                    guilds[playerCharacterEntity.GuildId] = guild;
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
            if (guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
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
                    playerCharacterEntity.Gold += amount;
                    guilds[playerCharacterEntity.GuildId] = guild;
                    SendSetGuildGoldToClients(guild);
                }
                else
                    SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotEnoughGoldToWithdraw);
            }
            else
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.NotJoinedGuild);
        }
#endif

        public override void FindCharacters(BasePlayerCharacterEntity playerCharacterEntity, string characterName)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            FindCharactersRoutine(playerCharacterEntity, characterName).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid FindCharactersRoutine(BasePlayerCharacterEntity playerCharacterEntity, string characterName)
        {
            FindCharactersResp resp = await DbServiceClient.FindCharactersAsync(new FindCharactersReq()
            {
                CharacterName = characterName
            });
            SendUpdateFoundCharactersToClient(playerCharacterEntity.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
        }
#endif

        public override void AddFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            AddFriendRoutine(playerCharacterEntity, friendCharacterId).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid AddFriendRoutine(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
            ReadFriendsResp resp = await DbServiceClient.CreateFriendAsync(new CreateFriendReq()
            {
                Character1Id = playerCharacterEntity.Id,
                Character2Id = friendCharacterId
            });
            SendUpdateFriendsToClient(playerCharacterEntity.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
        }
#endif

        public override void RemoveFriend(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            RemoveFriendRoutine(playerCharacterEntity, friendCharacterId).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid RemoveFriendRoutine(BasePlayerCharacterEntity playerCharacterEntity, string friendCharacterId)
        {
            ReadFriendsResp resp = await DbServiceClient.DeleteFriendAsync(new DeleteFriendReq()
            {
                Character1Id = playerCharacterEntity.Id,
                Character2Id = friendCharacterId
            });
            SendUpdateFriendsToClient(playerCharacterEntity.ConnectionId, resp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
        }
#endif

        public override void GetFriends(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GetFriendsRoutine(playerCharacterEntity).Forget();
#endif
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid GetFriendsRoutine(BasePlayerCharacterEntity playerCharacterEntity)
        {
            ReadFriendsResp readFriendsResp = await DbServiceClient.ReadFriendsAsync(new ReadFriendsReq()
            {
                CharacterId = playerCharacterEntity.Id
            });
            SendUpdateFriendsToClient(playerCharacterEntity.ConnectionId, readFriendsResp.List.MakeArrayFromRepeatedByteString<SocialCharacterData>());
        }
#endif
    }
}
