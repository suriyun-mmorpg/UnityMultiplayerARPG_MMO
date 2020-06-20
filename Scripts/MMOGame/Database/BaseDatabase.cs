using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public abstract partial class BaseDatabase : MonoBehaviour
    {
        public const byte AUTH_TYPE_NORMAL = 1;

        public virtual void Initialize() { }
        public virtual void Destroy() { }

        public abstract string ValidateUserLogin(string username, string password);
        public abstract bool ValidateAccessToken(string userId, string accessToken);
        public abstract byte GetUserLevel(string userId);
        public abstract int GetGold(string userId);
        public abstract void UpdateGold(string userId, int amount);
        public abstract int GetCash(string userId);
        public abstract void UpdateCash(string userId, int amount);
        public abstract void UpdateAccessToken(string userId, string accessToken);
        public abstract void CreateUserLogin(string username, string password);
        public abstract long FindUsername(string username);

        public abstract void CreateCharacter(string userId, IPlayerCharacterData characterData);
        public abstract PlayerCharacterData ReadCharacter(
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
        public abstract List<PlayerCharacterData> ReadCharacters(string userId);
        public abstract void UpdateCharacter(IPlayerCharacterData character);
        public abstract void DeleteCharacter(string userId, string id);
        public abstract long FindCharacterName(string characterName);
        public abstract List<SocialCharacterData> FindCharacters(string characterName);
        public abstract void CreateFriend(string id1, string id2);
        public abstract void DeleteFriend(string id1, string id2);
        public abstract List<SocialCharacterData> ReadFriends(string id1);

        public abstract void CreateBuilding(string mapName, IBuildingSaveData saveData);
        public abstract List<BuildingSaveData> ReadBuildings(string mapName);
        public abstract void UpdateBuilding(string mapName, IBuildingSaveData building);
        public abstract void DeleteBuilding(string mapName, string id);

        public abstract int CreateParty(bool shareExp, bool shareItem, string leaderId);
        public abstract PartyData ReadParty(int id);
        public abstract void UpdatePartyLeader(int id, string leaderId);
        public abstract void UpdateParty(int id, bool shareExp, bool shareItem);
        public abstract void DeleteParty(int id);
        public abstract void UpdateCharacterParty(string characterId, int partyId);

        public abstract int CreateGuild(string guildName, string leaderId);
        public abstract GuildData ReadGuild(int id, GuildRoleData[] defaultGuildRoles);
        public abstract void UpdateGuildLevel(int id, short level, int exp, short skillPoint);
        public abstract void UpdateGuildLeader(int id, string leaderId);
        public abstract void UpdateGuildMessage(int id, string guildMessage);
        public abstract void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage);
        public abstract void UpdateGuildMemberRole(string characterId, byte guildRole);
        public abstract void UpdateGuildSkillLevel(int id, int dataId, short skillLevel, short skillPoint);
        public abstract void DeleteGuild(int id);
        public abstract long FindGuildName(string guildName);
        public abstract void UpdateCharacterGuild(string characterId, int guildId, byte guildRole);
        public abstract int GetGuildGold(int guildId);
        public abstract void UpdateGuildGold(int guildId, int gold);

        public abstract void UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> storageCharacterItems);
        public abstract List<CharacterItem> ReadStorageItems(StorageType storageType, string storageOwnerId);
    }
}
