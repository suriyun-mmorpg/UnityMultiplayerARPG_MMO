using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override string GetCurrentMapName(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!IsInstanceMap())
                return base.GetCurrentMapName(playerCharacterEntity);
            return instanceMapCurrentLocations[playerCharacterEntity.ObjectId].Key;
        }

        public override Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!IsInstanceMap())
                return base.GetCurrentPosition(playerCharacterEntity);
            return instanceMapCurrentLocations[playerCharacterEntity.ObjectId].Value;
        }

        public override bool CanWarpCharacter(BasePlayerCharacterEntity playerCharacterEntity)
        {
            return base.CanWarpCharacter(playerCharacterEntity) && !warpingCharactersByObjectId.Contains(playerCharacterEntity.ObjectId);
        }

        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool isInstanceMap)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            base.WarpCharacter(playerCharacterEntity, mapName, position, isInstanceMap);
            StartCoroutine(WarpCharacterRoutine(playerCharacterEntity, mapName, position, isInstanceMap));
        }

        private IEnumerator WarpCharacterRoutine(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool isInstanceMap)
        {
            // If warping to different map
            long connectionId = playerCharacterEntity.ConnectionId;
            CentralServerPeerInfo peerInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectionId) &&
                mapServerConnectionIdsBySceneName.TryGetValue(mapName, out peerInfo))
            {
                // Add this character to warping list
                warpingCharactersByObjectId.Add(playerCharacterEntity.ObjectId);
                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                // Clone character data to save
                PlayerCharacterData savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                // Save character current map / position
                if (!isInstanceMap)
                {
                    savingCharacterData.CurrentMapName = mapName;
                    savingCharacterData.CurrentPosition = position;
                }
                while (savingCharacters.Contains(savingCharacterData.Id))
                {
                    yield return 0;
                }
                yield return StartCoroutine(SaveCharacterRoutine(savingCharacterData));
                // Remove this character from warping list
                warpingCharactersByObjectId.Remove(playerCharacterEntity.ObjectId);
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Send message to client to warp
                MMOWarpMessage message = new MMOWarpMessage();
                message.sceneName = mapName;
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                message.connectKey = peerInfo.connectKey;
                ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MsgTypes.Warp, message);
            }
        }

        public override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            // Generate instance id
            string instanceId = GenericUtils.GetUniqueId();
            RequestSpawnMapMessage requestSpawnMapMessage = new RequestSpawnMapMessage();
            requestSpawnMapMessage.sceneName = mapName;
            requestSpawnMapMessage.instanceId = instanceId;
            // Prepare data for warp character later when instance map server registered to this map server
            HashSet<uint> instanceMapWarpingCharacters = new HashSet<uint>();
            instanceMapWarpingCharacters.Add(playerCharacterEntity.ObjectId);
            warpingCharactersByObjectId.Add(playerCharacterEntity.ObjectId);
            instanceMapWarpingCharactersByInstanceId.Add(instanceId, instanceMapWarpingCharacters);
            instanceMapWarpingLocations.Add(instanceId, new KeyValuePair<string, Vector3>(mapName, position));
            CentralAppServerRegister.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestSpawnMap, requestSpawnMapMessage, OnRequestSpawnMap);
        }

        private void OnRequestSpawnMap(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            ResponseSpawnMapMessage castedMessage = messageData as ResponseSpawnMapMessage;
            if (LogInfo)
                Debug.Log("Spawn Map Ack Id: " + messageData.ackId + "  Status: " + responseCode + " Error: " + castedMessage.error);
            if (responseCode == AckResponseCode.Error ||
                responseCode == AckResponseCode.Timeout)
            {
                // Remove warping characters who warping to instance map
                HashSet<uint> instanceMapWarpingCharacters;
                if (instanceMapWarpingCharactersByInstanceId.TryGetValue(castedMessage.instanceId, out instanceMapWarpingCharacters))
                {
                    foreach (uint warpingCharacter in instanceMapWarpingCharacters)
                    {
                        warpingCharactersByObjectId.Remove(warpingCharacter);
                    }
                    instanceMapWarpingCharactersByInstanceId.Remove(castedMessage.instanceId);
                }
                instanceMapWarpingLocations.Remove(castedMessage.instanceId);
            }
        }

        public override void WarpCharacterToInstanceFollowPartyLeader(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            // TODO: Implement this
        }

        public override bool IsInstanceMap()
        {
            return !string.IsNullOrEmpty(instanceId);
        }

        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (!CanCreateParty(playerCharacterEntity))
                return;
            StartCoroutine(CreatePartyRoutine(playerCharacterEntity, shareExp, shareItem));
        }

        private IEnumerator CreatePartyRoutine(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            CreatePartyJob createPartyJob = new CreatePartyJob(Database, shareExp, shareItem, playerCharacterEntity.Id);
            createPartyJob.Start();
            yield return StartCoroutine(createPartyJob.WaitFor());
            int partyId = createPartyJob.result;
            // Create party
            base.CreateParty(playerCharacterEntity, shareExp, shareItem, partyId);
            // Save to database
            new SetCharacterPartyJob(Database, playerCharacterEntity.Id, partyId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.Client.SendCreateParty(null, MMOMessageTypes.UpdateParty, partyId, shareExp, shareItem, playerCharacterEntity.Id);
                ChatNetworkManager.Client.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
            }
        }

        public override void ChangePartyLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanChangePartyLeader(playerCharacterEntity, characterId, out partyId, out party))
                return;

            base.ChangePartyLeader(playerCharacterEntity, characterId);
            // Save to database
            new UpdatePartyLeaderJob(Database, partyId, characterId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendChangePartyLeader(null, MMOMessageTypes.UpdateParty, partyId, characterId);
        }

        public override void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            int partyId;
            PartyData party;
            if (!CanPartySetting(playerCharacterEntity, out partyId, out party))
                return;

            base.PartySetting(playerCharacterEntity, shareExp, shareItem);
            // Save to database
            new UpdatePartyJob(Database, partyId, shareExp, shareItem).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendPartySetting(null, MMOMessageTypes.UpdateParty, partyId, shareExp, shareItem);
        }

        public override void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int partyId;
            PartyData party;
            if (!CanAddPartyMember(inviteCharacterEntity, acceptCharacterEntity, out partyId, out party))
                return;

            base.AddPartyMember(inviteCharacterEntity, acceptCharacterEntity);
            // Save to database
            new SetCharacterPartyJob(Database, acceptCharacterEntity.Id, partyId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendAddSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int partyId;
            PartyData party;
            if (!CanKickFromParty(playerCharacterEntity, characterId, out partyId, out party))
                return;

            base.KickFromParty(playerCharacterEntity, characterId);
            // Save to database
            new SetCharacterPartyJob(Database, characterId, 0).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, characterId);
        }

        public override void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
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
                    new SetCharacterPartyJob(Database, memberId, 0).Start();
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, memberId);
                }
                parties.Remove(partyId);
                // Save to database
                new DeletePartyJob(Database, partyId).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.Client.SendPartyTerminate(null, MMOMessageTypes.UpdateParty, partyId);
            }
            else
            {
                playerCharacterEntity.ClearParty();
                SendPartyTerminateToClient(playerCharacterEntity.ConnectionId, partyId);
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
                SendRemovePartyMemberToClients(party, playerCharacterEntity.Id);
                // Save to database
                new SetCharacterPartyJob(Database, playerCharacterEntity.Id, 0).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdatePartyMember, partyId, playerCharacterEntity.Id);
            }
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            if (!CanCreateGuild(playerCharacterEntity))
                return;
            StartCoroutine(CreateGuildRoutine(playerCharacterEntity, guildName));
        }

        private IEnumerator CreateGuildRoutine(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            CreateGuildJob createGuildJob = new CreateGuildJob(Database, guildName, playerCharacterEntity.Id);
            createGuildJob.Start();
            yield return StartCoroutine(createGuildJob.WaitFor());
            int guildId = createGuildJob.result;
            // Create guild
            base.CreateGuild(playerCharacterEntity, guildName, guildId);
            // Save to database
            new SetCharacterGuildJob(Database, playerCharacterEntity.Id, guildId, guilds[guildId].GetMemberRole(playerCharacterEntity.Id)).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.Client.SendCreateGuild(null, MMOMessageTypes.UpdateGuild, guildId, guildName, playerCharacterEntity.Id);
                ChatNetworkManager.Client.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
            }
        }

        public override void ChangeGuildLeader(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanChangeGuildLeader(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            base.ChangeGuildLeader(playerCharacterEntity, characterId);
            // Save to database
            new UpdateGuildLeaderJob(Database, guildId, characterId).Start();
            new UpdateGuildMemberRoleJob(Database, characterId, guild.GetMemberRole(characterId)).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendChangeGuildLeader(null, MMOMessageTypes.UpdateGuild, guildId, characterId);
        }

        public override void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMessage(playerCharacterEntity, out guildId, out guild))
                return;

            base.SetGuildMessage(playerCharacterEntity, guildMessage);
            // Save to database
            new UpdateGuildMessageJob(Database, guildId, guildMessage).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendSetGuildMessage(null, MMOMessageTypes.UpdateGuild, guildId, guildMessage);
        }

        public override void SetGuildRole(BasePlayerCharacterEntity playerCharacterEntity, byte guildRole, string roleName, bool canInvite, bool canKick, byte shareExpPercentage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildRole(playerCharacterEntity, guildRole, out guildId, out guild))
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
                    new UpdateGuildMemberRoleJob(Database, memberId, guildRole).Start();
                }
            }
            SendSetGuildRoleToClients(guild, guildRole, roleName, canInvite, canKick, shareExpPercentage);
            // Save to database
            new UpdateGuildRoleJob(Database, guildId, guildRole, roleName, canInvite, canKick, shareExpPercentage).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendSetGuildRole(null, MMOMessageTypes.UpdateGuild, guildId, guildRole, roleName, canInvite, canKick, shareExpPercentage);
        }

        public override void SetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, string characterId, byte guildRole)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMemberRole(playerCharacterEntity, out guildId, out guild))
                return;

            base.SetGuildMemberRole(playerCharacterEntity, characterId, guildRole);
            // Save to database
            new UpdateGuildMemberRoleJob(Database, characterId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendSetGuildMemberRole(null, MMOMessageTypes.UpdateGuild, guildId, characterId, guildRole);
        }

        public override void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildMember(inviteCharacterEntity, acceptCharacterEntity, out guildId, out guild))
                return;

            base.AddGuildMember(inviteCharacterEntity, acceptCharacterEntity);
            // Save to database
            new SetCharacterGuildJob(Database, acceptCharacterEntity.Id, guildId, guild.GetMemberRole(acceptCharacterEntity.Id)).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendAddSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            int guildId;
            GuildData guild;
            if (!CanKickFromGuild(playerCharacterEntity, characterId, out guildId, out guild))
                return;

            base.KickFromGuild(playerCharacterEntity, characterId);
            // Save to database
            new SetCharacterGuildJob(Database, characterId, 0, 0).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, characterId);
        }

        public override void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
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
                    new SetCharacterGuildJob(Database, memberId, 0, 0).Start();
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, memberId);
                }
                guilds.Remove(guildId);
                // Save to database
                new DeleteGuildJob(Database, guildId).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.Client.SendGuildTerminate(null, MMOMessageTypes.UpdateGuild, guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                SendGuildTerminateToClient(playerCharacterEntity.ConnectionId, guildId);
                guild.RemoveMember(playerCharacterEntity.Id);
                guilds[guildId] = guild;
                SendRemoveGuildMemberToClients(guild, playerCharacterEntity.Id);
                // Save to database
                new SetCharacterGuildJob(Database, playerCharacterEntity.Id, 0, 0).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.Client.SendRemoveSocialMember(null, MMOMessageTypes.UpdateGuildMember, guildId, playerCharacterEntity.Id);
            }
        }

        public override void IncreaseGuildExp(BasePlayerCharacterEntity playerCharacterEntity, int exp)
        {
            int guildId;
            GuildData guild;
            if (!CanIncreaseGuildExp(playerCharacterEntity, exp, out guildId, out guild))
                return;
            StartCoroutine(IncreaseGuildExpRoutine(playerCharacterEntity, exp, guildId, guild));
        }

        private IEnumerator IncreaseGuildExpRoutine(BasePlayerCharacterEntity playerCharacterEntity, int exp, int guildId, GuildData guild)
        {
            IncreaseGuildExpJob job = new IncreaseGuildExpJob(Database, guildId, exp, gameInstance.SocialSystemSetting.GuildExpTree);
            job.Start();
            yield return StartCoroutine(job.WaitFor());
            if (job.result)
            {
                guild.level = job.resultLevel;
                guild.exp = job.resultExp;
                guild.skillPoint = job.resultSkillPoint;
                guilds[guildId] = guild;
                SendGuildLevelExpSkillPointToClients(guild);
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.Client.SendGuildLevelExpSkillPoint(null, MMOMessageTypes.UpdateGuild, guildId, guild.level, guild.exp, guild.skillPoint);
            }
        }

        public override void AddGuildSkill(BasePlayerCharacterEntity playerCharacterEntity, int dataId)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildSkill(playerCharacterEntity, dataId, out guildId, out guild))
                return;
            
            base.AddGuildSkill(playerCharacterEntity, dataId);
            // Save to database
            new UpdateGuildSkillLevelJob(Database, guildId, dataId, guild.GetSkillLevel(dataId), guild.skillPoint).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
            {
                ChatNetworkManager.Client.SendSetGuildSkillLevel(null, MMOMessageTypes.UpdateGuild, guildId, dataId, guild.GetSkillLevel(dataId));
                ChatNetworkManager.Client.SendGuildLevelExpSkillPoint(null, MMOMessageTypes.UpdateGuild, guildId, guild.level, guild.exp, guild.skillPoint);
            }
        }
    }
}
