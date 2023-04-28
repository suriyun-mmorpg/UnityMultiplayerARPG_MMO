using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityRestClient;

namespace MultiplayerARPG.MMO
{
    public partial class RestDatabaseClient : RestClient, IDatabaseClient
    {
        [System.Serializable]
        public struct Config
        {
            public string dbApiUrl;
            public string dbSecretKey;
        }

        public string apiUrl = "http://localhost:5757/api/";
        public string secretKey = "secret";

        void Awake()
        {
            string configFolder = "./Config";
            string configFilePath = configFolder + "/serverConfig.json";
            Dictionary<string, object> jsonConfig = new Dictionary<string, object>();
            Logging.Log(nameof(RestDatabaseClient), "Reading config file from " + configFilePath);
            if (File.Exists(configFilePath))
            {
                // Read config file
                Logging.Log(nameof(RestDatabaseClient), "Found config file");
                string dataAsJson = File.ReadAllText(configFilePath);
                Config newConfig = JsonConvert.DeserializeObject<Config>(dataAsJson);
                if (newConfig.dbApiUrl != null)
                    apiUrl = newConfig.dbApiUrl;
                if (newConfig.dbSecretKey != null)
                    secretKey = newConfig.dbSecretKey;
            }
        }

        private async UniTask<DatabaseApiResult<TResp>> SendRequest<TReq, TResp>(TReq request, string url, string functionName)
        {
            var resp = await Post<TReq, TResp>(url, request, secretKey);
            if (resp.IsError())
            {
                Logging.LogError(nameof(RestDatabaseClient), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult<TResp>()
                {
                    IsError = true,
                    Error = resp.Error,
                    Response = resp.Content,
                };
            }
            return new DatabaseApiResult<TResp>()
            {
                Response = resp.Content,
            };
        }

        private async UniTask<DatabaseApiResult> SendRequest<TReq>(TReq request, string url, string functionName)
        {
            var resp = await Post(url, request, secretKey);
            if (resp.IsError())
            {
                Logging.LogError(nameof(RestDatabaseClient), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult()
                {
                    IsError = true,
                    Error = resp.Error,
                };
            }
            return new DatabaseApiResult();
        }

        public async UniTask<DatabaseApiResult<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            return await SendRequest<ValidateUserLoginReq, ValidateUserLoginResp>(request, GetUrl(apiUrl, DatabaseApiPath.ValidateUserLogin), nameof(ValidateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            return await SendRequest<ValidateAccessTokenReq, ValidateAccessTokenResp>(request, GetUrl(apiUrl, DatabaseApiPath.ValidateAccessToken), nameof(ValidateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            return await SendRequest<GetUserLevelReq, GetUserLevelResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetUserLevel), nameof(GetUserLevelAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            return await SendRequest<GetGoldReq, GoldResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetGold), nameof(GetGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            return await SendRequest<ChangeGoldReq, GoldResp>(request, GetUrl(apiUrl, DatabaseApiPath.ChangeGold), nameof(ChangeGoldAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> GetCashAsync(GetCashReq request)
        {
            return await SendRequest<GetCashReq, CashResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetCash), nameof(GetCashAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            return await SendRequest<ChangeCashReq, CashResp>(request, GetUrl(apiUrl, DatabaseApiPath.ChangeCash), nameof(ChangeCashAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.UpdateAccessToken), nameof(UpdateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.CreateUserLogin), nameof(CreateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            return await SendRequest<FindUsernameReq, FindUsernameResp>(request, GetUrl(apiUrl, DatabaseApiPath.FindUsername), nameof(FindUsernameAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            return await SendRequest<CreateCharacterReq, CharacterResp>(request, GetUrl(apiUrl, DatabaseApiPath.CreateCharacter), nameof(CreateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request)
        {
            return await SendRequest<ReadCharacterReq, CharacterResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadCharacter), nameof(ReadCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request)
        {
            return await SendRequest<ReadCharactersReq, CharactersResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadCharacters), nameof(ReadCharactersAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            return await SendRequest<UpdateCharacterReq, CharacterResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateCharacter), nameof(UpdateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.DeleteCharacter), nameof(DeleteCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, FindCharacterNameResp>(request, GetUrl(apiUrl, DatabaseApiPath.FindCharacterName), nameof(FindCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, SocialCharactersResp>(request, GetUrl(apiUrl, DatabaseApiPath.FindCharacters), nameof(FindCharactersAsync));
        }

        public async UniTask<DatabaseApiResult> CreateFriendAsync(CreateFriendReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.CreateFriend), nameof(CreateFriendAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteFriendAsync(DeleteFriendReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.DeleteFriend), nameof(DeleteFriendAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request)
        {
            return await SendRequest<ReadFriendsReq, SocialCharactersResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadFriends), nameof(ReadFriendsAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            return await SendRequest<CreateBuildingReq, BuildingResp>(request, GetUrl(apiUrl, DatabaseApiPath.CreateBuilding), nameof(CreateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            return await SendRequest<UpdateBuildingReq, BuildingResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateBuilding), nameof(UpdateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.DeleteBuilding), nameof(DeleteBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request)
        {
            return await SendRequest<ReadBuildingsReq, BuildingsResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadBuildings), nameof(ReadBuildingsAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            return await SendRequest<CreatePartyReq, PartyResp>(request, GetUrl(apiUrl, DatabaseApiPath.CreateParty), nameof(CreatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            return await SendRequest<UpdatePartyReq, PartyResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateParty), nameof(UpdatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            return await SendRequest<UpdatePartyLeaderReq, PartyResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdatePartyLeader), nameof(UpdatePartyLeaderAsync));
        }

        public async UniTask<DatabaseApiResult> DeletePartyAsync(DeletePartyReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.DeleteParty), nameof(DeletePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            return await SendRequest<UpdateCharacterPartyReq, PartyResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateCharacterParty), nameof(UpdateCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.ClearCharacterParty), nameof(ClearCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> ReadPartyAsync(ReadPartyReq request)
        {
            return await SendRequest<ReadPartyReq, PartyResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadParty), nameof(ReadPartyAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            return await SendRequest<CreateGuildReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.CreateGuild), nameof(CreateGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            return await SendRequest<UpdateGuildLeaderReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildLeader), nameof(UpdateGuildLeaderAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildMessage), nameof(UpdateGuildMessageAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildMessage2), nameof(UpdateGuildMessage2Async));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            return await SendRequest<UpdateGuildOptionsReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildOptions), nameof(UpdateGuildOptionsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            return await SendRequest<UpdateGuildAutoAcceptRequestsReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildAutoAcceptRequests), nameof(UpdateGuildAutoAcceptRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            return await SendRequest<UpdateGuildRoleReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildRole), nameof(UpdateGuildRoleAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            return await SendRequest<UpdateGuildMemberRoleReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateGuildMemberRole), nameof(UpdateGuildMemberRoleAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildAsync(DeleteGuildReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.DeleteGuild), nameof(DeleteGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            return await SendRequest<UpdateCharacterGuildReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateCharacterGuild), nameof(UpdateCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.ClearCharacterGuild), nameof(ClearCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, FindGuildNameResp>(request, GetUrl(apiUrl, DatabaseApiPath.FindGuildName), nameof(FindGuildNameAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> ReadGuildAsync(ReadGuildReq request)
        {
            return await SendRequest<ReadGuildReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadGuild), nameof(ReadGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            return await SendRequest<IncreaseGuildExpReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.IncreaseGuildExp), nameof(IncreaseGuildExpAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            return await SendRequest<AddGuildSkillReq, GuildResp>(request, GetUrl(apiUrl, DatabaseApiPath.AddGuildSkill), nameof(AddGuildSkillAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            return await SendRequest<GetGuildGoldReq, GuildGoldResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetGuildGold), nameof(GetGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            return await SendRequest<ChangeGuildGoldReq, GuildGoldResp>(request, GetUrl(apiUrl, DatabaseApiPath.ChangeGuildGold), nameof(ChangeGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request)
        {
            return await SendRequest<ReadStorageItemsReq, ReadStorageItemsResp>(request, GetUrl(apiUrl, DatabaseApiPath.ReadStorageItems), nameof(ReadStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateStorageItemsAsync(UpdateStorageItemsReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.UpdateStorageItems), nameof(UpdateStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult<MailListResp>> MailListAsync(MailListReq request)
        {
            return await SendRequest<MailListReq, MailListResp>(request, GetUrl(apiUrl, DatabaseApiPath.MailList), nameof(MailListAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            return await SendRequest<UpdateReadMailStateReq, UpdateReadMailStateResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateReadMailState), nameof(UpdateReadMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            return await SendRequest<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateClaimMailItemsState), nameof(UpdateClaimMailItemsStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            return await SendRequest<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(request, GetUrl(apiUrl, DatabaseApiPath.UpdateDeleteMailState), nameof(UpdateDeleteMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            return await SendRequest<SendMailReq, SendMailResp>(request, GetUrl(apiUrl, DatabaseApiPath.SendMail), nameof(SendMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            return await SendRequest<GetMailReq, GetMailResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetMail), nameof(GetMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            return await SendRequest<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetIdByCharacterName), nameof(GetIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            return await SendRequest<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetUserIdByCharacterName), nameof(GetUserIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request)
        {
            return await SendRequest<GetMailNotificationReq, GetMailNotificationResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetMailNotification), nameof(GetMailNotificationAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            return await SendRequest<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetUserUnbanTime), nameof(GetUserUnbanTimeAsync));
        }

        public async UniTask<DatabaseApiResult> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.SetUserUnbanTimeByCharacterName), nameof(SetUserUnbanTimeByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.SetCharacterUnmuteTimeByName), nameof(SetCharacterUnmuteTimeByNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            return await SendRequest<GetSummonBuffsReq, GetSummonBuffsResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetSummonBuffs), nameof(GetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult> SetSummonBuffsAsync(SetSummonBuffsReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.SetSummonBuffs), nameof(SetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            return await SendRequest<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(request, GetUrl(apiUrl, DatabaseApiPath.ValidateEmailVerification), nameof(ValidateEmailVerificationAsync));
        }

        public async UniTask<DatabaseApiResult<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            return await SendRequest<FindEmailReq, FindEmailResp>(request, GetUrl(apiUrl, DatabaseApiPath.FindEmail), nameof(FindEmailAsync));
        }

        public async UniTask<DatabaseApiResult<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request)
        {
            return await SendRequest<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(request, GetUrl(apiUrl, DatabaseApiPath.GetFriendRequestNotification), nameof(GetFriendRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateUserCount(UpdateUserCountReq request)
        {
            return await SendRequest(request, GetUrl(apiUrl, DatabaseApiPath.UpdateUserCount), nameof(UpdateUserCount));
        }
    }
}