using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        public async UniTask<ValidateUserLoginResp> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            var result = await Client.SendRequestAsync<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.RequestValidateUserLogin, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new ValidateUserLoginResp();
            return result.Response;
        }

        public async UniTask<ValidateAccessTokenResp> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            var result = await Client.SendRequestAsync<ValidateAccessTokenReq, ValidateAccessTokenResp>(DatabaseRequestTypes.RequestValidateAccessToken, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new ValidateAccessTokenResp();
            return result.Response;
        }

        public async UniTask<GetUserLevelResp> GetUserLevelAsync(GetUserLevelReq request)
        {
            var result = await Client.SendRequestAsync<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.RequestGetUserLevel, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetUserLevelResp();
            return result.Response;
        }

        public async UniTask<GoldResp> GetGoldAsync(GetGoldReq request)
        {
            var result = await Client.SendRequestAsync<GetGoldReq, GoldResp>(DatabaseRequestTypes.RequestGetGold, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GoldResp();
            return result.Response;
        }

        public async UniTask<GoldResp> ChangeGoldAsync(ChangeGoldReq request)
        {
            var result = await Client.SendRequestAsync<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.RequestChangeGold, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GoldResp();
            return result.Response;
        }

        public async UniTask<CashResp> GetCashAsync(GetCashReq request)
        {
            var result = await Client.SendRequestAsync<GetCashReq, CashResp>(DatabaseRequestTypes.RequestGetCash, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CashResp();
            return result.Response;
        }

        public async UniTask<CashResp> ChangeCashAsync(ChangeCashReq request)
        {
            var result = await Client.SendRequestAsync<ChangeCashReq, CashResp>(DatabaseRequestTypes.RequestChangeCash, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CashResp();
            return result.Response;
        }

        public async UniTask UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            await Client.SendRequestAsync<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateAccessToken, request);
        }

        public async UniTask CreateUserLoginAsync(CreateUserLoginReq request)
        {
            await Client.SendRequestAsync<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateUserLogin, request);
        }

        public async UniTask<FindUsernameResp> FindUsernameAsync(FindUsernameReq request)
        {
            var result = await Client.SendRequestAsync<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.RequestFindUsername, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new FindUsernameResp();
            return result.Response;
        }

        public async UniTask<CharacterResp> CreateCharacterAsync(CreateCharacterReq request)
        {
            var result = await Client.SendRequestAsync<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestCreateCharacter, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CharacterResp();
            return result.Response;
        }

        public async UniTask<CharacterResp> ReadCharacterAsync(ReadCharacterReq request)
        {
            var result = await Client.SendRequestAsync<ReadCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestReadCharacter, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CharacterResp();
            return result.Response;
        }

        public async UniTask<CharactersResp> ReadCharactersAsync(ReadCharactersReq request)
        {
            var result = await Client.SendRequestAsync<ReadCharactersReq, CharactersResp>(DatabaseRequestTypes.RequestReadCharacters, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CharactersResp();
            return result.Response;
        }

        public async UniTask<CharacterResp> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            var result = await Client.SendRequestAsync<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestUpdateCharacter, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new CharacterResp();
            return result.Response;
        }

        public async UniTask DeleteCharacterAsync(DeleteCharacterReq request)
        {
            await Client.SendRequestAsync<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteCharacter, request);
        }

        public async UniTask<FindCharacterNameResp> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            var result = await Client.SendRequestAsync<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.RequestFindCharacterName, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new FindCharacterNameResp();
            return result.Response;
        }

        public async UniTask<SocialCharactersResp> FindCharactersAsync(FindCharacterNameReq request)
        {
            var result = await Client.SendRequestAsync<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.RequestFindCharacters, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SocialCharactersResp();
            return result.Response;
        }

        public async UniTask<SocialCharactersResp> CreateFriendAsync(CreateFriendReq request)
        {
            var result = await Client.SendRequestAsync<CreateFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestCreateFriend, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SocialCharactersResp();
            return result.Response;
        }

        public async UniTask<SocialCharactersResp> DeleteFriendAsync(DeleteFriendReq request)
        {
            var result = await Client.SendRequestAsync<DeleteFriendReq, SocialCharactersResp>(DatabaseRequestTypes.RequestDeleteFriend, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SocialCharactersResp();
            return result.Response;
        }

        public async UniTask<SocialCharactersResp> ReadFriendsAsync(ReadFriendsReq request)
        {
            var result = await Client.SendRequestAsync<ReadFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.RequestReadFriends, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SocialCharactersResp();
            return result.Response;
        }

        public async UniTask<BuildingResp> CreateBuildingAsync(CreateBuildingReq request)
        {
            var result = await Client.SendRequestAsync<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestCreateBuilding, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new BuildingResp();
            return result.Response;
        }

        public async UniTask<BuildingResp> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            var result = await Client.SendRequestAsync<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestUpdateBuilding, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new BuildingResp();
            return result.Response;
        }

        public async UniTask DeleteBuildingAsync(DeleteBuildingReq request)
        {
            await Client.SendRequestAsync<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteBuilding, request);
        }

        public async UniTask<BuildingsResp> ReadBuildingsAsync(ReadBuildingsReq request)
        {
            var result = await Client.SendRequestAsync<ReadBuildingsReq, BuildingsResp>(DatabaseRequestTypes.RequestReadBuildings, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new BuildingsResp();
            return result.Response;
        }

        public async UniTask<PartyResp> CreatePartyAsync(CreatePartyReq request)
        {
            var result = await Client.SendRequestAsync<CreatePartyReq, PartyResp>(DatabaseRequestTypes.RequestCreateParty, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new PartyResp();
            return result.Response;
        }

        public async UniTask<PartyResp> UpdatePartyAsync(UpdatePartyReq request)
        {
            var result = await Client.SendRequestAsync<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateParty, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new PartyResp();
            return result.Response;
        }

        public async UniTask<PartyResp> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            var result = await Client.SendRequestAsync<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.RequestUpdatePartyLeader, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new PartyResp();
            return result.Response;
        }

        public async UniTask DeletePartyAsync(DeletePartyReq request)
        {
            await Client.SendRequestAsync<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteParty, request);
        }

        public async UniTask<PartyResp> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            var result = await Client.SendRequestAsync<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateCharacterParty, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new PartyResp();
            return result.Response;
        }

        public async UniTask ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            await Client.SendRequestAsync<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterParty, request);
        }

        public async UniTask<PartyResp> ReadPartyAsync(ReadPartyReq request)
        {
            var result = await Client.SendRequestAsync<ReadPartyReq, PartyResp>(DatabaseRequestTypes.RequestReadParty, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new PartyResp();
            return result.Response;
        }

        public async UniTask<GuildResp> CreateGuildAsync(CreateGuildReq request)
        {
            var result = await Client.SendRequestAsync<CreateGuildReq, GuildResp>(DatabaseRequestTypes.RequestCreateGuild, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildLeader, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage2, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildOptionsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildOptions, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildAutoAcceptRequestsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildAutoAcceptRequests, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRole, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            var result = await Client.SendRequestAsync<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMemberRole, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask DeleteGuildAsync(DeleteGuildReq request)
        {
            await Client.SendRequestAsync<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteGuild, request);
        }

        public async UniTask<GuildResp> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            var result = await Client.SendRequestAsync<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.RequestUpdateCharacterGuild, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            await Client.SendRequestAsync<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterGuild, request);
        }

        public async UniTask<FindGuildNameResp> FindGuildNameAsync(FindGuildNameReq request)
        {
            var result = await Client.SendRequestAsync<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.RequestFindGuildName, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new FindGuildNameResp();
            return result.Response;
        }

        public async UniTask<GuildResp> ReadGuildAsync(ReadGuildReq request)
        {
            var result = await Client.SendRequestAsync<ReadGuildReq, GuildResp>(DatabaseRequestTypes.RequestReadGuild, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            var result = await Client.SendRequestAsync<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.RequestIncreaseGuildExp, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildResp> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            var result = await Client.SendRequestAsync<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.RequestAddGuildSkill, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildResp();
            return result.Response;
        }

        public async UniTask<GuildGoldResp> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            var result = await Client.SendRequestAsync<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestGetGuildGold, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildGoldResp();
            return result.Response;
        }

        public async UniTask<GuildGoldResp> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            var result = await Client.SendRequestAsync<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestChangeGuildGold, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GuildGoldResp();
            return result.Response;
        }

        public async UniTask<ReadStorageItemsResp> ReadStorageItemsAsync(ReadStorageItemsReq request)
        {
            var result = await Client.SendRequestAsync<ReadStorageItemsReq, ReadStorageItemsResp>(DatabaseRequestTypes.RequestReadStorageItems, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new ReadStorageItemsResp();
            return result.Response;
        }

        public async UniTask<MoveItemToStorageResp> MoveItemToStorageAsync(MoveItemToStorageReq request)
        {
            var result = await Client.SendRequestAsync<MoveItemToStorageReq, MoveItemToStorageResp>(DatabaseRequestTypes.RequestMoveItemToStorage, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new MoveItemToStorageResp();
            return result.Response;
        }

        public async UniTask<MoveItemFromStorageResp> MoveItemFromStorageAsync(MoveItemFromStorageReq request)
        {
            var result = await Client.SendRequestAsync<MoveItemFromStorageReq, MoveItemFromStorageResp>(DatabaseRequestTypes.RequestMoveItemFromStorage, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new MoveItemFromStorageResp();
            return result.Response;
        }

        public async UniTask<SwapOrMergeStorageItemResp> SwapOrMergeStorageItemAsync(SwapOrMergeStorageItemReq request)
        {
            var result = await Client.SendRequestAsync<SwapOrMergeStorageItemReq, SwapOrMergeStorageItemResp>(DatabaseRequestTypes.RequestSwapOrMergeStorageItem, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SwapOrMergeStorageItemResp();
            return result.Response;
        }

        public async UniTask<IncreaseStorageItemsResp> IncreaseStorageItemsAsync(IncreaseStorageItemsReq request)
        {
            var result = await Client.SendRequestAsync<IncreaseStorageItemsReq, IncreaseStorageItemsResp>(DatabaseRequestTypes.RequestIncreaseStorageItems, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new IncreaseStorageItemsResp();
            return result.Response;
        }

        public async UniTask<DecreaseStorageItemsResp> DecreaseStorageItemsAsync(DecreaseStorageItemsReq request)
        {
            var result = await Client.SendRequestAsync<DecreaseStorageItemsReq, DecreaseStorageItemsResp>(DatabaseRequestTypes.RequestDecreaseStorageItems, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new DecreaseStorageItemsResp();
            return result.Response;
        }

        public async UniTask<MailListResp> MailListAsync(MailListReq request)
        {
            var result = await Client.SendRequestAsync<MailListReq, MailListResp>(DatabaseRequestTypes.RequestMailList, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new MailListResp();
            return result.Response;
        }

        public async UniTask<UpdateReadMailStateResp> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            var result = await Client.SendRequestAsync<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.RequestUpdateReadMailState, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new UpdateReadMailStateResp();
            return result.Response;
        }

        public async UniTask<UpdateClaimMailItemsStateResp> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            var result = await Client.SendRequestAsync<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.RequestUpdateClaimMailItemsState, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new UpdateClaimMailItemsStateResp();
            return result.Response;
        }

        public async UniTask<UpdateDeleteMailStateResp> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            var result = await Client.SendRequestAsync<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.RequestUpdateDeleteMailState, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new UpdateDeleteMailStateResp();
            return result.Response;
        }

        public async UniTask<SendMailResp> SendMailAsync(SendMailReq request)
        {
            var result = await Client.SendRequestAsync<SendMailReq, SendMailResp>(DatabaseRequestTypes.RequestSendMail, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new SendMailResp();
            return result.Response;
        }

        public async UniTask<GetMailResp> GetMailAsync(GetMailReq request)
        {
            var result = await Client.SendRequestAsync<GetMailReq, GetMailResp>(DatabaseRequestTypes.RequestGetMail, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetMailResp();
            return result.Response;
        }

        public async UniTask<GetIdByCharacterNameResp> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            var result = await Client.SendRequestAsync<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetIdByCharacterName, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetIdByCharacterNameResp();
            return result.Response;
        }

        public async UniTask<GetUserIdByCharacterNameResp> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            var result = await Client.SendRequestAsync<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetUserIdByCharacterName, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetUserIdByCharacterNameResp();
            return result.Response;
        }

        public async UniTask<GetMailNotificationCountResp> GetMailsCountAsync(GetMailNotificationCountReq request)
        {
            var result = await Client.SendRequestAsync<GetMailNotificationCountReq, GetMailNotificationCountResp>(DatabaseRequestTypes.RequestGetMailNotificationCount, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetMailNotificationCountResp();
            return result.Response;
        }

        public async UniTask<GetUserUnbanTimeResp> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            var result = await Client.SendRequestAsync<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(DatabaseRequestTypes.RequestGetUserUnbanTime, request);
            if (result.ResponseCode != AckResponseCode.Success)
                return new GetUserUnbanTimeResp();
            return result.Response;
        }

        public async UniTask SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            await Client.SendRequestAsync<SetUserUnbanTimeByCharacterNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetUserUnbanTimeByCharacterName, request);
        }

        public async UniTask SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            await Client.SendRequestAsync<SetCharacterUnmuteTimeByNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetCharacterUnmuteTimeByName, request);
        }
    }
}