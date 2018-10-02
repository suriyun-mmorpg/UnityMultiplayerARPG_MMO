using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;

            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
            {
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
                return;
            }

            StartCoroutine(WarpCharacterRoutine(playerCharacterEntity, mapName, position));
        }

        private IEnumerator WarpCharacterRoutine(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            // If warping to different map
            var connectId = playerCharacterEntity.ConnectId;
            CentralServerPeerInfo peerInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectId) &&
                mapServerConnectionIdsBySceneName.TryGetValue(mapName, out peerInfo))
            {
                // Unregister player character
                UnregisterPlayerCharacter(connectId);
                // Clone character data to save
                var savingCharacterData = new PlayerCharacterData();
                playerCharacterEntity.CloneTo(savingCharacterData);
                // Save character current map / position
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                while (savingCharacters.Contains(savingCharacterData.Id))
                {
                    yield return 0;
                }
                yield return StartCoroutine(SaveCharacterRoutine(savingCharacterData));
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Send message to client to warp
                var message = new MMOWarpMessage();
                message.sceneName = mapName;
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                message.connectKey = peerInfo.connectKey;
                ServerSendPacket(connectId, SendOptions.ReliableOrdered, MsgTypes.Warp, message);
            }
        }

        public override void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            StartCoroutine(CreatePartyRoutine(playerCharacterEntity, shareExp, shareItem));
        }

        private IEnumerator CreatePartyRoutine(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            var createPartyJob = new CreatePartyJob(Database, shareExp, shareItem, playerCharacterEntity.Id);
            createPartyJob.Start();
            yield return StartCoroutine(createPartyJob.WaitFor());
            var partyId = createPartyJob.result;
            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
            playerCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(playerCharacterEntity);
            // Save to database
            new SetCharacterPartyJob(Database, playerCharacterEntity.Id, partyId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberAdd(partyId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
        }

        public override void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            party.Setting(shareExp, shareItem);
            parties[partyId] = party;
            // Save to database
            new UpdatePartyJob(Database, partyId, shareExp, shareItem).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartySetting(partyId, shareExp, shareItem);
        }

        public override void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            var partyId = inviteCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            if (party.CountMember() == gameInstance.SocialSystemSetting.maxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return;
            }
            party.AddMember(acceptCharacterEntity);
            parties[partyId] = party;
            acceptCharacterEntity.PartyId = partyId;
            acceptCharacterEntity.PartyMemberFlags = party.GetPartyMemberFlags(acceptCharacterEntity);
            // Save to database
            new SetCharacterPartyJob(Database, acceptCharacterEntity.Id, partyId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberAdd(partyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
                memberCharacterEntity.ClearParty();
            party.RemoveMember(characterId);
            parties[partyId] = party;
            // Save to database
            new SetCharacterPartyJob(Database, characterId, 0).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberRemove(partyId, characterId);
        }

        public override void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = playerCharacterEntity.PartyId;
            PartyData party;
            if (!parties.TryGetValue(partyId, out party))
                return;
            // If it is leader kick all members and terminate party
            if (party.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in party.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                        memberCharacterEntity.ClearParty();
                    // Save to database
                    new SetCharacterPartyJob(Database, memberId, 0).Start();
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdatePartyMemberRemove(partyId, memberId);
                }
                parties.Remove(partyId);
                // Save to database
                new DeletePartyJob(Database, partyId).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdatePartyTerminate(partyId);
            }
            else
            {
                playerCharacterEntity.ClearParty();
                party.RemoveMember(playerCharacterEntity.Id);
                parties[partyId] = party;
                // Save to database
                new SetCharacterPartyJob(Database, playerCharacterEntity.Id, 0).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdatePartyMemberRemove(partyId, playerCharacterEntity.Id);
            }
        }

        public override void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            StartCoroutine(CreateGuildRoutine(playerCharacterEntity, guildName));
        }

        private IEnumerator CreateGuildRoutine(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            var createGuildJob = new CreateGuildJob(Database, guildName, playerCharacterEntity.Id, playerCharacterEntity.CharacterName);
            createGuildJob.Start();
            yield return StartCoroutine(createGuildJob.WaitFor());
            var guildId = createGuildJob.result;
            var guild = new GuildData(guildId, guildName, playerCharacterEntity);
            byte guildRole;
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
            playerCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
            playerCharacterEntity.GuildRole = guildRole;
            // Save to database
            new SetCharacterGuildJob(Database, playerCharacterEntity.Id, guildId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(guildId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
        }

        public override void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var guildId = playerCharacterEntity.GuildId;
            GuildData guild;
            if (!guilds.TryGetValue(guildId, out guild))
                return;
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            guild.guildMessage = guildMessage;
            guilds[guildId] = guild;
            // Save to database
            new SetGuildMessageJob(Database, guildId, guildMessage).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateSetGuildMessage(guildId, guildMessage);
        }

        public override void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            var guildId = inviteCharacterEntity.GuildId;
            GuildData guild;
            if (!guilds.TryGetValue(guildId, out guild))
                return;
            if (!guild.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            if (guild.CountMember() == gameInstance.SocialSystemSetting.maxGuildMember)
            {
                // TODO: May warn that it's exceeds limit max guild member
                return;
            }
            guild.AddMember(acceptCharacterEntity);
            byte guildRole;
            guilds[guildId] = guild;
            acceptCharacterEntity.GuildId = guildId;
            acceptCharacterEntity.GuildMemberFlags = guild.GetGuildMemberFlagsAndRole(acceptCharacterEntity, out guildRole);
            acceptCharacterEntity.GuildRole = guildRole;
            // Save to database
            new SetCharacterGuildJob(Database, acceptCharacterEntity.Id, guildId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(guildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var guildId = playerCharacterEntity.GuildId;
            GuildData guild;
            if (!guilds.TryGetValue(guildId, out guild))
                return;
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            BasePlayerCharacterEntity memberCharacterEntity;
            if (playerCharactersById.TryGetValue(characterId, out memberCharacterEntity))
                memberCharacterEntity.ClearGuild();
            guild.RemoveMember(characterId);
            guilds[guildId] = guild;
            // Save to database
            new SetCharacterGuildJob(Database, characterId, 0, 0).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberRemove(guildId, characterId);
        }

        public override void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var guildId = playerCharacterEntity.GuildId;
            GuildData guild;
            if (!guilds.TryGetValue(guildId, out guild))
                return;
            // If it is leader kick all members and terminate guild
            if (guild.IsLeader(playerCharacterEntity))
            {
                foreach (var memberId in guild.GetMemberIds())
                {
                    BasePlayerCharacterEntity memberCharacterEntity;
                    if (playerCharactersById.TryGetValue(memberId, out memberCharacterEntity))
                        memberCharacterEntity.ClearGuild();
                    // Save to database
                    new SetCharacterGuildJob(Database, memberId, 0, 0).Start();
                    // Broadcast via chat server
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdateGuildMemberRemove(guildId, memberId);
                }
                // Save to database
                new DeleteGuildJob(Database, guildId).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildTerminate(guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                // Save to database
                new SetCharacterGuildJob(Database, playerCharacterEntity.Id, 0, 0).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildMemberRemove(guildId, playerCharacterEntity.Id);
            }
        }
    }
}
