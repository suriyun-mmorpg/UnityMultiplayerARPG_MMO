using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class DatabaseServiceImplement : DatabaseService.DatabaseServiceBase
    {
        // TODO: I'm going to make in-memory database without Redis for now
        // In the future it may implements Redis
        // It's going to get some data from all tables but not every records
        // Just some records that players were requested

        public override Task<ValidateUserLoginResp> ValidateUserLogin(ValidateUserLoginReq request, ServerCallContext context)
        {
            return base.ValidateUserLogin(request, context);
        }

        public override Task<ValidateAccessTokenResp> ValidateAccessToken(ValidateAccessTokenReq request, ServerCallContext context)
        {
            return base.ValidateAccessToken(request, context);
        }

        public override Task<GetUserLevelResp> GetUserLevel(GetUserLevelReq request, ServerCallContext context)
        {
            return base.GetUserLevel(request, context);
        }

        public override Task<GoldResp> GetGold(GetGoldReq request, ServerCallContext context)
        {
            return base.GetGold(request, context);
        }

        public override Task<GoldResp> IncreaseGold(IncreaseGoldReq request, ServerCallContext context)
        {
            return base.IncreaseGold(request, context);
        }

        public override Task<GoldResp> DecreaseGold(DecreaseGoldReq request, ServerCallContext context)
        {
            return base.DecreaseGold(request, context);
        }

        public override Task<CashResp> GetCash(GetCashReq request, ServerCallContext context)
        {
            return base.GetCash(request, context);
        }

        public override Task<CashResp> IncreaseCash(IncreaseCashReq request, ServerCallContext context)
        {
            return base.IncreaseCash(request, context);
        }

        public override Task<CashResp> DecreaseCash(DecreaseCashReq request, ServerCallContext context)
        {
            return base.DecreaseCash(request, context);
        }

        public override Task<VoidResp> UpdateAccessToken(UpdateAccessTokenReq request, ServerCallContext context)
        {
            return base.UpdateAccessToken(request, context);
        }

        public override Task<VoidResp> CreateUserLogin(CreateUserLoginReq request, ServerCallContext context)
        {
            return base.CreateUserLogin(request, context);
        }

        public override Task<FindUsernameResp> FindUsername(FindUsernameReq request, ServerCallContext context)
        {
            return base.FindUsername(request, context);
        }

        public override Task<VoidResp> CreateCharacter(CreateCharacterReq request, ServerCallContext context)
        {
            return base.CreateCharacter(request, context);
        }

        public override Task<ReadCharacterResp> ReadCharacter(ReadCharacterReq request, ServerCallContext context)
        {
            return base.ReadCharacter(request, context);
        }

        public override Task<ReadCharactersResp> ReadCharacters(ReadCharactersReq request, ServerCallContext context)
        {
            return base.ReadCharacters(request, context);
        }

        public override Task<VoidResp> UpdateCharacter(UpdateCharacterReq request, ServerCallContext context)
        {
            return base.UpdateCharacter(request, context);
        }

        public override Task<VoidResp> DeleteCharacter(DeleteCharacterReq request, ServerCallContext context)
        {
            return base.DeleteCharacter(request, context);
        }

        public override Task<FindCharacterNameResp> FindCharacterName(FindCharacterNameReq request, ServerCallContext context)
        {
            return base.FindCharacterName(request, context);
        }

        public override Task<FindCharactersResp> FindCharacters(FindCharactersReq request, ServerCallContext context)
        {
            return base.FindCharacters(request, context);
        }

        public override Task<VoidResp> CreateFriend(CreateFriendReq request, ServerCallContext context)
        {
            return base.CreateFriend(request, context);
        }

        public override Task<VoidResp> DeleteFriend(DeleteFriendReq request, ServerCallContext context)
        {
            return base.DeleteFriend(request, context);
        }

        public override Task<ReadFriendsResp> ReadFriends(ReadFriendsReq request, ServerCallContext context)
        {
            return base.ReadFriends(request, context);
        }

        public override Task<VoidResp> CreateBuilding(CreateBuildingReq request, ServerCallContext context)
        {
            return base.CreateBuilding(request, context);
        }

        public override Task<VoidResp> UpdateBuilding(UpdateBuildingReq request, ServerCallContext context)
        {
            return base.UpdateBuilding(request, context);
        }

        public override Task<VoidResp> DeleteBuilding(DeleteBuildingReq request, ServerCallContext context)
        {
            return base.DeleteBuilding(request, context);
        }

        public override Task<ReadBuildingsResp> ReadBuildings(ReadBuildingsReq request, ServerCallContext context)
        {
            return base.ReadBuildings(request, context);
        }

        public override Task<CreatePartyResp> CreateParty(CreatePartyReq request, ServerCallContext context)
        {
            return base.CreateParty(request, context);
        }

        public override Task<VoidResp> UpdateParty(UpdatePartyReq request, ServerCallContext context)
        {
            return base.UpdateParty(request, context);
        }

        public override Task<VoidResp> UpdatePartyLeader(UpdatePartyLeaderReq request, ServerCallContext context)
        {
            return base.UpdatePartyLeader(request, context);
        }

        public override Task<VoidResp> DeleteParty(DeletePartyReq request, ServerCallContext context)
        {
            return base.DeleteParty(request, context);
        }

        public override Task<VoidResp> UpdateCharacterParty(UpdateCharacterPartyReq request, ServerCallContext context)
        {
            return base.UpdateCharacterParty(request, context);
        }

        public override Task<ReadPartyResp> ReadParty(ReadPartyReq request, ServerCallContext context)
        {
            return base.ReadParty(request, context);
        }

        public override Task<CreateGuildResp> CreateGuild(CreateGuildReq request, ServerCallContext context)
        {
            return base.CreateGuild(request, context);
        }

        public override Task<IncreaseGuildExpResp> IncreaseGuildExp(IncreaseGuildExpReq request, ServerCallContext context)
        {
            return base.IncreaseGuildExp(request, context);
        }

        public override Task<VoidResp> UpdateGuildLeader(UpdateGuildLeaderReq request, ServerCallContext context)
        {
            return base.UpdateGuildLeader(request, context);
        }

        public override Task<VoidResp> UpdateGuildMessage(UpdateGuildMessageReq request, ServerCallContext context)
        {
            return base.UpdateGuildMessage(request, context);
        }

        public override Task<VoidResp> UpdateGuildRole(UpdateGuildRoleReq request, ServerCallContext context)
        {
            return base.UpdateGuildRole(request, context);
        }

        public override Task<VoidResp> UpdateGuildMemberRole(UpdateGuildMemberRoleReq request, ServerCallContext context)
        {
            return base.UpdateGuildMemberRole(request, context);
        }

        public override Task<VoidResp> UpdateGuildSkillLevel(UpdateGuildSkillLevelReq request, ServerCallContext context)
        {
            return base.UpdateGuildSkillLevel(request, context);
        }

        public override Task<VoidResp> UpdateCharacterGuild(UpdateCharacterGuildReq request, ServerCallContext context)
        {
            return base.UpdateCharacterGuild(request, context);
        }

        public override Task<VoidResp> DeleteGuild(DeleteGuildReq request, ServerCallContext context)
        {
            return base.DeleteGuild(request, context);
        }

        public override Task<FindGuildNameResp> FindGuildName(FindGuildNameReq request, ServerCallContext context)
        {
            return base.FindGuildName(request, context);
        }

        public override Task<ReadGuildResp> ReadGuild(ReadGuildReq request, ServerCallContext context)
        {
            return base.ReadGuild(request, context);
        }

        public override Task<GuildGoldResp> GetGuildGold(GetGuildGoldReq request, ServerCallContext context)
        {
            return base.GetGuildGold(request, context);
        }

        public override Task<GuildGoldResp> IncreaseGuildGold(IncreaseGuildGoldReq request, ServerCallContext context)
        {
            return base.IncreaseGuildGold(request, context);
        }

        public override Task<GuildGoldResp> DecreaseGuildGold(DecreaseGuildGoldReq request, ServerCallContext context)
        {
            return base.DecreaseGuildGold(request, context);
        }

        public override Task<VoidResp> UpdateStorageItems(UpdateStorageItemsReq request, ServerCallContext context)
        {
            return base.UpdateStorageItems(request, context);
        }

        public override Task<ReadStorageItemsResp> ReadStorageItems(ReadStorageItemsReq request, ServerCallContext context)
        {
            return base.ReadStorageItems(request, context);
        }
    }
}
