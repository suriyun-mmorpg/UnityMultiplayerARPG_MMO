using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager : IDatabaseClient
    {
        public UniTask<AsyncResponseData<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            return Client.SendRequestAsync<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.RequestValidateUserLogin, request);
        }

        public UniTask<AsyncResponseData<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            return Client.SendRequestAsync<ValidateAccessTokenReq, ValidateAccessTokenResp>(DatabaseRequestTypes.RequestValidateAccessToken, request);
        }

        public UniTask<AsyncResponseData<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            return Client.SendRequestAsync<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.RequestGetUserLevel, request);
        }

        public UniTask<AsyncResponseData<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            return Client.SendRequestAsync<GetGoldReq, GoldResp>(DatabaseRequestTypes.RequestGetGold, request);
        }

        public UniTask<AsyncResponseData<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            return Client.SendRequestAsync<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.RequestChangeGold, request);
        }

        public UniTask<AsyncResponseData<CashResp>> GetCashAsync(GetCashReq request)
        {
            return Client.SendRequestAsync<GetCashReq, CashResp>(DatabaseRequestTypes.RequestGetCash, request);
        }

        public UniTask<AsyncResponseData<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            return Client.SendRequestAsync<ChangeCashReq, CashResp>(DatabaseRequestTypes.RequestChangeCash, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            return Client.SendRequestAsync<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateAccessToken, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            return Client.SendRequestAsync<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateUserLogin, request);
        }

        public UniTask<AsyncResponseData<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            return Client.SendRequestAsync<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.RequestFindUsername, request);
        }

        public UniTask<AsyncResponseData<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            return Client.SendRequestAsync<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestCreateCharacter, request);
        }

        public UniTask<AsyncResponseData<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request)
        {
            return Client.SendRequestAsync<ReadCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestReadCharacter, request);
        }

        public UniTask<AsyncResponseData<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request)
        {
            return Client.SendRequestAsync<ReadCharactersReq, CharactersResp>(DatabaseRequestTypes.RequestReadCharacters, request);
        }

        public UniTask<AsyncResponseData<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            return Client.SendRequestAsync<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestUpdateCharacter, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            return Client.SendRequestAsync<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteCharacter, request);
        }

        public UniTask<AsyncResponseData<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            return Client.SendRequestAsync<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.RequestFindCharacterName, request);
        }

        public UniTask<AsyncResponseData<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            return Client.SendRequestAsync<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.RequestFindCharacters, request);
        }

        public UniTask<AsyncResponseData<SocialCharactersResp>> CreateFriendAsync(CreateFriendReq request)
        {
            return Client.SendRequestAsync<CreateFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestCreateFriend, request);
        }

        public UniTask<AsyncResponseData<SocialCharactersResp>> DeleteFriendAsync(DeleteFriendReq request)
        {
            return Client.SendRequestAsync<DeleteFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestDeleteFriend, request);
        }

        public UniTask<AsyncResponseData<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request)
        {
            return Client.SendRequestAsync<ReadFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.RequestReadFriends, request);
        }

        public UniTask<AsyncResponseData<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            return Client.SendRequestAsync<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestCreateBuilding, request);
        }

        public UniTask<AsyncResponseData<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            return Client.SendRequestAsync<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestUpdateBuilding, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            return Client.SendRequestAsync<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteBuilding, request);
        }

        public UniTask<AsyncResponseData<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request)
        {
            return Client.SendRequestAsync<ReadBuildingsReq, BuildingsResp>(DatabaseRequestTypes.RequestReadBuildings, request);
        }

        public UniTask<AsyncResponseData<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            return Client.SendRequestAsync<CreatePartyReq, PartyResp>(DatabaseRequestTypes.RequestCreateParty, request);
        }

        public UniTask<AsyncResponseData<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            return Client.SendRequestAsync<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateParty, request);
        }

        public UniTask<AsyncResponseData<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            return Client.SendRequestAsync<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.RequestUpdatePartyLeader, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> DeletePartyAsync(DeletePartyReq request)
        {
            return Client.SendRequestAsync<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteParty, request);
        }

        public UniTask<AsyncResponseData<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            return Client.SendRequestAsync<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateCharacterParty, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            return Client.SendRequestAsync<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterParty, request);
        }

        public UniTask<AsyncResponseData<PartyResp>> ReadPartyAsync(ReadPartyReq request)
        {
            return Client.SendRequestAsync<ReadPartyReq, PartyResp>(DatabaseRequestTypes.RequestReadParty, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            return Client.SendRequestAsync<CreateGuildReq, GuildResp>(DatabaseRequestTypes.RequestCreateGuild, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            return Client.SendRequestAsync<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildLeader, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            return Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            return Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage2, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            return Client.SendRequestAsync<UpdateGuildOptionsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildOptions, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            return Client.SendRequestAsync<UpdateGuildAutoAcceptRequestsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildAutoAcceptRequests, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            return Client.SendRequestAsync<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRole, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            return Client.SendRequestAsync<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMemberRole, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> DeleteGuildAsync(DeleteGuildReq request)
        {
            return Client.SendRequestAsync<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteGuild, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            return Client.SendRequestAsync<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.RequestUpdateCharacterGuild, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            return Client.SendRequestAsync<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterGuild, request);
        }

        public UniTask<AsyncResponseData<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            return Client.SendRequestAsync<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.RequestFindGuildName, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> ReadGuildAsync(ReadGuildReq request)
        {
            return Client.SendRequestAsync<ReadGuildReq, GuildResp>(DatabaseRequestTypes.RequestReadGuild, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            return Client.SendRequestAsync<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.RequestIncreaseGuildExp, request);
        }

        public UniTask<AsyncResponseData<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            return Client.SendRequestAsync<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.RequestAddGuildSkill, request);
        }

        public UniTask<AsyncResponseData<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            return Client.SendRequestAsync<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestGetGuildGold, request);
        }

        public UniTask<AsyncResponseData<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            return Client.SendRequestAsync<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestChangeGuildGold, request);
        }

        public UniTask<AsyncResponseData<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request)
        {
            return Client.SendRequestAsync<ReadStorageItemsReq, ReadStorageItemsResp>(DatabaseRequestTypes.RequestReadStorageItems, request);
        }

        public UniTask<AsyncResponseData<MoveItemToStorageResp>> MoveItemToStorageAsync(MoveItemToStorageReq request)
        {
            return Client.SendRequestAsync<MoveItemToStorageReq, MoveItemToStorageResp>(DatabaseRequestTypes.RequestMoveItemToStorage, request);
        }

        public UniTask<AsyncResponseData<MoveItemFromStorageResp>> MoveItemFromStorageAsync(MoveItemFromStorageReq request)
        {
            return Client.SendRequestAsync<MoveItemFromStorageReq, MoveItemFromStorageResp>(DatabaseRequestTypes.RequestMoveItemFromStorage, request);
        }

        public UniTask<AsyncResponseData<SwapOrMergeStorageItemResp>> SwapOrMergeStorageItemAsync(SwapOrMergeStorageItemReq request)
        {
            return Client.SendRequestAsync<SwapOrMergeStorageItemReq, SwapOrMergeStorageItemResp>(DatabaseRequestTypes.RequestSwapOrMergeStorageItem, request);
        }

        public UniTask<AsyncResponseData<IncreaseStorageItemsResp>> IncreaseStorageItemsAsync(IncreaseStorageItemsReq request)
        {
            return Client.SendRequestAsync<IncreaseStorageItemsReq, IncreaseStorageItemsResp>(DatabaseRequestTypes.RequestIncreaseStorageItems, request);
        }

        public UniTask<AsyncResponseData<DecreaseStorageItemsResp>> DecreaseStorageItemsAsync(DecreaseStorageItemsReq request)
        {
            return Client.SendRequestAsync<DecreaseStorageItemsReq, DecreaseStorageItemsResp>(DatabaseRequestTypes.RequestDecreaseStorageItems, request);
        }

        public UniTask<AsyncResponseData<MailListResp>> MailListAsync(MailListReq request)
        {
            return Client.SendRequestAsync<MailListReq, MailListResp>(DatabaseRequestTypes.RequestMailList, request);
        }

        public UniTask<AsyncResponseData<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            return Client.SendRequestAsync<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.RequestUpdateReadMailState, request);
        }

        public UniTask<AsyncResponseData<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            return Client.SendRequestAsync<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.RequestUpdateClaimMailItemsState, request);
        }

        public UniTask<AsyncResponseData<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            return Client.SendRequestAsync<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.RequestUpdateDeleteMailState, request);
        }

        public UniTask<AsyncResponseData<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            return Client.SendRequestAsync<SendMailReq, SendMailResp>(DatabaseRequestTypes.RequestSendMail, request);
        }

        public UniTask<AsyncResponseData<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            return Client.SendRequestAsync<GetMailReq, GetMailResp>(DatabaseRequestTypes.RequestGetMail, request);
        }

        public UniTask<AsyncResponseData<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            return Client.SendRequestAsync<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetIdByCharacterName, request);
        }

        public UniTask<AsyncResponseData<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            return Client.SendRequestAsync<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetUserIdByCharacterName, request);
        }

        public UniTask<AsyncResponseData<GetMailNotificationCountResp>> GetMailsCountAsync(GetMailNotificationCountReq request)
        {
            return Client.SendRequestAsync<GetMailNotificationCountReq, GetMailNotificationCountResp>(DatabaseRequestTypes.RequestGetMailNotificationCount, request);
        }

        public UniTask<AsyncResponseData<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            return Client.SendRequestAsync<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(DatabaseRequestTypes.RequestGetUserUnbanTime, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            return Client.SendRequestAsync<SetUserUnbanTimeByCharacterNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetUserUnbanTimeByCharacterName, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            return Client.SendRequestAsync<SetCharacterUnmuteTimeByNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetCharacterUnmuteTimeByName, request);
        }

        public UniTask<AsyncResponseData<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            return Client.SendRequestAsync<GetSummonBuffsReq, GetSummonBuffsResp>(DatabaseRequestTypes.RequestGetSummonBuffs, request);
        }

        public UniTask<AsyncResponseData<EmptyMessage>> SetSummonBuffsAsync(SetSummonBuffsReq request)
        {
            return Client.SendRequestAsync<SetSummonBuffsReq, EmptyMessage>(DatabaseRequestTypes.RequestSetSummonBuffs, request);
        }

        public UniTask<AsyncResponseData<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            return Client.SendRequestAsync<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(DatabaseRequestTypes.RequestValidateEmailVerification, request);
        }

        public UniTask<AsyncResponseData<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            return Client.SendRequestAsync<FindEmailReq, FindEmailResp>(DatabaseRequestTypes.RequestFindEmail, request);
        }
    }
}
