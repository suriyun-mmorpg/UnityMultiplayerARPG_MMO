using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager : IDatabaseClient
    {
        public async UniTask<AsyncResponseData<ValidateUserLoginResp>> ValidateUserLoginAsync(ValidateUserLoginReq request)
        {
            var resp = await Client.SendRequestAsync<ValidateUserLoginReq, ValidateUserLoginResp>(DatabaseRequestTypes.RequestValidateUserLogin, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ValidateUserLoginAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<ValidateAccessTokenResp>> ValidateAccessTokenAsync(ValidateAccessTokenReq request)
        {
            var resp = await Client.SendRequestAsync<ValidateAccessTokenReq, ValidateAccessTokenResp>(DatabaseRequestTypes.RequestValidateAccessToken, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ValidateAccessTokenAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetUserLevelResp>> GetUserLevelAsync(GetUserLevelReq request)
        {
            var resp = await Client.SendRequestAsync<GetUserLevelReq, GetUserLevelResp>(DatabaseRequestTypes.RequestGetUserLevel, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetUserLevelAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GoldResp>> GetGoldAsync(GetGoldReq request)
        {
            var resp = await Client.SendRequestAsync<GetGoldReq, GoldResp>(DatabaseRequestTypes.RequestGetGold, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetGoldAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GoldResp>> ChangeGoldAsync(ChangeGoldReq request)
        {
            var resp = await Client.SendRequestAsync<ChangeGoldReq, GoldResp>(DatabaseRequestTypes.RequestChangeGold, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ChangeGoldAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CashResp>> GetCashAsync(GetCashReq request)
        {
            var resp = await Client.SendRequestAsync<GetCashReq, CashResp>(DatabaseRequestTypes.RequestGetCash, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetCashAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CashResp>> ChangeCashAsync(ChangeCashReq request)
        {
            var resp = await Client.SendRequestAsync<ChangeCashReq, CashResp>(DatabaseRequestTypes.RequestChangeCash, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ChangeCashAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> UpdateAccessTokenAsync(UpdateAccessTokenReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateAccessTokenReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateAccessToken, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateAccessTokenAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> CreateUserLoginAsync(CreateUserLoginReq request)
        {
            var resp = await Client.SendRequestAsync<CreateUserLoginReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateUserLogin, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreateUserLoginAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<FindUsernameResp>> FindUsernameAsync(FindUsernameReq request)
        {
            var resp = await Client.SendRequestAsync<FindUsernameReq, FindUsernameResp>(DatabaseRequestTypes.RequestFindUsername, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(FindUsernameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CharacterResp>> CreateCharacterAsync(CreateCharacterReq request)
        {
            var resp = await Client.SendRequestAsync<CreateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestCreateCharacter, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreateCharacterAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CharacterResp>> ReadCharacterAsync(ReadCharacterReq request)
        {
            var resp = await Client.SendRequestAsync<ReadCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestReadCharacter, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadCharacterAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CharactersResp>> ReadCharactersAsync(ReadCharactersReq request)
        {
            var resp = await Client.SendRequestAsync<ReadCharactersReq, CharactersResp>(DatabaseRequestTypes.RequestReadCharacters, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadCharactersAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<CharacterResp>> UpdateCharacterAsync(UpdateCharacterReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateCharacterReq, CharacterResp>(DatabaseRequestTypes.RequestUpdateCharacter, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateCharacterAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> DeleteCharacterAsync(DeleteCharacterReq request)
        {
            var resp = await Client.SendRequestAsync<DeleteCharacterReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteCharacter, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(DeleteCharacterAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<FindCharacterNameResp>> FindCharacterNameAsync(FindCharacterNameReq request)
        {
            var resp = await Client.SendRequestAsync<FindCharacterNameReq, FindCharacterNameResp>(DatabaseRequestTypes.RequestFindCharacterName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(FindCharacterNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> FindCharactersAsync(FindCharacterNameReq request)
        {
            var resp = await Client.SendRequestAsync<FindCharacterNameReq, SocialCharactersResp>(DatabaseRequestTypes.RequestFindCharacters, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(FindCharactersAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> CreateFriendAsync(CreateFriendReq request)
        {
            var resp = await Client.SendRequestAsync<CreateFriendReq, EmptyMessage>(DatabaseRequestTypes.RequestCreateFriend, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreateFriendAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> DeleteFriendAsync(DeleteFriendReq request)
        {
            var resp = await Client.SendRequestAsync<DeleteFriendReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteFriend, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(DeleteFriendAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<SocialCharactersResp>> ReadFriendsAsync(ReadFriendsReq request)
        {
            var resp = await Client.SendRequestAsync<ReadFriendsReq, SocialCharactersResp>(DatabaseRequestTypes.RequestReadFriends, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadFriendsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<BuildingResp>> CreateBuildingAsync(CreateBuildingReq request)
        {
            var resp = await Client.SendRequestAsync<CreateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestCreateBuilding, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreateBuildingAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<BuildingResp>> UpdateBuildingAsync(UpdateBuildingReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateBuildingReq, BuildingResp>(DatabaseRequestTypes.RequestUpdateBuilding, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateBuildingAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> DeleteBuildingAsync(DeleteBuildingReq request)
        {
            var resp = await Client.SendRequestAsync<DeleteBuildingReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteBuilding, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(DeleteBuildingAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<BuildingsResp>> ReadBuildingsAsync(ReadBuildingsReq request)
        {
            var resp = await Client.SendRequestAsync<ReadBuildingsReq, BuildingsResp>(DatabaseRequestTypes.RequestReadBuildings, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadBuildingsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<PartyResp>> CreatePartyAsync(CreatePartyReq request)
        {
            var resp = await Client.SendRequestAsync<CreatePartyReq, PartyResp>(DatabaseRequestTypes.RequestCreateParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreatePartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<PartyResp>> UpdatePartyAsync(UpdatePartyReq request)
        {
            var resp = await Client.SendRequestAsync<UpdatePartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdatePartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<PartyResp>> UpdatePartyLeaderAsync(UpdatePartyLeaderReq request)
        {
            var resp = await Client.SendRequestAsync<UpdatePartyLeaderReq, PartyResp>(DatabaseRequestTypes.RequestUpdatePartyLeader, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdatePartyLeaderAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> DeletePartyAsync(DeletePartyReq request)
        {
            var resp = await Client.SendRequestAsync<DeletePartyReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(DeletePartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<PartyResp>> UpdateCharacterPartyAsync(UpdateCharacterPartyReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateCharacterPartyReq, PartyResp>(DatabaseRequestTypes.RequestUpdateCharacterParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateCharacterPartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterPartyAsync(ClearCharacterPartyReq request)
        {
            var resp = await Client.SendRequestAsync<ClearCharacterPartyReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ClearCharacterPartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<PartyResp>> ReadPartyAsync(ReadPartyReq request)
        {
            var resp = await Client.SendRequestAsync<ReadPartyReq, PartyResp>(DatabaseRequestTypes.RequestReadParty, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadPartyAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> CreateGuildAsync(CreateGuildReq request)
        {
            var resp = await Client.SendRequestAsync<CreateGuildReq, GuildResp>(DatabaseRequestTypes.RequestCreateGuild, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(CreateGuildAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildLeaderAsync(UpdateGuildLeaderReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildLeaderReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildLeader, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildLeaderAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessageAsync(UpdateGuildMessageReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildMessageAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildMessage2Async(UpdateGuildMessageReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildMessageReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMessage2, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildMessage2Async)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildOptionsAsync(UpdateGuildOptionsReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildOptionsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildOptions, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildOptionsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildAutoAcceptRequestsAsync(UpdateGuildAutoAcceptRequestsReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildAutoAcceptRequestsReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildAutoAcceptRequests, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildAutoAcceptRequestsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildRoleAsync(UpdateGuildRoleReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildRole, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildRoleAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateGuildMemberRoleAsync(UpdateGuildMemberRoleReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateGuildMemberRoleReq, GuildResp>(DatabaseRequestTypes.RequestUpdateGuildMemberRole, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateGuildMemberRoleAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> DeleteGuildAsync(DeleteGuildReq request)
        {
            var resp = await Client.SendRequestAsync<DeleteGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestDeleteGuild, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(DeleteGuildAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> UpdateCharacterGuildAsync(UpdateCharacterGuildReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateCharacterGuildReq, GuildResp>(DatabaseRequestTypes.RequestUpdateCharacterGuild, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateCharacterGuildAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> ClearCharacterGuildAsync(ClearCharacterGuildReq request)
        {
            var resp = await Client.SendRequestAsync<ClearCharacterGuildReq, EmptyMessage>(DatabaseRequestTypes.RequestClearCharacterGuild, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ClearCharacterGuildAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<FindGuildNameResp>> FindGuildNameAsync(FindGuildNameReq request)
        {
            var resp = await Client.SendRequestAsync<FindGuildNameReq, FindGuildNameResp>(DatabaseRequestTypes.RequestFindGuildName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(FindGuildNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> ReadGuildAsync(ReadGuildReq request)
        {
            var resp = await Client.SendRequestAsync<ReadGuildReq, GuildResp>(DatabaseRequestTypes.RequestReadGuild, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadGuildAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> IncreaseGuildExpAsync(IncreaseGuildExpReq request)
        {
            var resp = await Client.SendRequestAsync<IncreaseGuildExpReq, GuildResp>(DatabaseRequestTypes.RequestIncreaseGuildExp, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(IncreaseGuildExpAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildResp>> AddGuildSkillAsync(AddGuildSkillReq request)
        {
            var resp = await Client.SendRequestAsync<AddGuildSkillReq, GuildResp>(DatabaseRequestTypes.RequestAddGuildSkill, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(AddGuildSkillAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildGoldResp>> GetGuildGoldAsync(GetGuildGoldReq request)
        {
            var resp = await Client.SendRequestAsync<GetGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestGetGuildGold, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetGuildGoldAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GuildGoldResp>> ChangeGuildGoldAsync(ChangeGuildGoldReq request)
        {
            var resp = await Client.SendRequestAsync<ChangeGuildGoldReq, GuildGoldResp>(DatabaseRequestTypes.RequestChangeGuildGold, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ChangeGuildGoldAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<ReadStorageItemsResp>> ReadStorageItemsAsync(ReadStorageItemsReq request)
        {
            var resp = await Client.SendRequestAsync<ReadStorageItemsReq, ReadStorageItemsResp>(DatabaseRequestTypes.RequestReadStorageItems, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ReadStorageItemsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> UpdateStorageItemsAsync(UpdateStorageItemsReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateStorageItemsReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateStorageItems, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateStorageItemsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<MailListResp>> MailListAsync(MailListReq request)
        {
            var resp = await Client.SendRequestAsync<MailListReq, MailListResp>(DatabaseRequestTypes.RequestMailList, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(MailListAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<UpdateReadMailStateResp>> UpdateReadMailStateAsync(UpdateReadMailStateReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateReadMailStateReq, UpdateReadMailStateResp>(DatabaseRequestTypes.RequestUpdateReadMailState, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateReadMailStateAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<UpdateClaimMailItemsStateResp>> UpdateClaimMailItemsStateAsync(UpdateClaimMailItemsStateReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateClaimMailItemsStateReq, UpdateClaimMailItemsStateResp>(DatabaseRequestTypes.RequestUpdateClaimMailItemsState, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateClaimMailItemsStateAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<UpdateDeleteMailStateResp>> UpdateDeleteMailStateAsync(UpdateDeleteMailStateReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateDeleteMailStateReq, UpdateDeleteMailStateResp>(DatabaseRequestTypes.RequestUpdateDeleteMailState, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateDeleteMailStateAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<SendMailResp>> SendMailAsync(SendMailReq request)
        {
            var resp = await Client.SendRequestAsync<SendMailReq, SendMailResp>(DatabaseRequestTypes.RequestSendMail, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(SendMailAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetMailResp>> GetMailAsync(GetMailReq request)
        {
            var resp = await Client.SendRequestAsync<GetMailReq, GetMailResp>(DatabaseRequestTypes.RequestGetMail, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetMailAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetIdByCharacterNameResp>> GetIdByCharacterNameAsync(GetIdByCharacterNameReq request)
        {
            var resp = await Client.SendRequestAsync<GetIdByCharacterNameReq, GetIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetIdByCharacterName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetIdByCharacterNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetUserIdByCharacterNameResp>> GetUserIdByCharacterNameAsync(GetUserIdByCharacterNameReq request)
        {
            var resp = await Client.SendRequestAsync<GetUserIdByCharacterNameReq, GetUserIdByCharacterNameResp>(DatabaseRequestTypes.RequestGetUserIdByCharacterName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetUserIdByCharacterNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetMailNotificationResp>> GetMailNotificationAsync(GetMailNotificationReq request)
        {
            var resp = await Client.SendRequestAsync<GetMailNotificationReq, GetMailNotificationResp>(DatabaseRequestTypes.RequestGetMailNotification, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetMailNotificationAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetUserUnbanTimeResp>> GetUserUnbanTimeAsync(GetUserUnbanTimeReq request)
        {
            var resp = await Client.SendRequestAsync<GetUserUnbanTimeReq, GetUserUnbanTimeResp>(DatabaseRequestTypes.RequestGetUserUnbanTime, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetUserUnbanTimeAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> SetUserUnbanTimeByCharacterNameAsync(SetUserUnbanTimeByCharacterNameReq request)
        {
            var resp = await Client.SendRequestAsync<SetUserUnbanTimeByCharacterNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetUserUnbanTimeByCharacterName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(SetUserUnbanTimeByCharacterNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> SetCharacterUnmuteTimeByNameAsync(SetCharacterUnmuteTimeByNameReq request)
        {
            var resp = await Client.SendRequestAsync<SetCharacterUnmuteTimeByNameReq, EmptyMessage>(DatabaseRequestTypes.RequestSetCharacterUnmuteTimeByName, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(SetCharacterUnmuteTimeByNameAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetSummonBuffsResp>> GetSummonBuffsAsync(GetSummonBuffsReq request)
        {
            var resp = await Client.SendRequestAsync<GetSummonBuffsReq, GetSummonBuffsResp>(DatabaseRequestTypes.RequestGetSummonBuffs, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetSummonBuffsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> SetSummonBuffsAsync(SetSummonBuffsReq request)
        {
            var resp = await Client.SendRequestAsync<SetSummonBuffsReq, EmptyMessage>(DatabaseRequestTypes.RequestSetSummonBuffs, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(SetSummonBuffsAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<ValidateEmailVerificationResp>> ValidateEmailVerificationAsync(ValidateEmailVerificationReq request)
        {
            var resp = await Client.SendRequestAsync<ValidateEmailVerificationReq, ValidateEmailVerificationResp>(DatabaseRequestTypes.RequestValidateEmailVerification, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(ValidateEmailVerificationAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<FindEmailResp>> FindEmailAsync(FindEmailReq request)
        {
            var resp = await Client.SendRequestAsync<FindEmailReq, FindEmailResp>(DatabaseRequestTypes.RequestFindEmail, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(FindEmailAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<GetFriendRequestNotificationResp>> GetFriendRequestNotificationAsync(GetFriendRequestNotificationReq request)
        {
            var resp = await Client.SendRequestAsync<GetFriendRequestNotificationReq, GetFriendRequestNotificationResp>(DatabaseRequestTypes.RequestGetFriendRequestNotification, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(GetFriendRequestNotificationAsync)} status: {resp.ResponseCode}");
            return resp;
        }

        public async UniTask<AsyncResponseData<EmptyMessage>> UpdateUserCount(UpdateUserCountReq request)
        {
            var resp = await Client.SendRequestAsync<UpdateUserCountReq, EmptyMessage>(DatabaseRequestTypes.RequestUpdateUserCount, request);
            if (!resp.IsSuccess)
                Logging.LogError(nameof(DatabaseNetworkManager), $"Cannot {nameof(UpdateUserCount)} status: {resp.ResponseCode}");
            return resp;
        }
    }
}
