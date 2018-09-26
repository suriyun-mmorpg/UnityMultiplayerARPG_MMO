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
            var setCharacterPartyJob = new SetCharacterPartyJob(Database, playerCharacterEntity.Id, partyId);
            setCharacterPartyJob.Start();
            yield return StartCoroutine(setCharacterPartyJob.WaitFor());
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
        }

        public override void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            PartyData party;
            if (!parties.TryGetValue(playerCharacterEntity.PartyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            new UpdatePartyJob(Database, playerCharacterEntity.PartyId, shareExp, shareItem).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartySetting(playerCharacterEntity.PartyId, shareExp, shareItem);
        }

        public override void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            PartyData party;
            if (!parties.TryGetValue(inviteCharacterEntity.PartyId, out party))
                return;
            if (!party.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            if (party.CountMember() == gameInstance.maxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return;
            }
            new SetCharacterPartyJob(Database, acceptCharacterEntity.Id, inviteCharacterEntity.PartyId).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberAdd(inviteCharacterEntity.PartyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            PartyData party;
            if (!parties.TryGetValue(playerCharacterEntity.PartyId, out party))
                return;
            if (!party.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not party leader
                return;
            }
            new SetCharacterPartyJob(Database, characterId, 0).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberRemove(playerCharacterEntity.PartyId, characterId);
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
                    new SetCharacterPartyJob(Database, memberId, 0).Start();
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdatePartyMemberRemove(partyId, memberId);
                }
                new DeletePartyJob(Database, partyId).Start();
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdatePartyTerminate(partyId);
            }
            else
            {
                new SetCharacterPartyJob(Database, playerCharacterEntity.Id, 0).Start();
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
            var setCharacterGuildJob = new SetCharacterGuildJob(Database, playerCharacterEntity.Id, guildId);
            setCharacterGuildJob.Start();
            yield return StartCoroutine(setCharacterGuildJob.WaitFor());
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
        }

        public override void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string message)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            GuildData guild;
            if (!guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
                return;
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            new SetGuildMessageJob(Database, playerCharacterEntity.GuildId, message).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateSetGuildMessage(playerCharacterEntity.GuildId, message);
        }

        public override void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
        {
            if (inviteCharacterEntity == null || acceptCharacterEntity == null || !IsServer)
                return;
            GuildData guild;
            if (!guilds.TryGetValue(inviteCharacterEntity.GuildId, out guild))
                return;
            if (!guild.IsLeader(inviteCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            if (guild.CountMember() == gameInstance.maxGuildMember)
            {
                // TODO: May warn that it's exceeds limit max guild member
                return;
            }
            new SetCharacterGuildJob(Database, acceptCharacterEntity.Id, inviteCharacterEntity.GuildId).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(inviteCharacterEntity.GuildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            GuildData guild;
            if (!guilds.TryGetValue(playerCharacterEntity.GuildId, out guild))
                return;
            if (!guild.IsLeader(playerCharacterEntity))
            {
                // TODO: May warn that it's not guild leader
                return;
            }
            new SetCharacterGuildJob(Database, characterId, 0).Start();
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberRemove(playerCharacterEntity.GuildId, characterId);
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
                    new SetCharacterGuildJob(Database, memberId, 0).Start();
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdateGuildMemberRemove(guildId, memberId);
                }
                new DeleteGuildJob(Database, guildId).Start();
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildTerminate(guildId);
            }
            else
            {
                new SetCharacterGuildJob(Database, playerCharacterEntity.Id, 0).Start();
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildMemberRemove(guildId, playerCharacterEntity.Id);
            }
        }
    }
}
