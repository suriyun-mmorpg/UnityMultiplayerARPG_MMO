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
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            base.WarpCharacter(playerCharacterEntity, mapName, position);
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
            if (!CanCreateParty(playerCharacterEntity))
                return;
            StartCoroutine(CreatePartyRoutine(playerCharacterEntity, shareExp, shareItem));
        }

        private IEnumerator CreatePartyRoutine(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            var createPartyJob = new CreatePartyJob(Database, shareExp, shareItem, playerCharacterEntity.Id);
            createPartyJob.Start();
            yield return StartCoroutine(createPartyJob.WaitFor());
            var partyId = createPartyJob.result;
            // Create party
            base.CreateParty(playerCharacterEntity, shareExp, shareItem, partyId);
            // Save to database
            new SetCharacterPartyJob(Database, playerCharacterEntity.Id, partyId).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberAdd(partyId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
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
                ChatNetworkManager.UpdatePartySetting(partyId, shareExp, shareItem);
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
                ChatNetworkManager.UpdatePartyMemberAdd(partyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
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
                ChatNetworkManager.UpdatePartyMemberRemove(partyId, characterId);
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
            if (!CanCreateGuild(playerCharacterEntity))
                return;
            StartCoroutine(CreateGuildRoutine(playerCharacterEntity, guildName));
        }

        private IEnumerator CreateGuildRoutine(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            var createGuildJob = new CreateGuildJob(Database, guildName, playerCharacterEntity.Id, playerCharacterEntity.CharacterName);
            createGuildJob.Start();
            yield return StartCoroutine(createGuildJob.WaitFor());
            var guildId = createGuildJob.result;
            // Create guild
            base.CreateGuild(playerCharacterEntity, guildName, guildId);
            // Retrieve required data
            byte guildRole;
            guilds[guildId].GetGuildMemberFlagsAndRole(playerCharacterEntity, out guildRole);
            // Save to database
            new SetCharacterGuildJob(Database, playerCharacterEntity.Id, guildId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(guildId, playerCharacterEntity.Id, playerCharacterEntity.CharacterName, playerCharacterEntity.DataId, playerCharacterEntity.Level);
        }

        public override void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string guildMessage)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMessage(playerCharacterEntity, out guildId, out guild))
                return;

            base.SetGuildMessage(playerCharacterEntity, guildMessage);
            // Save to database
            new SetGuildMessageJob(Database, guildId, guildMessage).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateSetGuildMessage(guildId, guildMessage);
        }

        public override void SetGuildMemberRole(BasePlayerCharacterEntity playerCharacterEntity, string characterId, byte guildRole)
        {
            int guildId;
            GuildData guild;
            if (!CanSetGuildMemberRole(playerCharacterEntity, out guildId, out guild))
                return;

            base.SetGuildMemberRole(playerCharacterEntity, characterId, guildRole);
            // Save to database
            new SetGuildMemberRoleJob(Database, characterId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateSetGuildMemberRole(guildId, characterId, guildRole);
        }

        public override void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            int guildId;
            GuildData guild;
            if (!CanAddGuildMember(inviteCharacterEntity, acceptCharacterEntity, out guildId, out guild))
                return;

            base.AddGuildMember(inviteCharacterEntity, acceptCharacterEntity);
            byte guildRole;
            guild.GetGuildMemberFlagsAndRole(acceptCharacterEntity, out guildRole);
            // Save to database
            new SetCharacterGuildJob(Database, acceptCharacterEntity.Id, guildId, guildRole).Start();
            // Broadcast via chat server
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(guildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
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
                ChatNetworkManager.UpdateGuildMemberRemove(guildId, characterId);
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
                guilds.Remove(guildId);
                // Save to database
                new DeleteGuildJob(Database, guildId).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildTerminate(guildId);
            }
            else
            {
                playerCharacterEntity.ClearGuild();
                guild.RemoveMember(playerCharacterEntity.Id);
                guilds[guildId] = guild;
                // Save to database
                new SetCharacterGuildJob(Database, playerCharacterEntity.Id, 0, 0).Start();
                // Broadcast via chat server
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildMemberRemove(guildId, playerCharacterEntity.Id);
            }
        }
    }
}
