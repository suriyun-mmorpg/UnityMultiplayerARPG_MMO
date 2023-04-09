using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager : IDatabaseClient
    {
        private async UniTask<DatabaseApiResult<TResp>> SendRequest<TReq, TResp>(TReq request, ushort requestType, string functionName)
            where TReq : INetSerializable, new()
            where TResp : INetSerializable, new()
        {
            var resp = await Client.SendRequestAsync<TReq, TResp>(requestType, request);
            if (!resp.IsSuccess)
            {
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult<TResp>()
                {
                    IsError = true,
                    Response = resp.Response,
                };
            }
            return new DatabaseApiResult<TResp>()
            {
                Response = resp.Response,
            };
        }

        private async UniTask<DatabaseApiResult> SendRequest<TReq>(TReq request, ushort requestType, string functionName)
            where TReq : INetSerializable, new()
        {
            var resp = await Client.SendRequestAsync<TReq, EmptyMessage>(requestType, request);
            if (!resp.IsSuccess)
            {
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {functionName} status: {resp.ResponseCode}");
                return new DatabaseApiResult()
                {
                    IsError = true,
                };
            }
            return new DatabaseApiResult();
        }

        public async UniTask<DatabaseApiResult<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            return await SendRequest<ValidateUserLoginReq, ValidateUserLoginResp>(request, DatabaseRequestTypes.RequestValidateUserLogin, nameof(ValidateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            return await SendRequest<ValidateAccessTokenReq, ValidateAccessTokenResp>(request, DatabaseRequestTypes.RequestValidateAccessToken, nameof(ValidateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            return await SendRequest<GetUserLevelReq, GetUserLevelResp>(request, DatabaseRequestTypes.RequestGetUserLevel, nameof(GetUserLevelAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            return await SendRequest<GetGoldReq, GoldResp>(request, DatabaseRequestTypes.RequestGetGold, nameof(GetGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            return await SendRequest<ChangeGoldReq, GoldResp>(request, DatabaseRequestTypes.RequestChangeGold, nameof(ChangeGoldAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> GetCashAsync(GetCashReq request)
        {
            return await SendRequest<GetCashReq, CashResp>(request, DatabaseRequestTypes.RequestGetCash, nameof(GetCashAsync));
        }

        public async UniTask<DatabaseApiResult<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            return await SendRequest<ChangeCashReq, CashResp>(request, DatabaseRequestTypes.RequestChangeCash, nameof(ChangeCashAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestUpdateAccessToken, nameof(UpdateAccessTokenAsync));
        }

        public async UniTask<DatabaseApiResult> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestCreateUserLogin, nameof(CreateUserLoginAsync));
        }

        public async UniTask<DatabaseApiResult<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            return await SendRequest<FindUsernameReq, FindUsernameResp>(request, DatabaseRequestTypes.RequestFindUsername, nameof(FindUsernameAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            return await SendRequest<CreateCharacterReq, CharacterResp>(request, DatabaseRequestTypes.RequestCreateCharacter, nameof(CreateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request)
        {
            return await SendRequest<ReadCharacterReq, CharacterResp>(request, DatabaseRequestTypes.RequestReadCharacter, nameof(ReadCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request)
        {
            return await SendRequest<ReadCharactersReq, CharactersResp>(request, DatabaseRequestTypes.RequestReadCharacters, nameof(ReadCharactersAsync));
        }

        public async UniTask<DatabaseApiResult<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            return await SendRequest<UpdateCharacterReq, CharacterResp>(request, DatabaseRequestTypes.RequestUpdateCharacter, nameof(UpdateCharacterAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestDeleteCharacter, nameof(DeleteCharacterAsync));
        }

        public async UniTask<DatabaseApiResult<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, FindCharacterNameResp>(request, DatabaseRequestTypes.RequestFindCharacterName, nameof(FindCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            return await SendRequest<FindCharacterNameReq, SocialCharactersResp>(request, DatabaseRequestTypes.RequestFindCharacters, nameof(FindCharactersAsync));
        }

        public async UniTask<DatabaseApiResult> CreateFriendAsync(CreateFriendReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestCreateFriend, nameof(CreateFriendAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteFriendAsync(DeleteFriendReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestDeleteFriend, nameof(DeleteFriendAsync));
        }

        public async UniTask<DatabaseApiResult<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request)
        {
            return await SendRequest<ReadFriendsReq, SocialCharactersResp>(request, DatabaseRequestTypes.RequestReadFriends, nameof(ReadFriendsAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            return await SendRequest<CreateBuildingReq, BuildingResp>(request, DatabaseRequestTypes.RequestCreateBuilding, nameof(CreateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            return await SendRequest<UpdateBuildingReq, BuildingResp>(request, DatabaseRequestTypes.RequestUpdateBuilding, nameof(UpdateBuildingAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestDeleteBuilding, nameof(DeleteBuildingAsync));
        }

        public async UniTask<DatabaseApiResult<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request)
        {
            return await SendRequest<ReadBuildingsReq, BuildingsResp>(request, DatabaseRequestTypes.RequestReadBuildings, nameof(ReadBuildingsAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            return await SendRequest<CreatePartyReq, PartyResp>(request, DatabaseRequestTypes.RequestCreateParty, nameof(CreatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            return await SendRequest<UpdatePartyReq, PartyResp>(request, DatabaseRequestTypes.RequestUpdateParty, nameof(UpdatePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            return await SendRequest<UpdatePartyLeaderReq, PartyResp>(request, DatabaseRequestTypes.RequestUpdatePartyLeader, nameof(UpdatePartyLeaderAsync));
        }

        public async UniTask<DatabaseApiResult> DeletePartyAsync(DeletePartyReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestDeleteParty, nameof(DeletePartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            return await SendRequest<UpdateCharacterPartyReq, PartyResp>(request, DatabaseRequestTypes.RequestUpdateCharacterParty, nameof(UpdateCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestClearCharacterParty, nameof(ClearCharacterPartyAsync));
        }

        public async UniTask<DatabaseApiResult<PartyResp>> ReadPartyAsync(ReadPartyReq request)
        {
            return await SendRequest<ReadPartyReq, PartyResp>(request, DatabaseRequestTypes.RequestReadParty, nameof(ReadPartyAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            return await SendRequest<CreateGuildReq, GuildResp>(request, DatabaseRequestTypes.RequestCreateGuild, nameof(CreateGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            return await SendRequest<UpdateGuildLeaderReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildLeader, nameof(UpdateGuildLeaderAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildMessage, nameof(UpdateGuildMessageAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            return await SendRequest<UpdateGuildMessageReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildMessage2, nameof(UpdateGuildMessage2Async));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            return await SendRequest<UpdateGuildOptionsReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildOptions, nameof(UpdateGuildOptionsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            return await SendRequest<UpdateGuildAutoAcceptRequestsReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildAutoAcceptRequests, nameof(UpdateGuildAutoAcceptRequestsAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            return await SendRequest<UpdateGuildRoleReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildRole, nameof(UpdateGuildRoleAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            return await SendRequest<UpdateGuildMemberRoleReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateGuildMemberRole, nameof(UpdateGuildMemberRoleAsync));
        }

        public async UniTask<DatabaseApiResult> DeleteGuildAsync(DeleteGuildReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestDeleteGuild, nameof(DeleteGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            return await SendRequest<UpdateCharacterGuildReq, GuildResp>(request, DatabaseRequestTypes.RequestUpdateCharacterGuild, nameof(UpdateCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestClearCharacterGuild, nameof(ClearCharacterGuildAsync));
        }

        public async UniTask<DatabaseApiResult<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            return await SendRequest<FindGuildNameReq, FindGuildNameResp>(request, DatabaseRequestTypes.RequestFindGuildName, nameof(FindGuildNameAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> ReadGuildAsync(ReadGuildReq request)
        {
            return await SendRequest<ReadGuildReq, GuildResp>(request, DatabaseRequestTypes.RequestReadGuild, nameof(ReadGuildAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            return await SendRequest<IncreaseGuildExpReq, GuildResp>(request, DatabaseRequestTypes.RequestIncreaseGuildExp, nameof(IncreaseGuildExpAsync));
        }

        public async UniTask<DatabaseApiResult<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            return await SendRequest<AddGuildSkillReq, GuildResp>(request, DatabaseRequestTypes.RequestAddGuildSkill, nameof(AddGuildSkillAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            return await SendRequest<GetGuildGoldReq, GuildGoldResp>(request, DatabaseRequestTypes.RequestGetGuildGold, nameof(GetGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            return await SendRequest<ChangeGuildGoldReq, GuildGoldResp>(request, DatabaseRequestTypes.RequestChangeGuildGold, nameof(ChangeGuildGoldAsync));
        }

        public async UniTask<DatabaseApiResult<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request)
        {
            return await SendRequest<ReadStorageItemsReq, ReadStorageItemsResp>(request, DatabaseRequestTypes.RequestReadStorageItems, nameof(ReadStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateStorageItemsAsync(UpdateStorageItemsReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestUpdateStorageItems, nameof(UpdateStorageItemsAsync));
        }

        public async UniTask<DatabaseApiResult<MailListResp>> MailListAsync(MailListReq request)
        {
            return await SendRequest<MailListReq, MailListResp>(request, DatabaseRequestTypes.RequestMailList, nameof(MailListAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            return await SendRequest<UpdateReadMailStateReq, UpdateReadMailStateResp>(request, DatabaseRequestTypes.RequestUpdateReadMailState, nameof(UpdateReadMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            return await SendRequest<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(request, DatabaseRequestTypes.RequestUpdateClaimMailItemsState, nameof(UpdateClaimMailItemsStateAsync));
        }

        public async UniTask<DatabaseApiResult<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            return await SendRequest<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(request, DatabaseRequestTypes.RequestUpdateDeleteMailState, nameof(UpdateDeleteMailStateAsync));
        }

        public async UniTask<DatabaseApiResult<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            return await SendRequest<SendMailReq, SendMailResp>(request, DatabaseRequestTypes.RequestSendMail, nameof(SendMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            return await SendRequest<GetMailReq, GetMailResp>(request, DatabaseRequestTypes.RequestGetMail, nameof(GetMailAsync));
        }

        public async UniTask<DatabaseApiResult<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            return await SendRequest<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(request, DatabaseRequestTypes.RequestGetIdByCharacterName, nameof(GetIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            return await SendRequest<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(request, DatabaseRequestTypes.RequestGetUserIdByCharacterName, nameof(GetUserIdByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request)
        {
            return await SendRequest<GetMailNotificationReq, GetMailNotificationResp>(request, DatabaseRequestTypes.RequestGetMailNotification, nameof(GetMailNotificationAsync));
        }

        public async UniTask<DatabaseApiResult<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            return await SendRequest<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(request, DatabaseRequestTypes.RequestGetUserUnbanTime, nameof(GetUserUnbanTimeAsync));
        }

        public async UniTask<DatabaseApiResult> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestSetUserUnbanTimeByCharacterName, nameof(SetUserUnbanTimeByCharacterNameAsync));
        }

        public async UniTask<DatabaseApiResult> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestSetCharacterUnmuteTimeByName, nameof(SetCharacterUnmuteTimeByNameAsync));
        }

        public async UniTask<DatabaseApiResult<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            return await SendRequest<GetSummonBuffsReq, GetSummonBuffsResp>(request, DatabaseRequestTypes.RequestGetSummonBuffs, nameof(GetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult> SetSummonBuffsAsync(SetSummonBuffsReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestSetSummonBuffs, nameof(SetSummonBuffsAsync));
        }

        public async UniTask<DatabaseApiResult<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            return await SendRequest<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(request, DatabaseRequestTypes.RequestValidateEmailVerification, nameof(ValidateEmailVerificationAsync));
        }

        public async UniTask<DatabaseApiResult<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            return await SendRequest<FindEmailReq, FindEmailResp>(request, DatabaseRequestTypes.RequestFindEmail, nameof(FindEmailAsync));
        }

        public async UniTask<DatabaseApiResult<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request)
        {
            return await SendRequest<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(request, DatabaseRequestTypes.RequestGetFriendRequestNotification, nameof(GetFriendRequestNotificationAsync));
        }

        public async UniTask<DatabaseApiResult> UpdateUserCount(UpdateUserCountReq request)
        {
            return await SendRequest(request, DatabaseRequestTypes.RequestUpdateUserCount, nameof(UpdateUserCount));
        }
    }
}
