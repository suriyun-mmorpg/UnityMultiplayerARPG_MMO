#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
#endif
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public abstract partial class BaseDatabase : MonoBehaviour, IDatabaseManagerLogging
    {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public const byte AUTH_TYPE_NORMAL = 1;

        public virtual void Initialize() { }
        public virtual void Destroy() { }

        public abstract string ValidateUserLogin(string username, string password);
        public abstract bool ValidateAccessToken(string userId, string accessToken);
        public abstract bool ValidateEmailVerification(string userId);
        public abstract long FindEmail(string email);
        public abstract byte GetUserLevel(string userId);
        public abstract int GetGold(string userId);
        public abstract void UpdateGold(string userId, int amount);
        public abstract int GetCash(string userId);
        public abstract void UpdateCash(string userId, int amount);
        public abstract void UpdateAccessToken(string userId, string accessToken);
        public abstract void CreateUserLogin(string username, string password, string email);
        public abstract long FindUsername(string username);
        public abstract long GetUserUnbanTime(string userId);
        public abstract void SetUserUnbanTimeByCharacterName(string characterName, long unbanTime);
        public abstract void SetCharacterUnmuteTimeByName(string characterName, long unmuteTime);

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
            bool withQuests = true,
            bool withCurrencies = true);
        public abstract List<PlayerCharacterData> ReadCharacters(string userId);
        public abstract void UpdateCharacter(IPlayerCharacterData character);
        public abstract void DeleteCharacter(string userId, string id);
        public abstract List<CharacterBuff> GetSummonBuffs(string characterId);
        public abstract void SetSummonBuffs(string characterId, List<CharacterBuff> summonBuffs);
        public abstract long FindCharacterName(string characterName);
        public abstract List<SocialCharacterData> FindCharacters(string finderId, string characterName, int skip, int limit);
        public abstract void CreateFriend(string id1, string id2, byte state);
        public abstract void DeleteFriend(string id1, string id2);
        public abstract List<SocialCharacterData> ReadFriends(string id, bool readById2, byte state, int skip, int limit);
        public abstract int GetFriendRequestNotification(string characterId);
        public abstract string GetIdByCharacterName(string characterName);
        public abstract string GetUserIdByCharacterName(string characterName);

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
        public abstract void UpdateGuildLevel(int id, int level, int exp, int skillPoint);
        public abstract void UpdateGuildLeader(int id, string leaderId);
        public abstract void UpdateGuildMessage(int id, string guildMessage);
        public abstract void UpdateGuildMessage2(int id, string guildMessage);
        public abstract void UpdateGuildScore(int id, int score);
        public abstract void UpdateGuildOptions(int id, string options);
        public abstract void UpdateGuildAutoAcceptRequests(int id, bool autoAcceptRequests);
        public abstract void UpdateGuildRank(int id, int rank);
        public abstract void UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage);
        public abstract void UpdateGuildMemberRole(string characterId, byte guildRole);
        public abstract void UpdateGuildSkillLevel(int id, int dataId, int skillLevel, int skillPoint);
        public abstract void DeleteGuild(int id);
        public abstract long FindGuildName(string guildName);
        public abstract void UpdateCharacterGuild(string characterId, int guildId, byte guildRole);
        public abstract int GetGuildGold(int guildId);
        public abstract void UpdateGuildGold(int guildId, int gold);

        public abstract void UpdateStorageItems(StorageType storageType, string storageOwnerId, List<CharacterItem> storageCharacterItems);
        public abstract List<CharacterItem> ReadStorageItems(StorageType storageType, string storageOwnerId);

        public abstract List<MailListEntry> MailList(string userId, bool onlyNewMails);
        public abstract Mail GetMail(string mailId, string userId);
        public abstract long UpdateReadMailState(string mailId, string userId);
        public abstract long UpdateClaimMailItemsState(string mailId, string userId);
        public abstract long UpdateDeleteMailState(string mailId, string userId);
        public abstract int CreateMail(Mail mail);
        public abstract int GetMailNotification(string userId);

        public abstract void UpdateUserCount(int userCount);
#endif
    }
}