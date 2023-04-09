using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerGuildHandlers : MonoBehaviour, IServerGuildHandlers
    {
        public const int GuildInvitationDuration = 10000;
        public static readonly ConcurrentDictionary<int, GuildData> Guilds = new ConcurrentDictionary<int, GuildData>();
        public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = new ConcurrentDictionary<long, GuildData>();
        public static readonly HashSet<string> GuildInvitations = new HashSet<string>();

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

        public int GuildsCount { get { return Guilds.Count; } }

        public bool TryGetGuild(int guildId, out GuildData guildData)
        {
            return Guilds.TryGetValue(guildId, out guildData);
        }

        public bool ContainsGuild(int guildId)
        {
            return Guilds.ContainsKey(guildId);
        }

        public void SetGuild(int guildId, GuildData guildData)
        {
            if (Guilds.ContainsKey(guildId))
                Guilds[guildId] = guildData;
            else
                Guilds.TryAdd(guildId, guildData);
        }

        public void RemoveGuild(int guildId)
        {
            Guilds.TryRemove(guildId, out _);
        }

        public bool HasGuildInvitation(int guildId, string characterId)
        {
            return GuildInvitations.Contains(GetGuildInvitationId(guildId, characterId));
        }

        public void AppendGuildInvitation(int guildId, string characterId)
        {
            RemoveGuildInvitation(guildId, characterId);
            GuildInvitations.Add(GetGuildInvitationId(guildId, characterId));
            DelayRemoveGuildInvitation(guildId, characterId).Forget();
        }

        public void RemoveGuildInvitation(int guildId, string characterId)
        {
            GuildInvitations.Remove(GetGuildInvitationId(guildId, characterId));
        }

        public void ClearGuild()
        {
            Guilds.Clear();
            UpdatingGuildMembers.Clear();
            GuildInvitations.Clear();
        }

        public async UniTaskVoid IncreaseGuildExp(IPlayerCharacterData playerCharacter, int exp)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            ValidateGuildRequestResult validateResult = this.CanIncreaseGuildExp(playerCharacter, exp);
            if (!validateResult.IsSuccess)
                return;
            DatabaseApiResult<GuildResp> resp = await DbServiceClient.IncreaseGuildExpAsync(new IncreaseGuildExpReq()
            {
                GuildId = validateResult.GuildId,
                Exp = exp,
            });
            if (!resp.IsSuccess)
                return;
            GuildData guild = resp.Response.GuildData;
            SetGuild(validateResult.GuildId, guild);
            // Broadcast via chat server
            if (ClusterClient.IsNetworkActive)
            {
                ClusterClient.SendSetGuildLevelExpSkillPoint(MMOMessageTypes.UpdateGuild, guild.id, guild.level, guild.exp, guild.skillPoint);
            }
            GameInstance.ServerGameMessageHandlers.SendSetGuildLevelExpSkillPointToMembers(guild);
#endif
        }

        private string GetGuildInvitationId(int guildId, string characterId)
        {
            return $"{guildId}_{characterId}";
        }

        private async UniTaskVoid DelayRemoveGuildInvitation(int partyId, string characterId)
        {
            await UniTask.Delay(GuildInvitationDuration);
            RemoveGuildInvitation(partyId, characterId);
        }

        public IEnumerable<GuildData> GetGuilds()
        {
            return Guilds.Values;
        }
    }
}
