using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override async void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            // If warping to same map player does not have to reload new map data
            if (string.IsNullOrEmpty(mapName) || mapName.Equals(playerCharacterEntity.CurrentMapName))
            {
                playerCharacterEntity.CacheNetTransform.Teleport(position, Quaternion.identity);
                return;
            }
            // If warping to different map
            long connectId = playerCharacterEntity.ConnectId;
            CentralServerPeerInfo peerInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                !mapName.Equals(playerCharacterEntity.CurrentMapName) &&
                playerCharacters.ContainsKey(connectId) &&
                ConnectionIds.Contains(connectId) &&
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
                await SaveCharacter(savingCharacterData);
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

        public override async void CreateParty(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var partyId = await Database.CreateParty(shareExp, shareItem, playerCharacterEntity.Id);
            var party = new PartyData(partyId, shareExp, shareItem, playerCharacterEntity);
            await Database.SetCharacterParty(playerCharacterEntity.Id, partyId);
            parties[partyId] = party;
            playerCharacterEntity.PartyId = partyId;
        }

        public override async void PartySetting(BasePlayerCharacterEntity playerCharacterEntity, bool shareExp, bool shareItem)
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
            await Database.UpdateParty(playerCharacterEntity.PartyId, shareExp, shareItem);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartySetting(partyId, shareExp, shareItem);
        }

        public override async void AddPartyMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
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
            if (party.CountMember() == gameInstance.maxPartyMember)
            {
                // TODO: May warn that it's exceeds limit max party member
                return;
            }
            await Database.SetCharacterParty(acceptCharacterEntity.Id, partyId);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberAdd(partyId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override async void KickFromParty(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
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
            await Database.SetCharacterParty(characterId, 0);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdatePartyMemberRemove(partyId, characterId);
        }

        public override async void LeaveParty(BasePlayerCharacterEntity playerCharacterEntity)
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
                var tasks = new List<Task>();
                foreach (var memberId in party.GetMemberIds())
                {
                    tasks.Add(Database.SetCharacterParty(memberId, 0));
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdatePartyMemberRemove(partyId, memberId);
                }
                tasks.Add(Database.DeleteParty(partyId));
                await Task.WhenAll(tasks);
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdatePartyTerminate(partyId);
            }
            else
            {
                await Database.SetCharacterParty(playerCharacterEntity.Id, 0);
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdatePartyMemberRemove(partyId, playerCharacterEntity.Id);
            }
        }

        public override async void CreateGuild(BasePlayerCharacterEntity playerCharacterEntity, string guildName)
        {
            if (playerCharacterEntity == null || !IsServer)
                return;
            var guildId = await Database.CreateGuild(guildName, playerCharacterEntity.Id, playerCharacterEntity.CharacterName);
            var guild = new GuildData(guildId, guildName, playerCharacterEntity);
            await Database.SetCharacterGuild(playerCharacterEntity.Id, guildId);
            guilds[guildId] = guild;
            playerCharacterEntity.GuildId = guildId;
        }

        public override async void SetGuildMessage(BasePlayerCharacterEntity playerCharacterEntity, string message)
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
            await Database.UpdateGuildMessage(playerCharacterEntity.GuildId, message);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateSetGuildMessage(guildId, message);
        }

        public override async void AddGuildMember(BasePlayerCharacterEntity inviteCharacterEntity, BasePlayerCharacterEntity acceptCharacterEntity)
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
            if (guild.CountMember() == gameInstance.maxGuildMember)
            {
                // TODO: May warn that it's exceeds limit max guild member
                return;
            }
            await Database.SetCharacterGuild(acceptCharacterEntity.Id, guildId);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberAdd(guildId, acceptCharacterEntity.Id, acceptCharacterEntity.CharacterName, acceptCharacterEntity.DataId, acceptCharacterEntity.Level);
        }

        public override async void KickFromGuild(BasePlayerCharacterEntity playerCharacterEntity, string characterId)
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
            await Database.SetCharacterGuild(characterId, 0);
            if (ChatNetworkManager.IsClientConnected)
                ChatNetworkManager.UpdateGuildMemberRemove(guildId, characterId);
        }

        public override async void LeaveGuild(BasePlayerCharacterEntity playerCharacterEntity)
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
                var tasks = new List<Task>();
                foreach (var memberId in guild.GetMemberIds())
                {
                    tasks.Add(Database.SetCharacterGuild(memberId, 0));
                    if (ChatNetworkManager.IsClientConnected)
                        ChatNetworkManager.UpdateGuildMemberRemove(guildId, memberId);
                }
                tasks.Add(Database.DeleteGuild(guildId));
                await Task.WhenAll(tasks);
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildTerminate(guildId);
            }
            else
            {
                await Database.SetCharacterGuild(playerCharacterEntity.Id, 0);
                if (ChatNetworkManager.IsClientConnected)
                    ChatNetworkManager.UpdateGuildMemberRemove(guildId, playerCharacterEntity.Id);
            }
        }
    }
}
