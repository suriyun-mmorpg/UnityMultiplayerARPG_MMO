using System.Linq;
using UnityEngine;
using LiteNetLibManager;
using ConcurrentCollections;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-898)]
    public partial class DatabaseNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        [SerializeField]
        private BaseDatabase database = null;
        [SerializeField]
        private BaseDatabase[] databaseOptions = new BaseDatabase[0];
        [SerializeField]
        private bool disableCacheReading = false;

        public BaseDatabase Database
        {
            get
            {
                return database == null ? databaseOptions.FirstOrDefault() : database;
            }
        }

        public bool DisableCacheReading
        {
            get { return disableCacheReading; }
            set { disableCacheReading = value; }
        }

        public void SetDatabaseByOptionIndex(int index)
        {
            if (databaseOptions != null &&
                databaseOptions.Length > 0 &&
                index >= 0 &&
                index < databaseOptions.Length)
                database = databaseOptions[index];
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        // TODO: I'm going to make in-memory database without Redis for now
        // In the future it may implements Redis
        // It's going to get some data from all tables but not every records
        // Just some records that players were requested
        private ConcurrentHashSet<string> cachedUsernames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentHashSet<string> cachedEmails = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentHashSet<string> cachedCharacterNames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentHashSet<string> cachedGuildNames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, string> cachedUserAccessToken = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, int> cachedUserGold = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, int> cachedUserCash = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, PlayerCharacterData> cachedUserCharacter = new ConcurrentDictionary<string, PlayerCharacterData>();
        private ConcurrentDictionary<string, SocialCharacterData> cachedSocialCharacter = new ConcurrentDictionary<string, SocialCharacterData>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, BuildingSaveData>> cachedBuilding = new ConcurrentDictionary<string, ConcurrentDictionary<string, BuildingSaveData>>();
        private ConcurrentDictionary<int, PartyData> cachedParty = new ConcurrentDictionary<int, PartyData>();
        private ConcurrentDictionary<int, GuildData> cachedGuild = new ConcurrentDictionary<int, GuildData>();
        private ConcurrentDictionary<StorageId, List<CharacterItem>> cachedStorageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();
        private ConcurrentDictionary<StorageId, float> updatingStorages = new ConcurrentDictionary<StorageId, float>();
#endif

        protected override void Start()
        {
            useWebSocket = false;
            maxConnections = int.MaxValue;
            base.Start();
        }

        public override void OnStartServer()
        {
            base.OnStartServer();
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            Database.Initialize();
#endif
        }

        protected override void RegisterMessages()
        {
            base.RegisterMessages();
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            RegisterRequestToServer<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.RequestValidateUserLogin, ValidateUserLogin);
            RegisterRequestToServer<ValidateAccessTokenReq, ValidateAccessTokenResp>(DatabaseRequestTypes.RequestValidateAccessToken, ValidateAccessToken);
            RegisterRequestToServer<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.RequestGetUserLevel, GetUserLevel);
            RegisterRequestToServer<GetGoldReq, GoldResp>(DatabaseRequestTypes.RequestGetGold, GetGold);
            RegisterRequestToServer<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.RequestChangeGold, ChangeGold);
            RegisterRequestToServer<GetCashReq, CashResp>(DatabaseRequestTypes.RequestGetCash, GetCash);
            RegisterRequestToServer<ChangeCashReq, CashResp>(DatabaseRequestTypes.RequestChangeCash, ChangeCash);
            RegisterRequestToServer<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateAccessToken, UpdateAccessToken);
            RegisterRequestToServer<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateUserLogin, CreateUserLogin);
            RegisterRequestToServer<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.RequestFindUsername, FindUsername);
            RegisterRequestToServer<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestCreateCharacter, CreateCharacter);
            RegisterRequestToServer<ReadCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestReadCharacter, ReadCharacter);
            RegisterRequestToServer<ReadCharactersReq, CharactersResp>(DatabaseRequestTypes.RequestReadCharacters, ReadCharacters);
            RegisterRequestToServer<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestUpdateCharacter, UpdateCharacter);
            RegisterRequestToServer<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteCharacter, DeleteCharacter);
            RegisterRequestToServer<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.RequestFindCharacterName, FindCharacterName);
            RegisterRequestToServer<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.RequestFindCharacters, FindCharacters);
            RegisterRequestToServer<CreateFriendReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateFriend, CreateFriend);
            RegisterRequestToServer<DeleteFriendReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteFriend, DeleteFriend);
            RegisterRequestToServer<ReadFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.RequestReadFriends, ReadFriends);
            RegisterRequestToServer<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(DatabaseRequestTypes.RequestGetFriendRequestNotification, GetFriendRequestNotification);
            RegisterRequestToServer<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestCreateBuilding, CreateBuilding);
            RegisterRequestToServer<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestUpdateBuilding, UpdateBuilding);
            RegisterRequestToServer<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteBuilding, DeleteBuilding);
            RegisterRequestToServer<ReadBuildingsReq, BuildingsResp>(DatabaseRequestTypes.RequestReadBuildings, ReadBuildings);
            RegisterRequestToServer<CreatePartyReq, PartyResp>(DatabaseRequestTypes.RequestCreateParty, CreateParty);
            RegisterRequestToServer<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateParty, UpdateParty);
            RegisterRequestToServer<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.RequestUpdatePartyLeader, UpdatePartyLeader);
            RegisterRequestToServer<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteParty, DeleteParty);
            RegisterRequestToServer<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateCharacterParty, UpdateCharacterParty);
            RegisterRequestToServer<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterParty, ClearCharacterParty);
            RegisterRequestToServer<ReadPartyReq, PartyResp>(DatabaseRequestTypes.RequestReadParty, ReadParty);
            RegisterRequestToServer<CreateGuildReq, GuildResp>(DatabaseRequestTypes.RequestCreateGuild, CreateGuild);
            RegisterRequestToServer<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildLeader, UpdateGuildLeader);
            RegisterRequestToServer<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage, UpdateGuildMessage);
            RegisterRequestToServer<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage2, UpdateGuildMessage2);
            RegisterRequestToServer<UpdateGuildScoreReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildScore, UpdateGuildScore);
            RegisterRequestToServer<UpdateGuildOptionsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildOptions, UpdateGuildOptions);
            RegisterRequestToServer<UpdateGuildAutoAcceptRequestsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildAutoAcceptRequests, UpdateGuildAutoAcceptRequests);
            RegisterRequestToServer<UpdateGuildRankReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRank, UpdateGuildRank);
            RegisterRequestToServer<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRole, UpdateGuildRole);
            RegisterRequestToServer<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMemberRole, UpdateGuildMemberRole);
            RegisterRequestToServer<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteGuild, DeleteGuild);
            RegisterRequestToServer<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.RequestUpdateCharacterGuild, UpdateCharacterGuild);
            RegisterRequestToServer<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterGuild, ClearCharacterGuild);
            RegisterRequestToServer<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.RequestFindGuildName, FindGuildName);
            RegisterRequestToServer<ReadGuildReq, GuildResp>(DatabaseRequestTypes.RequestReadGuild, ReadGuild);
            RegisterRequestToServer<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.RequestIncreaseGuildExp, IncreaseGuildExp);
            RegisterRequestToServer<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.RequestAddGuildSkill, AddGuildSkill);
            RegisterRequestToServer<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestGetGuildGold, GetGuildGold);
            RegisterRequestToServer<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestChangeGuildGold, ChangeGuildGold);
            RegisterRequestToServer<ReadStorageItemsReq, ReadStorageItemsResp>(DatabaseRequestTypes.RequestReadStorageItems, ReadStorageItems);
            RegisterRequestToServer<UpdateStorageItemsReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateStorageItems, UpdateStorageItems);
            RegisterRequestToServer<MailListReq, MailListResp>(DatabaseRequestTypes.RequestMailList, MailList);
            RegisterRequestToServer<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.RequestUpdateReadMailState, UpdateReadMailState);
            RegisterRequestToServer<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.RequestUpdateClaimMailItemsState, UpdateClaimMailItemsState);
            RegisterRequestToServer<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.RequestUpdateDeleteMailState, UpdateDeleteMailState);
            RegisterRequestToServer<SendMailReq, SendMailResp>(DatabaseRequestTypes.RequestSendMail, SendMail);
            RegisterRequestToServer<GetMailReq, GetMailResp>(DatabaseRequestTypes.RequestGetMail, GetMail);
            RegisterRequestToServer<GetMailNotificationReq, GetMailNotificationResp>(DatabaseRequestTypes.RequestGetMailNotification, GetMailNotification);
            RegisterRequestToServer<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetIdByCharacterName, GetIdByCharacterName);
            RegisterRequestToServer<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetUserIdByCharacterName, GetUserIdByCharacterName);
            RegisterRequestToServer<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(DatabaseRequestTypes.RequestGetUserUnbanTime, GetUserUnbanTime);
            RegisterRequestToServer<SetUserUnbanTimeByCharacterNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetUserUnbanTimeByCharacterName, SetUserUnbanTimeByCharacterName);
            RegisterRequestToServer<SetCharacterUnmuteTimeByNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetCharacterUnmuteTimeByName, SetCharacterUnmuteTimeByName);
            RegisterRequestToServer<GetSummonBuffsReq, GetSummonBuffsResp>(DatabaseRequestTypes.RequestGetSummonBuffs, GetSummonBuffs);
            RegisterRequestToServer<SetSummonBuffsReq, EmptyMessage>(DatabaseRequestTypes.RequestSetSummonBuffs, SetSummonBuffs);
            RegisterRequestToServer<FindEmailReq, FindEmailResp>(DatabaseRequestTypes.RequestFindEmail, FindEmail);
            RegisterRequestToServer<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(DatabaseRequestTypes.RequestValidateEmailVerification, ValidateEmailVerification);
            RegisterRequestToServer<UpdateUserCountReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateUserCount, UpdateUserCount);
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }
    }
}