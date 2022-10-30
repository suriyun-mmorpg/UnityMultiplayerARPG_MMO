using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial interface IDatabaseClient
    {
        UniTask<AsyncResponseData<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request);

        UniTask<AsyncResponseData<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request);

        UniTask<AsyncResponseData<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request);

        UniTask<AsyncResponseData<GoldResp>> GetGoldAsync(GetGoldReq request);

        UniTask<AsyncResponseData<GoldResp>> ChangeGoldAsync(ChangeGoldReq request);

        UniTask<AsyncResponseData<CashResp>> GetCashAsync(GetCashReq request);

        UniTask<AsyncResponseData<CashResp>> ChangeCashAsync(ChangeCashReq request);

        UniTask<AsyncResponseData<EmptyMessage>> UpdateAccessTokenAsync(UpdateAccessTokenReq request);

        UniTask<AsyncResponseData<EmptyMessage>> CreateUserLoginAsync(CreateUserLoginReq request);

        UniTask<AsyncResponseData<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request);

        UniTask<AsyncResponseData<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request);

        UniTask<AsyncResponseData<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request);

        UniTask<AsyncResponseData<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request);

        UniTask<AsyncResponseData<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request);

        UniTask<AsyncResponseData<EmptyMessage>> DeleteCharacterAsync(DeleteCharacterReq request);

        UniTask<AsyncResponseData<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request);

        UniTask<AsyncResponseData<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request);

        UniTask<AsyncResponseData<EmptyMessage>> CreateFriendAsync(CreateFriendReq request);

        UniTask<AsyncResponseData<EmptyMessage>> DeleteFriendAsync(DeleteFriendReq request);

        UniTask<AsyncResponseData<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request);

        UniTask<AsyncResponseData<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request);

        UniTask<AsyncResponseData<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request);

        UniTask<AsyncResponseData<EmptyMessage>> DeleteBuildingAsync(DeleteBuildingReq request);

        UniTask<AsyncResponseData<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request);

        UniTask<AsyncResponseData<PartyResp>> CreatePartyAsync(CreatePartyReq request);

        UniTask<AsyncResponseData<PartyResp>> UpdatePartyAsync(UpdatePartyReq request);

        UniTask<AsyncResponseData<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request);

        UniTask<AsyncResponseData<EmptyMessage>> DeletePartyAsync(DeletePartyReq request);

        UniTask<AsyncResponseData<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request);

        UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterPartyAsync(ClearCharacterPartyReq request);

        UniTask<AsyncResponseData<PartyResp>> ReadPartyAsync(ReadPartyReq request);

        UniTask<AsyncResponseData<GuildResp>> CreateGuildAsync(CreateGuildReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request);

        UniTask<AsyncResponseData<EmptyMessage>> DeleteGuildAsync(DeleteGuildReq request);

        UniTask<AsyncResponseData<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request);

        UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterGuildAsync(ClearCharacterGuildReq request);

        UniTask<AsyncResponseData<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request);

        UniTask<AsyncResponseData<GuildResp>> ReadGuildAsync(ReadGuildReq request);

        UniTask<AsyncResponseData<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request);

        UniTask<AsyncResponseData<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request);

        UniTask<AsyncResponseData<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request);

        UniTask<AsyncResponseData<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request);

        UniTask<AsyncResponseData<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request);

        UniTask<AsyncResponseData<EmptyMessage>> UpdateStorageItemsAsync(UpdateStorageItemsReq request);

        UniTask<AsyncResponseData<MailListResp>> MailListAsync(MailListReq request);

        UniTask<AsyncResponseData<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request);

        UniTask<AsyncResponseData<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request);

        UniTask<AsyncResponseData<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request);

        UniTask<AsyncResponseData<SendMailResp>> SendMailAsync(SendMailReq request);

        UniTask<AsyncResponseData<GetMailResp>> GetMailAsync(GetMailReq request);

        UniTask<AsyncResponseData<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request);

        UniTask<AsyncResponseData<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request);

        UniTask<AsyncResponseData<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request);

        UniTask<AsyncResponseData<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request);

        UniTask<AsyncResponseData<EmptyMessage>> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request);

        UniTask<AsyncResponseData<EmptyMessage>> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request);

        UniTask<AsyncResponseData<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request);

        UniTask<AsyncResponseData<EmptyMessage>> SetSummonBuffsAsync(SetSummonBuffsReq request);

        UniTask<AsyncResponseData<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request);

        UniTask<AsyncResponseData<FindEmailResp>> FindEmailAsync(FindEmailReq request);

        UniTask<AsyncResponseData<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request);

        UniTask<AsyncResponseData<EmptyMessage>> UpdateUserCount(UpdateUserCountReq request);
    }
}
