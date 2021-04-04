using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        public async UniTask<AsyncResponseData<ValidateUserLoginResp>> RequestValidateUserLogin(ValidateUserLoginReq request)
        {
            return await Client.SendRequestAsync<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.RequestValidateUserLogin, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestValidateAccessToken(ValidateAccessTokenReq request)
        {
            return await Client.SendRequestAsync<ValidateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestValidateAccessToken, request);
        }

        public async UniTask<AsyncResponseData<GetUserLevelResp>> RequestGetUserLevel(GetUserLevelReq request)
        {
            return await Client.SendRequestAsync<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.RequestGetUserLevel, request);
        }

        public async UniTask<AsyncResponseData<GoldResp>> RequestGetGold(GetGoldReq request)
        {
            return await Client.SendRequestAsync<GetGoldReq, GoldResp>(DatabaseRequestTypes.RequestGetGold, request);
        }

        public async UniTask<AsyncResponseData<GoldResp>> RequestChangeGold(ChangeGoldReq request)
        {
            return await Client.SendRequestAsync<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.RequestChangeGold, request);
        }

        public async UniTask<AsyncResponseData<CashResp>> RequestGetCash(GetCashReq request)
        {
            return await Client.SendRequestAsync<GetCashReq, CashResp>(DatabaseRequestTypes.RequestGetCash, request);
        }

        public async UniTask<AsyncResponseData<CashResp>> RequestChangeCash(ChangeCashReq request)
        {
            return await Client.SendRequestAsync<ChangeCashReq, CashResp>(DatabaseRequestTypes.RequestChangeCash, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestUpdateAccessToken(UpdateAccessTokenReq request)
        {
            return await Client.SendRequestAsync<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateAccessToken, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestCreateUserLogin(CreateUserLoginReq request)
        {
            return await Client.SendRequestAsync<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateUserLogin, request);
        }

        public async UniTask<AsyncResponseData<FindUsernameResp>> RequestFindUsername(FindUsernameReq request)
        {
            return await Client.SendRequestAsync<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.RequestFindUsername, request);
        }

        public async UniTask<AsyncResponseData<CharacterResp>> RequestCreateCharacter(CreateCharacterReq request)
        {
            return await Client.SendRequestAsync<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestCreateCharacter, request);
        }

        public async UniTask<AsyncResponseData<CharacterResp>> RequestReadCharacter(ReadCharacterReq request)
        {
            return await Client.SendRequestAsync<ReadCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestReadCharacter, request);
        }

        public async UniTask<AsyncResponseData<CharactersResp>> RequestReadCharacters(ReadCharactersReq request)
        {
            return await Client.SendRequestAsync<ReadCharactersReq, CharactersResp>(DatabaseRequestTypes.RequestReadCharacters, request);
        }

        public async UniTask<AsyncResponseData<CharacterResp>> RequestUpdateCharacter(UpdateCharacterReq request)
        {
            return await Client.SendRequestAsync<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestUpdateCharacter, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestDeleteCharacter(DeleteCharacterReq request)
        {
            return await Client.SendRequestAsync<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteCharacter, request);
        }

        public async UniTask<AsyncResponseData<FindCharacterNameResp>> RequestFindCharacterName(FindCharacterNameReq request)
        {
            return await Client.SendRequestAsync<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.RequestFindCharacterName, request);
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> RequestFindCharacters(FindCharacterNameReq request)
        {
            return await Client.SendRequestAsync<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.RequestFindCharacters, request);
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> RequestCreateFriend(CreateFriendReq request)
        {
            return await Client.SendRequestAsync<CreateFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestCreateFriend, request);
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> RequestDeleteFriend(DeleteFriendReq request)
        {
            return await Client.SendRequestAsync<DeleteFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestDeleteFriend, request);
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> RequestReadFriends(ReadFriendsReq request)
        {
            return await Client.SendRequestAsync<ReadFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.RequestReadFriends, request);
        }

        public async UniTask<AsyncResponseData<BuildingResp>> RequestCreateBuilding(CreateBuildingReq request)
        {
            return await Client.SendRequestAsync<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestCreateBuilding, request);
        }

        public async UniTask<AsyncResponseData<BuildingResp>> RequestUpdateBuilding(UpdateBuildingReq request)
        {
            return await Client.SendRequestAsync<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestUpdateBuilding, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestDeleteBuilding(DeleteBuildingReq request)
        {
            return await Client.SendRequestAsync<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteBuilding, request);
        }

        public async UniTask<AsyncResponseData<BuildingsResp>> RequestReadBuildings(ReadBuildingsReq request)
        {
            return await Client.SendRequestAsync<ReadBuildingsReq, BuildingsResp>(DatabaseRequestTypes.RequestReadBuildings, request);
        }

        public async UniTask<AsyncResponseData<PartyResp>> RequestCreateParty(CreatePartyReq request)
        {
            return await Client.SendRequestAsync<CreatePartyReq, PartyResp>(DatabaseRequestTypes.RequestCreateParty, request);
        }

        public async UniTask<AsyncResponseData<PartyResp>> RequestUpdateParty(UpdatePartyReq request)
        {
            return await Client.SendRequestAsync<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateParty, request);
        }

        public async UniTask<AsyncResponseData<PartyResp>> RequestUpdatePartyLeader(UpdatePartyLeaderReq request)
        {
            return await Client.SendRequestAsync<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.RequestUpdatePartyLeader, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestDeleteParty(DeletePartyReq request)
        {
            return await Client.SendRequestAsync<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteParty, request);
        }

        public async UniTask<AsyncResponseData<PartyResp>> RequestUpdateCharacterParty(UpdateCharacterPartyReq request)
        {
            return await Client.SendRequestAsync<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateCharacterParty, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestClearCharacterParty(ClearCharacterPartyReq request)
        {
            return await Client.SendRequestAsync<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterParty, request);
        }

        public async UniTask<AsyncResponseData<PartyResp>> RequestReadParty(ReadPartyReq request)
        {
            return await Client.SendRequestAsync<ReadPartyReq, PartyResp>(DatabaseRequestTypes.RequestReadParty, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestCreateGuild(CreateGuildReq request)
        {
            return await Client.SendRequestAsync<CreateGuildReq, GuildResp>(DatabaseRequestTypes.RequestCreateGuild, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestUpdateGuildLeader(UpdateGuildLeaderReq request)
        {
            return await Client.SendRequestAsync<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildLeader, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestUpdateGuildMessage(UpdateGuildMessageReq request)
        {
            return await Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestUpdateGuildRole(UpdateGuildRoleReq request)
        {
            return await Client.SendRequestAsync<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRole, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestUpdateGuildMemberRole(UpdateGuildMemberRoleReq request)
        {
            return await Client.SendRequestAsync<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMemberRole, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestDeleteGuild(DeleteGuildReq request)
        {
            return await Client.SendRequestAsync<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteGuild, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestUpdateCharacterGuild(UpdateCharacterGuildReq request)
        {
            return await Client.SendRequestAsync<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.RequestUpdateCharacterGuild, request);
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> RequestClearCharacterGuild(ClearCharacterGuildReq request)
        {
            return await Client.SendRequestAsync<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterGuild, request);
        }

        public async UniTask<AsyncResponseData<FindGuildNameResp>> RequestFindGuildName(FindGuildNameReq request)
        {
            return await Client.SendRequestAsync<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.RequestFindGuildName, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestReadGuild(ReadGuildReq request)
        {
            return await Client.SendRequestAsync<ReadGuildReq, GuildResp>(DatabaseRequestTypes.RequestReadGuild, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestIncreaseGuildExp(IncreaseGuildExpReq request)
        {
            return await Client.SendRequestAsync<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.RequestIncreaseGuildExp, request);
        }

        public async UniTask<AsyncResponseData<GuildResp>> RequestAddGuildSkill(AddGuildSkillReq request)
        {
            return await Client.SendRequestAsync<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.RequestAddGuildSkill, request);
        }

        public async UniTask<AsyncResponseData<GuildGoldResp>> RequestGetGuildGold(GetGuildGoldReq request)
        {
            return await Client.SendRequestAsync<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestGetGuildGold, request);
        }

        public async UniTask<AsyncResponseData<GuildGoldResp>> RequestChangeGuildGold(ChangeGuildGoldReq request)
        {
            return await Client.SendRequestAsync<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestChangeGuildGold, request);
        }

        public async UniTask<AsyncResponseData<ReadStorageItemsResp>> RequestReadStorageItems(ReadStorageItemsReq request)
        {
            return await Client.SendRequestAsync<ReadStorageItemsReq, ReadStorageItemsResp>(DatabaseRequestTypes.RequestReadStorageItems, request);
        }

        public async UniTask<AsyncResponseData<MoveItemToStorageResp>> RequestMoveItemToStorage(MoveItemToStorageReq request)
        {
            return await Client.SendRequestAsync<MoveItemToStorageReq, MoveItemToStorageResp>(DatabaseRequestTypes.RequestMoveItemToStorage, request);
        }

        public async UniTask<AsyncResponseData<MoveItemFromStorageResp>> RequestMoveItemFromStorage(MoveItemFromStorageReq request)
        {
            return await Client.SendRequestAsync<MoveItemFromStorageReq, MoveItemFromStorageResp>(DatabaseRequestTypes.RequestMoveItemFromStorage, request);
        }

        public async UniTask<AsyncResponseData<SwapOrMergeStorageItemResp>> RequestSwapOrMergeStorageItem(SwapOrMergeStorageItemReq request)
        {
            return await Client.SendRequestAsync<SwapOrMergeStorageItemReq, SwapOrMergeStorageItemResp>(DatabaseRequestTypes.RequestSwapOrMergeStorageItem, request);
        }

        public async UniTask<AsyncResponseData<IncreaseStorageItemsResp>> RequestIncreaseStorageItems(IncreaseStorageItemsReq request)
        {
            return await Client.SendRequestAsync<IncreaseStorageItemsReq, IncreaseStorageItemsResp>(DatabaseRequestTypes.RequestIncreaseStorageItems, request);
        }

        public async UniTask<AsyncResponseData<DecreaseStorageItemsResp>> RequestDecreaseStorageItems(DecreaseStorageItemsReq request)
        {
            return await Client.SendRequestAsync<DecreaseStorageItemsReq, DecreaseStorageItemsResp>(DatabaseRequestTypes.RequestDecreaseStorageItems, request);
        }

        public async UniTask<AsyncResponseData<MailListResp>> RequestMailList(MailListReq request)
        {
            return await Client.SendRequestAsync<MailListReq, MailListResp>(DatabaseRequestTypes.RequestMailList, request);
        }

        public async UniTask<AsyncResponseData<UpdateReadMailStateResp>> RequestUpdateReadMailState(UpdateReadMailStateReq request)
        {
            return await Client.SendRequestAsync<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.RequestUpdateReadMailState, request);
        }

        public async UniTask<AsyncResponseData<UpdateClaimMailItemsStateResp>> RequestUpdateClaimMailItemsState(UpdateClaimMailItemsStateReq request)
        {
            return await Client.SendRequestAsync<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.RequestUpdateClaimMailItemsState, request);
        }

        public async UniTask<AsyncResponseData<UpdateDeleteMailStateResp>> RequestUpdateDeleteMailState(UpdateDeleteMailStateReq request)
        {
            return await Client.SendRequestAsync<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.RequestUpdateDeleteMailState, request);
        }

        public async UniTask<AsyncResponseData<SendMailResp>> RequestSendMail(SendMailReq request)
        {
            return await Client.SendRequestAsync<SendMailReq, SendMailResp>(DatabaseRequestTypes.RequestSendMail, request);
        }

        public async UniTask<AsyncResponseData<GetMailResp>> RequestGetMail(GetMailReq request)
        {
            return await Client.SendRequestAsync<GetMailReq, GetMailResp>(DatabaseRequestTypes.RequestGetMail, request);
        }

        public async UniTask<AsyncResponseData<GetIdByCharacterNameResp>> RequestGetIdByCharacterName(GetIdByCharacterNameReq request)
        {
            return await Client.SendRequestAsync<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetIdByCharacterName, request);
        }

        public async UniTask<AsyncResponseData<GetUserIdByCharacterNameResp>> RequestGetUserIdByCharacterName(GetUserIdByCharacterNameReq request)
        {
            return await Client.SendRequestAsync<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetUserIdByCharacterName, request);
        }
    }
}