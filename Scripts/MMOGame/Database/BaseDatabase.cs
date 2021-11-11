#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
#endif
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public abstract partial class BaseDatabase : MonoBehaviour
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public const byte AUTH_TYPE_NORMAL = 1;

        public virtual void Initialize() { }
        public virtual void Destroy() { }

        public abstract UniTask<string> ValidateUserLogin(string username, string password);
        public abstract UniTask<bool> ValidateAccessToken(string userId, string accessToken);
        public abstract UniTask<bool> ValidateEmailVerification(string userId);
        public abstract UniTask<long> FindEmail(string email);
        public abstract UniTask<byte> GetUserLevel(string userId);
        public abstract UniTask<int> GetGold(string userId);
        public abstract UniTask UpdateGold(string userId, int amount);
        public abstract UniTask<int> GetCash(string userId);
        public abstract UniTask UpdateCash(string userId, int amount);
        public abstract UniTask UpdateAccessToken(string userId, string accessToken);
        public abstract UniTask CreateUserLogin(string username, string password, string email);
        public abstract UniTask<long> FindUsername(string username);
        public abstract UniTask<long> GetUserUnbanTime(string userId);
        public abstract UniTask SetUserUnbanTimeByCharacterName(string characterName, long unbanTime);
        public abstract UniTask SetCharacterUnmuteTimeByName(string characterName, long unmuteTime);

        public abstract UniTask CreateCharacter(string userId, IPlayerCharacterData characterData);
        public abstract UniTask<PlayerCharacterData> ReadCharacter(
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
        public abstract UniTask<List<PlayerCharacterData>> ReadCharacters(string userId);
        public abstract UniTask UpdateCharacter(IPlayerCharacterData character);
        public abstract UniTask DeleteCharacter(string userId, string id);
        public abstract UniTask<List<CharacterBuff>> GetSummonBuffs(string characterId);
        public abstract UniTask SetSummonBuffs(string characterId, List<CharacterBuff> summonBuffs);
        public abstract UniTask<long> FindCharacterName(string characterName);
        public abstract UniTask<List<SocialCharacterData>> FindCharacters(string characterName);
        public abstract UniTask CreateFriend(string id1, string id2);
        public abstract UniTask DeleteFriend(string id1, string id2);
        public abstract UniTask<List<SocialCharacterData>> ReadFriends(string id1);
        public abstract UniTask<string> GetIdByCharacterName(string characterName);
        public abstract UniTask<string> GetUserIdByCharacterName(string characterName);

        public abstract UniTask CreateBuilding(string mapName, IBuildingSaveData saveData);
        public abstract UniTask<List<BuildingSaveData>> ReadBuildings(string mapName);
        public abstract UniTask UpdateBuilding(string mapName, IBuildingSaveData building);
        public abstract UniTask DeleteBuilding(string mapName, string id);

        public abstract UniTask<int> CreateParty(bool shareExp, bool shareItem, string leaderId);
        public abstract UniTask<PartyData> ReadParty(int id);
        public abstract UniTask UpdatePartyLeader(int id, string leaderId);
        public abstract UniTask UpdateParty(int id, bool shareExp, bool shareItem);
        public abstract UniTask DeleteParty(int id);
        public abstract UniTask UpdateCharacterParty(string characterId, int partyId);

        public abstract UniTask<int> CreateGuild(string guildName, string leaderId);
        public abstract UniTask<GuildData> ReadGuild(int id, GuildRoleData[] defaultGuildRoles);
        public abstract UniTask UpdateGuildLevel(int id, short level, int exp, short skillPoint);
        public abstract UniTask UpdateGuildLeader(int id, string leaderId);
        public abstract UniTask UpdateGuildMessage(int id, string guildMessage);
        public abstract UniTask UpdateGuildMessage2(int id, string guildMessage);
        public abstract UniTask UpdateGuildScore(int id, int score);
        public abstract UniTask UpdateGuildOptions(int id, string options);
        public abstract UniTask UpdateGuildAutoAcceptRequests(int id, bool autoAcceptRequests);
        public abstract UniTask UpdateGuildRank(int id, int rank);
        public abstract UniTask UpdateGuildRole(int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage);
        public abstract UniTask UpdateGuildMemberRole(string characterId, byte guildRole);
        public abstract UniTask UpdateGuildSkillLevel(int id, int dataId, short skillLevel, short skillPoint);
        public abstract UniTask DeleteGuild(int id);
        public abstract UniTask<long> FindGuildName(string guildName);
        public abstract UniTask UpdateCharacterGuild(string characterId, int guildId, byte guildRole);
        public abstract UniTask<int> GetGuildGold(int guildId);
        public abstract UniTask UpdateGuildGold(int guildId, int gold);

        public abstract UniTask UpdateStorageItems(StorageType storageType, string storageOwnerId, IList<CharacterItem> storageCharacterItems);
        public abstract UniTask<List<CharacterItem>> ReadStorageItems(StorageType storageType, string storageOwnerId);

        public abstract UniTask<List<MailListEntry>> MailList(string userId, bool onlyNewMails);
        public abstract UniTask<Mail> GetMail(string mailId, string userId);
        public abstract UniTask<long> UpdateReadMailState(string mailId, string userId);
        public abstract UniTask<long> UpdateClaimMailItemsState(string mailId, string userId);
        public abstract UniTask<long> UpdateDeleteMailState(string mailId, string userId);
        public abstract UniTask<int> CreateMail(Mail mail);
        public abstract UniTask<int> GetMailNotificationCount(string userId);
#endif
    }
}