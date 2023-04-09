using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial interface IDatabaseClient
    {
        UniTask<DatabaseApiResult<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request);

        UniTask<DatabaseApiResult<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request);

        UniTask<DatabaseApiResult<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request);

        UniTask<DatabaseApiResult<GoldResp>> GetGoldAsync(GetGoldReq request);

        UniTask<DatabaseApiResult<GoldResp>> ChangeGoldAsync(ChangeGoldReq request);

        UniTask<DatabaseApiResult<CashResp>> GetCashAsync(GetCashReq request);

        UniTask<DatabaseApiResult<CashResp>> ChangeCashAsync(ChangeCashReq request);

        UniTask<DatabaseApiResult> UpdateAccessTokenAsync(UpdateAccessTokenReq request);

        UniTask<DatabaseApiResult> CreateUserLoginAsync(CreateUserLoginReq request);

        UniTask<DatabaseApiResult<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request);

        UniTask<DatabaseApiResult<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request);

        UniTask<DatabaseApiResult<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request);

        UniTask<DatabaseApiResult<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request);

        UniTask<DatabaseApiResult<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request);

        UniTask<DatabaseApiResult> DeleteCharacterAsync(DeleteCharacterReq request);

        UniTask<DatabaseApiResult<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request);

        UniTask<DatabaseApiResult<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request);

        UniTask<DatabaseApiResult> CreateFriendAsync(CreateFriendReq request);

        UniTask<DatabaseApiResult> DeleteFriendAsync(DeleteFriendReq request);

        UniTask<DatabaseApiResult<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request);

        UniTask<DatabaseApiResult<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request);

        UniTask<DatabaseApiResult<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request);

        UniTask<DatabaseApiResult> DeleteBuildingAsync(DeleteBuildingReq request);

        UniTask<DatabaseApiResult<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request);

        UniTask<DatabaseApiResult<PartyResp>> CreatePartyAsync(CreatePartyReq request);

        UniTask<DatabaseApiResult<PartyResp>> UpdatePartyAsync(UpdatePartyReq request);

        UniTask<DatabaseApiResult<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request);

        UniTask<DatabaseApiResult> DeletePartyAsync(DeletePartyReq request);

        UniTask<DatabaseApiResult<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request);

        UniTask<DatabaseApiResult> ClearCharacterPartyAsync(ClearCharacterPartyReq request);

        UniTask<DatabaseApiResult<PartyResp>> ReadPartyAsync(ReadPartyReq request);

        UniTask<DatabaseApiResult<GuildResp>> CreateGuildAsync(CreateGuildReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request);

        UniTask<DatabaseApiResult> DeleteGuildAsync(DeleteGuildReq request);

        UniTask<DatabaseApiResult<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request);

        UniTask<DatabaseApiResult> ClearCharacterGuildAsync(ClearCharacterGuildReq request);

        UniTask<DatabaseApiResult<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request);

        UniTask<DatabaseApiResult<GuildResp>> ReadGuildAsync(ReadGuildReq request);

        UniTask<DatabaseApiResult<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request);

        UniTask<DatabaseApiResult<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request);

        UniTask<DatabaseApiResult<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request);

        UniTask<DatabaseApiResult<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request);

        UniTask<DatabaseApiResult<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request);

        UniTask<DatabaseApiResult> UpdateStorageItemsAsync(UpdateStorageItemsReq request);

        UniTask<DatabaseApiResult<MailListResp>> MailListAsync(MailListReq request);

        UniTask<DatabaseApiResult<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request);

        UniTask<DatabaseApiResult<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request);

        UniTask<DatabaseApiResult<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request);

        UniTask<DatabaseApiResult<SendMailResp>> SendMailAsync(SendMailReq request);

        UniTask<DatabaseApiResult<GetMailResp>> GetMailAsync(GetMailReq request);

        UniTask<DatabaseApiResult<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request);

        UniTask<DatabaseApiResult<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request);

        UniTask<DatabaseApiResult<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request);

        UniTask<DatabaseApiResult<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request);

        UniTask<DatabaseApiResult> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request);

        UniTask<DatabaseApiResult> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request);

        UniTask<DatabaseApiResult<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request);

        UniTask<DatabaseApiResult> SetSummonBuffsAsync(SetSummonBuffsReq request);

        UniTask<DatabaseApiResult<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request);

        UniTask<DatabaseApiResult<FindEmailResp>> FindEmailAsync(FindEmailReq request);

        UniTask<DatabaseApiResult<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request);

        UniTask<DatabaseApiResult> UpdateUserCount(UpdateUserCountReq request);
    }
}
