using System;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public abstract class DatabaseJob : ThreadedJob
    {
        protected BaseDatabase database;
        protected Action onFinished;
        public DatabaseJob(BaseDatabase database, Action onFinished)
        {
            this.database = database;
            this.onFinished = onFinished;
        }

        protected override void OnFinished()
        {
            if (onFinished != null)
                onFinished.Invoke();
        }
    }

    public abstract class DatabaseJob<T> : ThreadedJob
    {
        protected BaseDatabase database;
        public T result { get; protected set; }
        protected Action<T> onFinished;
        public DatabaseJob(BaseDatabase database, Action<T> onFinished)
        {
            this.database = database;
            this.onFinished = onFinished;
        }

        protected override void OnFinished()
        {
            if (onFinished != null)
                onFinished.Invoke(result);
        }
    }

    #region Authentication
    public class ValidateAccessTokenJob : DatabaseJob<bool>
    {
        private string userId;
        private string accessToken;
        public ValidateAccessTokenJob(BaseDatabase database, string userId, string accessToken, Action<bool> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.accessToken = accessToken;
        }

        protected override void ThreadFunction()
        {
            result = database.ValidateAccessToken(userId, accessToken);
        }
    }

    public class UpdateAccessTokenJob : DatabaseJob
    {
        private string userId;
        private string accessToken;
        public UpdateAccessTokenJob(BaseDatabase database, string userId, string accessToken, Action onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.accessToken = accessToken;
        }

        protected override void ThreadFunction()
        {
            database.UpdateAccessToken(userId, accessToken);
        }
    }

    public class ValidateUserLoginJob : DatabaseJob<string>
    {
        private string username;
        private string password;
        public ValidateUserLoginJob(BaseDatabase database, string username, string password, Action<string> onFinished = null) : base(database, onFinished)
        {
            this.username = username;
            this.password = password;
        }

        protected override void ThreadFunction()
        {
            result = database.ValidateUserLogin(username, password);
        }
    }

    public class FindUsernameJob : DatabaseJob<long>
    {
        private string username;
        public FindUsernameJob(BaseDatabase database, string username, Action<long> onFinished = null) : base(database, onFinished)
        {
            this.username = username;
        }

        protected override void ThreadFunction()
        {
            result = database.FindUsername(username);
        }
    }

    public class CreateUserLoginJob : DatabaseJob
    {
        private string username;
        private string password;
        public CreateUserLoginJob(BaseDatabase database, string username, string password, Action onFinished = null) : base(database, onFinished)
        {
            this.username = username;
            this.password = password;
        }

        protected override void ThreadFunction()
        {
            database.CreateUserLogin(username, password);
        }
    }

    public class FacebookLoginJob : DatabaseJob<string>
    {
        private string id;
        private string email;
        public FacebookLoginJob(BaseDatabase database, string id, string email, Action<string> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.email = email;
        }

        protected override void ThreadFunction()
        {
            result = database.FacebookLogin(id, email);
        }
    }

    public class GooglePlayLoginJob : DatabaseJob<string>
    {
        private string id;
        private string email;
        public GooglePlayLoginJob(BaseDatabase database, string id, string email, Action<string> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.email = email;
        }

        protected override void ThreadFunction()
        {
            result = database.GooglePlayLogin(id, email);
        }
    }

    public class GetUserLevelJob : DatabaseJob<byte>
    {
        private string userId;
        public GetUserLevelJob(BaseDatabase database, string userId, Action<byte> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
        }

        protected override void ThreadFunction()
        {
            result = database.GetUserLevel(userId);
        }
    }
    #endregion

    #region Party
    public class CreatePartyJob : DatabaseJob<int>
    {
        private bool shareExp;
        private bool shareItem;
        private string leaderId;
        public CreatePartyJob(BaseDatabase database, bool shareExp, bool shareItem, string leaderId, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.shareExp = shareExp;
            this.shareItem = shareItem;
            this.leaderId = leaderId;
        }

        protected override void ThreadFunction()
        {
            result = database.CreateParty(shareExp, shareItem, leaderId);
        }
    }

    public class ReadPartyJob : DatabaseJob<PartyData>
    {
        private int id;
        public ReadPartyJob(BaseDatabase database, int id, Action<PartyData> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadParty(id);
        }
    }

    public class UpdatePartyLeaderJob : DatabaseJob
    {
        private int id;
        private string characterId;
        public UpdatePartyLeaderJob(BaseDatabase database, int id, string characterId, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.characterId = characterId;
        }

        protected override void ThreadFunction()
        {
            database.UpdatePartyLeader(id, characterId);
        }
    }

    public class UpdatePartyJob : DatabaseJob
    {
        private int id;
        private bool shareExp;
        private bool shareItem;
        public UpdatePartyJob(BaseDatabase database, int id, bool shareExp, bool shareItem, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.shareExp = shareExp;
            this.shareItem = shareItem;
        }

        protected override void ThreadFunction()
        {
            database.UpdateParty(id, shareExp, shareItem);
        }
    }

    public class DeletePartyJob : DatabaseJob
    {
        private int id;
        public DeletePartyJob(BaseDatabase database, int id, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            database.DeleteParty(id);
        }
    }

    public class SetCharacterPartyJob : DatabaseJob
    {
        private string characterId;
        private int id;
        public SetCharacterPartyJob(BaseDatabase database, string characterId, int id, Action onFinished = null) : base(database, onFinished)
        {
            this.characterId = characterId;
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            database.UpdateCharacterParty(characterId, id);
        }
    }
    #endregion

    #region Guild
    public class CreateGuildJob : DatabaseJob<int>
    {
        private string guildName;
        private string leaderId;
        public CreateGuildJob(BaseDatabase database, string guildName, string leaderId, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.guildName = guildName;
            this.leaderId = leaderId;
        }

        protected override void ThreadFunction()
        {
            result = database.CreateGuild(guildName, leaderId);
        }
    }

    public class ReadGuildJob : DatabaseJob<GuildData>
    {
        private int id;
        private GuildRoleData[] defaultGuildRoles;
        public ReadGuildJob(BaseDatabase database, int id, GuildRoleData[] defaultGuildRoles, Action<GuildData> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.defaultGuildRoles = defaultGuildRoles;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadGuild(id, defaultGuildRoles);
        }
    }

    public class IncreaseGuildExpJob : DatabaseJob<bool>
    {
        private int id;
        private int increaseExp;
        private int[] expTree;
        private short tempResultLevel;
        public short resultLevel { get { return tempResultLevel; } }
        private int tempResultExp;
        public int resultExp { get { return tempResultExp; } }
        private short tempResultSkillPoint;
        public short resultSkillPoint { get { return tempResultSkillPoint; } }
        public IncreaseGuildExpJob(BaseDatabase database, int id, int increaseExp, int[] expTree, Action<bool> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.increaseExp = increaseExp;
            this.expTree = expTree;
        }

        protected override void ThreadFunction()
        {
            result = database.IncreaseGuildExp(id, increaseExp, expTree, out tempResultLevel, out tempResultExp, out tempResultSkillPoint);
        }
    }

    public class UpdateGuildLeaderJob : DatabaseJob
    {
        private int id;
        private string characterId;
        public UpdateGuildLeaderJob(BaseDatabase database, int id, string characterId, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.characterId = characterId;
        }

        protected override void ThreadFunction()
        {
            database.UpdateGuildLeader(id, characterId);
        }
    }

    public class UpdateGuildMessageJob : DatabaseJob
    {
        private int id;
        private string message;
        public UpdateGuildMessageJob(BaseDatabase database, int id, string message, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.message = message;
        }

        protected override void ThreadFunction()
        {
            database.UpdateGuildMessage(id, message);
        }
    }

    public class UpdateGuildRoleJob : DatabaseJob
    {
        private int id;
        private byte guildRole;
        private string name;
        private bool canInvite;
        private bool canKick;
        private byte shareExpPercentage;
        public UpdateGuildRoleJob(BaseDatabase database, int id, byte guildRole, string name, bool canInvite, bool canKick, byte shareExpPercentage, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.guildRole = guildRole;
            this.name = name;
            this.canInvite = canInvite;
            this.canKick = canKick;
            this.shareExpPercentage = shareExpPercentage;
        }

        protected override void ThreadFunction()
        {
            database.UpdateGuildRole(id, guildRole, name, canInvite, canKick, shareExpPercentage);
        }
    }

    public class UpdateGuildMemberRoleJob : DatabaseJob
    {
        private string characterId;
        private byte guildRole;
        public UpdateGuildMemberRoleJob(BaseDatabase database, string characterId, byte guildRole, Action onFinished = null) : base(database, onFinished)
        {
            this.characterId = characterId;
            this.guildRole = guildRole;
        }

        protected override void ThreadFunction()
        {
            database.UpdateGuildMemberRole(characterId, guildRole);
        }
    }

    public class UpdateGuildSkillLevelJob : DatabaseJob
    {
        private int id;
        private int dataId;
        private short level;
        private short skillPoint;
        public UpdateGuildSkillLevelJob(BaseDatabase database, int id, int dataId, short level, short skillPoint, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.dataId = dataId;
            this.level = level;
            this.skillPoint = skillPoint;
        }

        protected override void ThreadFunction()
        {
            database.UpdateGuildSkillLevel(id, dataId, level, skillPoint);
        }
    }

    public class DeleteGuildJob : DatabaseJob
    {
        private int id;
        public DeleteGuildJob(BaseDatabase database, int id, Action onFinished = null) : base(database, onFinished)
        {
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            database.DeleteGuild(id);
        }
    }

    public class FindGuildNameJob : DatabaseJob<long>
    {
        private string guildName;
        public FindGuildNameJob(BaseDatabase database, string guildName, Action<long> onFinished = null) : base(database, onFinished)
        {
            this.guildName = guildName;
        }

        protected override void ThreadFunction()
        {
            result = database.FindGuildName(guildName);
        }
    }

    public class SetCharacterGuildJob : DatabaseJob
    {
        private string characterId;
        private int id;
        private byte guildRole;
        public SetCharacterGuildJob(BaseDatabase database, string characterId, int id, byte guildRole, Action onFinished = null) : base(database, onFinished)
        {
            this.characterId = characterId;
            this.id = id;
            this.guildRole = guildRole;
        }

        protected override void ThreadFunction()
        {
            database.UpdateCharacterGuild(characterId, id, guildRole);
        }
    }

    public class GetGuildGoldJob : DatabaseJob<int>
    {
        private int guildId;
        public GetGuildGoldJob(BaseDatabase database, int guildId, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.guildId = guildId;
        }

        protected override void ThreadFunction()
        {
            result = database.GetGuildGold(guildId);
        }
    }

    public class DecreaseGuildGoldJob : DatabaseJob<int>
    {
        private int guildId;
        private int amount;
        public DecreaseGuildGoldJob(BaseDatabase database, int guildId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.guildId = guildId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.DecreaseGuildGold(guildId, amount);
        }
    }

    public class IncreaseGuildGoldJob : DatabaseJob<int>
    {
        private int guildId;
        private int amount;
        public IncreaseGuildGoldJob(BaseDatabase database, int guildId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.guildId = guildId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.IncreaseGuildGold(guildId, amount);
        }
    }
    #endregion

    #region User Gold
    public class GetGoldJob : DatabaseJob<int>
    {
        private string userId;
        public GetGoldJob(BaseDatabase database, string userId, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
        }

        protected override void ThreadFunction()
        {
            result = database.GetGold(userId);
        }
    }

    public class DecreaseGoldJob : DatabaseJob<int>
    {
        private string userId;
        private int amount;
        public DecreaseGoldJob(BaseDatabase database, string userId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.DecreaseGold(userId, amount);
        }
    }

    public class IncreaseGoldJob : DatabaseJob<int>
    {
        private string userId;
        private int amount;
        public IncreaseGoldJob(BaseDatabase database, string userId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.IncreaseGold(userId, amount);
        }
    }
    #endregion

    #region User Cash
    public class GetCashJob : DatabaseJob<int>
    {
        private string userId;
        public GetCashJob(BaseDatabase database, string userId, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
        }

        protected override void ThreadFunction()
        {
            result = database.GetCash(userId);
        }
    }

    public class DecreaseCashJob : DatabaseJob<int>
    {
        private string userId;
        private int amount;
        public DecreaseCashJob(BaseDatabase database, string userId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.DecreaseCash(userId, amount);
        }
    }

    public class IncreaseCashJob : DatabaseJob<int>
    {
        private string userId;
        private int amount;
        public IncreaseCashJob(BaseDatabase database, string userId, int amount, Action<int> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.amount = amount;
        }

        protected override void ThreadFunction()
        {
            result = database.IncreaseCash(userId, amount);
        }
    }
    #endregion

    #region Building
    public class CreateBuildingJob : DatabaseJob
    {
        private string sceneName;
        private IBuildingSaveData saveData;
        public CreateBuildingJob(BaseDatabase database, string sceneName, IBuildingSaveData saveData, Action onFinished = null) : base(database, onFinished)
        {
            this.sceneName = sceneName;
            this.saveData = saveData;
        }

        protected override void ThreadFunction()
        {
            database.CreateBuilding(sceneName, saveData);
        }
    }

    public class ReadBuildingsJob : DatabaseJob<List<BuildingSaveData>>
    {
        private string sceneName;
        public ReadBuildingsJob(BaseDatabase database, string sceneName, Action<List<BuildingSaveData>> onFinished = null) : base(database, onFinished)
        {
            this.sceneName = sceneName;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadBuildings(sceneName);
        }
    }

    public class UpdateBuildingJob : DatabaseJob
    {
        private string sceneName;
        private IBuildingSaveData saveData;
        public UpdateBuildingJob(BaseDatabase database, string sceneName, IBuildingSaveData saveData, Action onFinished = null) : base(database, onFinished)
        {
            this.sceneName = sceneName;
            this.saveData = saveData;
        }

        protected override void ThreadFunction()
        {
            database.UpdateBuilding(sceneName, saveData);
        }
    }

    public class DeleteBuildingJob : DatabaseJob
    {
        private string sceneName;
        private string id;
        public DeleteBuildingJob(BaseDatabase database, string sceneName, string id, Action onFinished = null) : base(database, onFinished)
        {
            this.sceneName = sceneName;
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            database.DeleteBuilding(sceneName, id);
        }
    }
    #endregion

    #region Character
    public class CreateCharacterJob : DatabaseJob
    {
        private string userId;
        private IPlayerCharacterData playerCharacterData;
        public CreateCharacterJob(BaseDatabase database, string userId, IPlayerCharacterData playerCharacterData, Action onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.playerCharacterData = playerCharacterData;
        }

        protected override void ThreadFunction()
        {
            database.CreateCharacter(userId, playerCharacterData);
        }
    }

    public class ReadCharacterJob : DatabaseJob<PlayerCharacterData>
    {
        private string userId;
        private string id;
        private bool withEquipWeapons = true;
        private bool withAttributes = true;
        private bool withSkills = true;
        private bool withSkillUsages = true;
        private bool withBuffs = true;
        private bool withEquipItems = true;
        private bool withNonEquipItems = true;
        private bool withHotkeys = true;
        private bool withQuests = true;
        public ReadCharacterJob(BaseDatabase database,
            string userId,
            string id,
            bool withEquipWeapons = true,
            bool withAttributes = true,
            bool withSkills = true,
            bool withSkillUsages = true,
            bool withBuffs = true,
            bool withEquipItems = true,
            bool withNonEquipItems = true,
            bool withHotkeys = true,
            bool withQuests = true,
            Action<PlayerCharacterData> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.id = id;
            this.withEquipWeapons = withEquipWeapons;
            this.withAttributes = withAttributes;
            this.withSkills = withSkills;
            this.withSkillUsages = withSkillUsages;
            this.withBuffs = withBuffs;
            this.withEquipItems = withEquipItems;
            this.withNonEquipItems = withNonEquipItems;
            this.withHotkeys = withHotkeys;
            this.withQuests = withQuests;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadCharacter(
                userId,
                id,
                withEquipWeapons,
                withAttributes,
                withSkills,
                withSkillUsages,
                withBuffs,
                withEquipItems,
                withNonEquipItems,
                withHotkeys,
                withQuests);
        }
    }

    public class ReadCharactersJob : DatabaseJob<List<PlayerCharacterData>>
    {
        private string userId;
        public ReadCharactersJob(BaseDatabase database, string userId, Action<List<PlayerCharacterData>> onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadCharacters(userId);
        }
    }

    public class UpdateCharacterJob : DatabaseJob
    {
        private IPlayerCharacterData playerCharacterData;
        public UpdateCharacterJob(BaseDatabase database, IPlayerCharacterData playerCharacterData, Action onFinished = null) : base(database, onFinished)
        {
            this.playerCharacterData = playerCharacterData;
        }

        protected override void ThreadFunction()
        {
            database.UpdateCharacter(playerCharacterData);
        }
    }

    public class DeleteCharactersJob : DatabaseJob
    {
        private string userId;
        private string id;
        public DeleteCharactersJob(BaseDatabase database, string userId, string id, Action onFinished = null) : base(database, onFinished)
        {
            this.userId = userId;
            this.id = id;
        }

        protected override void ThreadFunction()
        {
            database.DeleteCharacter(userId, id);
        }
    }

    public class FindCharacterNameJob : DatabaseJob<long>
    {
        private string characterName;
        public FindCharacterNameJob(BaseDatabase database, string characterName, Action<long> onFinished = null) : base(database, onFinished)
        {
            this.characterName = characterName;
        }

        protected override void ThreadFunction()
        {
            result = database.FindCharacterName(characterName);
        }
    }

    public class FindCharactersJob : DatabaseJob<List<SocialCharacterData>>
    {
        private string characterName;
        public FindCharactersJob(BaseDatabase database, string characterName, Action<List<SocialCharacterData>> onFinished = null) : base(database, onFinished)
        {
            this.characterName = characterName;
        }

        protected override void ThreadFunction()
        {
            result = database.FindCharacters(characterName);
        }
    }

    public class CreateFriendJob : DatabaseJob
    {
        private string id1;
        private string id2;
        public CreateFriendJob(BaseDatabase database, string id1, string id2, Action onFinished = null) : base(database, onFinished)
        {
            this.id1 = id1;
            this.id2 = id2;
        }

        protected override void ThreadFunction()
        {
            database.CreateFriend(id1, id2);
        }
    }

    public class DeleteFriendJob : DatabaseJob
    {
        private string id1;
        private string id2;
        public DeleteFriendJob(BaseDatabase database, string id1, string id2, Action onFinished = null) : base(database, onFinished)
        {
            this.id1 = id1;
            this.id2 = id2;
        }

        protected override void ThreadFunction()
        {
            database.DeleteFriend(id1, id2);
        }
    }

    public class ReadFriendsJob : DatabaseJob<List<SocialCharacterData>>
    {
        private string id1;
        public ReadFriendsJob(BaseDatabase database, string id1, Action<List<SocialCharacterData>> onFinished = null) : base(database, onFinished)
        {
            this.id1 = id1;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadFriends(id1);
        }
    }
    #endregion

    #region Storage Items
    public class ReadStorageItemsJob : DatabaseJob<List<CharacterItem>>
    {
        private StorageType storageType;
        private string storageOwnerId;
        public ReadStorageItemsJob(BaseDatabase database, StorageType storageType, string storageOwnerId, Action<List<CharacterItem>> onFinished = null) : base(database, onFinished)
        {
            this.storageType = storageType;
            this.storageOwnerId = storageOwnerId;
        }

        protected override void ThreadFunction()
        {
            result = database.ReadStorageItems(storageType, storageOwnerId);
        }
    }

    public class UpdateStorageItemsJob : DatabaseJob
    {
        private StorageType storageType;
        private string storageOwnerId;
        private IList<CharacterItem> characterItems;
        public UpdateStorageItemsJob(BaseDatabase database, StorageType storageType, string storageOwnerId, IList<CharacterItem> characterItems, Action onFinished = null) : base(database, onFinished)
        {
            this.storageType = storageType;
            this.storageOwnerId = storageOwnerId;
            this.characterItems = characterItems;
        }

        protected override void ThreadFunction()
        {
            database.UpdateStorageItems(storageType, storageOwnerId, characterItems);
        }
    }
    #endregion
}