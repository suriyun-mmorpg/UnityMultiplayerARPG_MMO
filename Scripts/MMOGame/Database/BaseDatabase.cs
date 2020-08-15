using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public abstract partial class BaseDatabase : MonoBehaviour
    {
        public const byte AUTH_TYPE_NORMAL = 1;

        public virtual void Initialize() { }
        public virtual void Destroy() { }

        public abstract Task<string> ValidateUserLogin(string username, string password);
        public abstract Task<bool> ValidateAccessToken(string userId, string accessToken);
        public abstract Task<byte> GetUserLevel(string userId);
        public abstract Task<int> GetGold(string userId);
        public abstract Task UpdateGold(string userId, int amount);
        public abstract Task<int> GetCash(string userId);
        public abstract Task UpdateCash(string userId, int amount);
        public abstract Task UpdateAccessToken(string userId, string accessToken);
        public abstract Task CreateUserLogin(string username, string password);
        public abstract Task<long> FindUsername(string username);

        public abstract Task CreateCharacter(string userId, IPlayerCharacterData characterData);
        public abstract Task<PlayerCharacterData> ReadCharacter(
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withSummons = true,
            bool withHotkeys = true,
            bool withQuests = true);
        public abstract Task<List<PlayerCharacterData>> ReadCharacters(string userId);
        public abstract Task UpdateCharacter(IPlayerCharacterData character);
        public abstract Task DeleteCharacter(string userId, string id);
        public abstract Task<long> FindCharacterName(string characterName);
        public abstract Task<List<SocialCharacterData>> FindCharacters(string characterName);
        public abstract Task CreateFriend(string id1, string id2);
        public abstract Task DeleteFriend(string id1, string id2);
        public abstract Task<List<SocialCharacterData>> ReadFriends(string id1);

        public abstract Task CreateBuilding(string mapName, IBuildingSaveData saveData);
        public abstract Task<List<BuildingSaveData>> ReadBuildings(string mapName);
        public abstract Task UpdateBuilding(string mapName, IBuildingSaveData building);
        public abstract Task DeleteBuilding(string mapName, string id);

        public abstract Task<int> CreateParty(bool shareExp, bool shareItem, string leaderId);
        public abstract Task<PartyData> ReadParty(int id);
        public abstract Task UpdatePartyLeader(int id, string leaderId);
        public abstract Task UpdateParty(int id, bool shareExp, bool shareItem);
        public abstract Task DeleteParty(int id);
        public abstract Task UpdateCharacterParty(string characterId, int partyId);

        public abstract Task<int> CreateGuild(string guildName, string leaderId);
        public abstract Task<GuildData> ReadGuild(int id, GuildRoleData[] defaultGuildRoles);
        public abstract Task UpdateGuildLevel(int id, short level, int exp, short skillPoint);
        public abstract Task UpdateGuildLeader(int id, string leaderId);
        public abstract Task UpdateGuildMessage(int id, string guildMessage);
        public abstract Task UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage);
        public abstract Task UpdateGuildMemberRole(string characterId, byte guildRole);
        public abstract Task UpdateGuildSkillLevel(int id, int dataId, short skillLevel, short skillPoint);
        public abstract Task DeleteGuild(int id);
        public abstract Task<long> FindGuildName(string guildName);
        public abstract Task UpdateCharacterGuild(string characterId, int guildId, byte guildRole);
        public abstract Task<int> GetGuildGold(int guildId);
        public abstract Task UpdateGuildGold(int guildId, int gold);

        public abstract Task UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> storageCharacterItems);
        public abstract Task<List<CharacterItem>> ReadStorageItems(StorageType storageType, string storageOwnerId);
    }
}
