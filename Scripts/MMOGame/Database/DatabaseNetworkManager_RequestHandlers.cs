using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        protected async UniTaskVoid ValidateUserLogin(RequestHandlerData requestHandler, ValidateUserLoginReq request, RequestProceedResultDelegate<ValidateUserLoginResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            string userId = Database.ValidateUserLogin(request.Username, request.Password);
            if (string.IsNullOrEmpty(userId))
            {
                result.Invoke(AckResponseCode.Success, new ValidateUserLoginResp());
                return;
            }
            result.Invoke(AckResponseCode.Success, new ValidateUserLoginResp()
            {
                UserId = userId,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ValidateAccessToken(RequestHandlerData requestHandler, ValidateAccessTokenReq request, RequestProceedResultDelegate<ValidateAccessTokenResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new ValidateAccessTokenResp()
            {
                IsPass = ValidateAccessToken(request.UserId, request.AccessToken),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserLevel(RequestHandlerData requestHandler, GetUserLevelReq request, RequestProceedResultDelegate<GetUserLevelResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetUserLevelResp()
            {
                UserLevel = Database.GetUserLevel(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetGold(RequestHandlerData requestHandler, GetGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GoldResp()
            {
                Gold = ReadGold(request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ChangeGold(RequestHandlerData requestHandler, ChangeGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            int gold = ReadGold(request.UserId);
            gold += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserGold[request.UserId] = gold;
            // Update data to database
            Database.UpdateGold(request.UserId, gold);
            result.Invoke(AckResponseCode.Success, new GoldResp()
            {
                Gold = gold
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetCash(RequestHandlerData requestHandler, GetCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new CashResp()
            {
                Cash = ReadCash(request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ChangeCash(RequestHandlerData requestHandler, ChangeCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            int cash = ReadCash(request.UserId);
            cash += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserCash[request.UserId] = cash;
            // Update data to database
            Database.UpdateCash(request.UserId, cash);
            result.Invoke(AckResponseCode.Success, new CashResp()
            {
                Cash = cash
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateAccessToken(RequestHandlerData requestHandler, UpdateAccessTokenReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Store access token to the dictionary, it will be used to validate later
            cachedUserAccessToken[request.UserId] = request.AccessToken;
            // Update data to database
            Database.UpdateAccessToken(request.UserId, request.AccessToken);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateUserLogin(RequestHandlerData requestHandler, CreateUserLoginReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Cache username, it will be used to validate later
            cachedUsernames.Add(request.Username);
            // Insert new user login to database
            Database.CreateUserLogin(request.Username, request.Password, request.Email);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindUsername(RequestHandlerData requestHandler, FindUsernameReq request, RequestProceedResultDelegate<FindUsernameResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new FindUsernameResp()
            {
                FoundAmount = FindUsername(request.Username),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateCharacter(RequestHandlerData requestHandler, CreateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PlayerCharacterData character = request.CharacterData;
            // Insert new character to database
            Database.CreateCharacter(request.UserId, character);
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = character
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadCharacter(RequestHandlerData requestHandler, ReadCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = ReadCharacterWithUserIdValidation(request.CharacterId, request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadCharacters(RequestHandlerData requestHandler, ReadCharactersReq request, RequestProceedResultDelegate<CharactersResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            List<PlayerCharacterData> characters = Database.ReadCharacters(request.UserId);
            // Read and cache character (or load from cache)
            long lastUpdate;
            for (int i = 0; i < characters.Count; ++i)
            {
                lastUpdate = characters[i].LastUpdate;
                characters[i] = ReadCharacter(characters[i].Id);
                characters[i].LastUpdate = lastUpdate;
            }
            result.Invoke(AckResponseCode.Success, new CharactersResp()
            {
                List = characters
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateCharacter(RequestHandlerData requestHandler, UpdateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PlayerCharacterData character = request.CharacterData;
            // Cache the data, it will be used later
            cachedUserCharacter[character.Id] = character;
            // Response success immediately
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = character
            });
            await UniTask.Yield();
            // Update data to database
            Database.UpdateCharacter(character);
#endif
        }

        protected async UniTaskVoid DeleteCharacter(RequestHandlerData requestHandler, DeleteCharacterReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Remove data from cache
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
            {
                string characterName = cachedUserCharacter[request.CharacterId].CharacterName;
                cachedCharacterNames.TryRemove(characterName);
                cachedUserCharacter.TryRemove(request.CharacterId, out _);
            }
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
            // Delete data from database
            Database.DeleteCharacter(request.UserId, request.CharacterId);
#endif
        }

        protected async UniTaskVoid FindCharacterName(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<FindCharacterNameResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new FindCharacterNameResp()
            {
                FoundAmount = FindCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindCharacters(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = Database.FindCharacters(request.FinderId, request.CharacterName, request.Skip, request.Limit)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateFriend(RequestHandlerData requestHandler, CreateFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.CreateFriend(request.Character1Id, request.Character2Id, request.State);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteFriend(RequestHandlerData requestHandler, DeleteFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.DeleteFriend(request.Character1Id, request.Character2Id);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadFriends(RequestHandlerData requestHandler, ReadFriendsReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = Database.ReadFriends(request.CharacterId, request.ReadById2, request.State, request.Skip, request.Limit),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateBuilding(RequestHandlerData requestHandler, CreateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            BuildingSaveData building = request.BuildingData;
            // Cache building data
            if (cachedBuilding.ContainsKey(request.MapName))
            {
                if (cachedBuilding[request.MapName].ContainsKey(building.Id))
                    cachedBuilding[request.MapName][building.Id] = building;
                else
                    cachedBuilding[request.MapName].TryAdd(building.Id, building);
            }
            // Insert data to database
            Database.CreateBuilding(request.MapName, building);
            result.Invoke(AckResponseCode.Success, new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateBuilding(RequestHandlerData requestHandler, UpdateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            BuildingSaveData building = request.BuildingData;
            // Cache building data
            if (cachedBuilding.ContainsKey(request.MapName))
            {
                if (cachedBuilding[request.MapName].ContainsKey(building.Id))
                    cachedBuilding[request.MapName][building.Id] = building;
                else
                    cachedBuilding[request.MapName].TryAdd(building.Id, building);
            }
            // Update data to database
            Database.UpdateBuilding(request.MapName, building);
            result.Invoke(AckResponseCode.Success, new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteBuilding(RequestHandlerData requestHandler, DeleteBuildingReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Remove from cache
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName].TryRemove(request.BuildingId, out _);
            // Remove from database
            Database.DeleteBuilding(request.MapName, request.BuildingId);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadBuildings(RequestHandlerData requestHandler, ReadBuildingsReq request, RequestProceedResultDelegate<BuildingsResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new BuildingsResp()
            {
                List = ReadBuildings(request.MapName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateParty(RequestHandlerData requestHandler, CreatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Insert to database
            int partyId = Database.CreateParty(request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            // Cached the data
            PartyData party = new PartyData(partyId, request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            cachedParty[partyId] = party;
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateParty(RequestHandlerData requestHandler, UpdatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PartyData party = ReadParty(request.PartyId);
            if (party == null)
            {
                result.Invoke(AckResponseCode.Error, new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            // Update to cache
            party.Setting(request.ShareExp, request.ShareItem);
            cachedParty[request.PartyId] = party;
            // Update to database
            Database.UpdateParty(request.PartyId, request.ShareExp, request.ShareItem);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdatePartyLeader(RequestHandlerData requestHandler, UpdatePartyLeaderReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PartyData party = ReadParty(request.PartyId);
            if (party == null)
            {
                result.Invoke(AckResponseCode.Error, new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            // Update to cache
            party.SetLeader(request.LeaderCharacterId);
            cachedParty[request.PartyId] = party;
            // Update to database
            Database.UpdatePartyLeader(request.PartyId, request.LeaderCharacterId);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteParty(RequestHandlerData requestHandler, DeletePartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.DeleteParty(request.PartyId);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateCharacterParty(RequestHandlerData requestHandler, UpdateCharacterPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PartyData party = ReadParty(request.PartyId);
            if (party == null)
            {
                result.Invoke(AckResponseCode.Error, new PartyResp()
                {
                    PartyData = null
                });
                return;
            }
            // Update to cache
            SocialCharacterData character = request.SocialCharacterData;
            party.AddMember(character);
            cachedParty[request.PartyId] = party;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].PartyId = request.PartyId;
            // Update to database
            Database.UpdateCharacterParty(character.id, request.PartyId);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ClearCharacterParty(RequestHandlerData requestHandler, ClearCharacterPartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PlayerCharacterData character = ReadCharacter(request.CharacterId);
            if (character == null)
            {
                result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
                return;
            }
            PartyData party = ReadParty(character.PartyId);
            if (party == null)
            {
                result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
                return;
            }
            // Update to cache
            party.RemoveMember(request.CharacterId);
            cachedParty[character.PartyId] = party;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
                cachedUserCharacter[request.CharacterId].PartyId = 0;
            // Update to database
            Database.UpdateCharacterParty(request.CharacterId, 0);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadParty(RequestHandlerData requestHandler, ReadPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = ReadParty(request.PartyId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateGuild(RequestHandlerData requestHandler, CreateGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Insert to database
            int guildId = Database.CreateGuild(request.GuildName, request.LeaderCharacterId);
            // Cached the data
            GuildData guild = new GuildData(guildId, request.GuildName, request.LeaderCharacterId);
            cachedGuild[guildId] = guild;
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildLeader(RequestHandlerData requestHandler, UpdateGuildLeaderReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.SetLeader(request.LeaderCharacterId);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildLeader(request.GuildId, request.LeaderCharacterId);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage(RequestHandlerData requestHandler, UpdateGuildMessageReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.guildMessage = request.GuildMessage;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildMessage(request.GuildId, request.GuildMessage);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildMessage2(RequestHandlerData requestHandler, UpdateGuildMessageReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.guildMessage2 = request.GuildMessage;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildMessage2(request.GuildId, request.GuildMessage);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildScore(RequestHandlerData requestHandler, UpdateGuildScoreReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.score = request.Score;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildScore(request.GuildId, request.Score);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildOptions(RequestHandlerData requestHandler, UpdateGuildOptionsReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.options = request.Options;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildOptions(request.GuildId, request.Options);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildAutoAcceptRequests(RequestHandlerData requestHandler, UpdateGuildAutoAcceptRequestsReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.autoAcceptRequests = request.AutoAcceptRequests;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildAutoAcceptRequests(request.GuildId, request.AutoAcceptRequests);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildRank(RequestHandlerData requestHandler, UpdateGuildRankReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.score = request.Rank;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildRank(request.GuildId, request.Rank);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildRole(RequestHandlerData requestHandler, UpdateGuildRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.SetRole(request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, request.ShareExpPercentage);
            cachedGuild[request.GuildId] = guild;
            // Update to
            Database.UpdateGuildRole(request.GuildId, request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, request.ShareExpPercentage);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateGuildMemberRole(RequestHandlerData requestHandler, UpdateGuildMemberRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            guild.SetMemberRole(request.MemberCharacterId, request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildMemberRole(request.MemberCharacterId, request.GuildRole);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteGuild(RequestHandlerData requestHandler, DeleteGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Remove data from cache
            if (cachedGuild.ContainsKey(request.GuildId))
            {
                string guildName = cachedGuild[request.GuildId].guildName;
                cachedGuildNames.TryRemove(guildName);
                cachedGuild.TryRemove(request.GuildId, out _);
            }
            // Remove data from database
            Database.DeleteGuild(request.GuildId);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateCharacterGuild(RequestHandlerData requestHandler, UpdateCharacterGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            // Update to cache
            SocialCharacterData character = request.SocialCharacterData;
            guild.AddMember(character, request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].GuildId = request.GuildId;
            // Update to database
            Database.UpdateCharacterGuild(character.id, request.GuildId, request.GuildRole);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ClearCharacterGuild(RequestHandlerData requestHandler, ClearCharacterGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            PlayerCharacterData character = ReadCharacter(request.CharacterId);
            if (character == null)
            {
                result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
                return;
            }
            GuildData guild = ReadGuild(character.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
                return;
            }
            // Update to cache
            guild.RemoveMember(request.CharacterId);
            cachedGuild[character.GuildId] = guild;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
            {
                cachedUserCharacter[request.CharacterId].GuildId = 0;
                cachedUserCharacter[request.CharacterId].GuildRole = 0;
            }
            // Update to database
            Database.UpdateCharacterGuild(request.CharacterId, 0, 0);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindGuildName(RequestHandlerData requestHandler, FindGuildNameReq request, RequestProceedResultDelegate<FindGuildNameResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new FindGuildNameResp()
            {
                FoundAmount = FindGuildName(request.GuildName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadGuild(RequestHandlerData requestHandler, ReadGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = ReadGuild(request.GuildId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid IncreaseGuildExp(RequestHandlerData requestHandler, IncreaseGuildExpReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // TODO: May validate guild by character
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            await UniTask.SwitchToMainThread();
            guild = GameInstance.Singleton.SocialSystemSetting.IncreaseGuildExp(guild, request.Exp);
            // Update to cache
            cachedGuild.TryAdd(guild.id, guild);
            // Update to database
            Database.UpdateGuildLevel(request.GuildId, guild.level, guild.exp, guild.skillPoint);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = ReadGuild(request.GuildId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid AddGuildSkill(RequestHandlerData requestHandler, AddGuildSkillReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // TODO: May validate guild by character
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildResp()
                {
                    GuildData = null
                });
                return;
            }
            await UniTask.SwitchToMainThread();
            if (!guild.IsSkillReachedMaxLevel(request.SkillId) && guild.skillPoint > 0)
            {
                guild.AddSkillLevel(request.SkillId);
                // Update to cache
                cachedGuild[guild.id] = guild;
                // Update to database
                Database.UpdateGuildSkillLevel(request.GuildId, request.SkillId, guild.GetSkillLevel(request.SkillId), guild.skillPoint);
            }
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = ReadGuild(request.GuildId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetGuildGold(RequestHandlerData requestHandler, GetGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildGoldResp()
                {
                    GuildGold = 0
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ChangeGuildGold(RequestHandlerData requestHandler, ChangeGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            GuildData guild = ReadGuild(request.GuildId);
            if (guild == null)
            {
                result.Invoke(AckResponseCode.Error, new GuildGoldResp()
                {
                    GuildGold = 0
                });
                return;
            }
            // Update to cache
            guild.gold += request.ChangeAmount;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            Database.UpdateGuildGold(request.GuildId, guild.gold);
            result.Invoke(AckResponseCode.Success, new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadStorageItems(RequestHandlerData requestHandler, ReadStorageItemsReq request, RequestProceedResultDelegate<ReadStorageItemsResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            if (request.ReadForUpdate)
            {
                float time = Time.unscaledTime;
                if (updatingStorages.TryGetValue(storageId, out float oldTime) && time - oldTime < 0.5f)
                {
                    // Not allow to update yet
                    result.Invoke(AckResponseCode.Error, new ReadStorageItemsResp());
                    return;
                }
                updatingStorages.TryRemove(storageId, out _);
                updatingStorages.TryAdd(storageId, time);
            }
            result.Invoke(AckResponseCode.Success, new ReadStorageItemsResp()
            {
                StorageCharacterItems = ReadStorageItems(storageId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateStorageItems(RequestHandlerData requestHandler, UpdateStorageItemsReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            float time = Time.unscaledTime;
            if (updatingStorages.TryGetValue(storageId, out float oldTime) && time - oldTime >= 0.5f)
            {
                // Timeout
                result.Invoke(AckResponseCode.Error, EmptyMessage.Value);
                return;
            }
            if (request.UpdateCharacterData)
            {
                PlayerCharacterData character = request.CharacterData;
                // Cache the data, it will be used later
                cachedUserCharacter[character.Id] = character;
                // Update data to database
                Database.UpdateCharacter(character);
            }
            // Cache the data, it will be used later
            cachedStorageItems[storageId] = request.StorageItems;
            // Update data to database
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, request.StorageItems);
            updatingStorages.TryRemove(storageId, out _);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid MailList(RequestHandlerData requestHandler, MailListReq request, RequestProceedResultDelegate<MailListResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new MailListResp()
            {
                List = Database.MailList(request.UserId, request.OnlyNewMails)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateReadMailState(RequestHandlerData requestHandler, UpdateReadMailStateReq request, RequestProceedResultDelegate<UpdateReadMailStateResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            long updated = Database.UpdateReadMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Error, new UpdateReadMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new UpdateReadMailStateResp()
            {
                Mail = Database.GetMail(request.MailId, request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateClaimMailItemsState(RequestHandlerData requestHandler, UpdateClaimMailItemsStateReq request, RequestProceedResultDelegate<UpdateClaimMailItemsStateResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            long updated = Database.UpdateClaimMailItemsState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Error, new UpdateClaimMailItemsStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new UpdateClaimMailItemsStateResp()
            {
                Mail = Database.GetMail(request.MailId, request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateDeleteMailState(RequestHandlerData requestHandler, UpdateDeleteMailStateReq request, RequestProceedResultDelegate<UpdateDeleteMailStateResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            long updated = Database.UpdateDeleteMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Error, new UpdateDeleteMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new UpdateDeleteMailStateResp());
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SendMail(RequestHandlerData requestHandler, SendMailReq request, RequestProceedResultDelegate<SendMailResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Mail mail = request.Mail;
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                result.Invoke(AckResponseCode.Error, new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER
                });
                return;
            }
            long created = Database.CreateMail(mail);
            if (created <= 0)
            {
                result.Invoke(AckResponseCode.Error, new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new SendMailResp());
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetMail(RequestHandlerData requestHandler, GetMailReq request, RequestProceedResultDelegate<GetMailResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetMailResp()
            {
                Mail = Database.GetMail(request.MailId, request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetMailNotification(RequestHandlerData requestHandler, GetMailNotificationReq request, RequestProceedResultDelegate<GetMailNotificationResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetMailNotificationResp()
            {
                NotificationCount = Database.GetMailNotification(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetIdByCharacterName(RequestHandlerData requestHandler, GetIdByCharacterNameReq request, RequestProceedResultDelegate<GetIdByCharacterNameResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetIdByCharacterNameResp()
            {
                Id = Database.GetIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserIdByCharacterName(RequestHandlerData requestHandler, GetUserIdByCharacterNameReq request, RequestProceedResultDelegate<GetUserIdByCharacterNameResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetUserIdByCharacterNameResp()
            {
                UserId = Database.GetUserIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserUnbanTime(RequestHandlerData requestHandler, GetUserUnbanTimeReq request, RequestProceedResultDelegate<GetUserUnbanTimeResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            long unbanTime = Database.GetUserUnbanTime(request.UserId);
            result.Invoke(AckResponseCode.Success, new GetUserUnbanTimeResp()
            {
                UnbanTime = unbanTime,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetUserUnbanTimeByCharacterName(RequestHandlerData requestHandler, SetUserUnbanTimeByCharacterNameReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.SetUserUnbanTimeByCharacterName(request.CharacterName, request.UnbanTime);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetCharacterUnmuteTimeByName(RequestHandlerData requestHandler, SetCharacterUnmuteTimeByNameReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.SetCharacterUnmuteTimeByName(request.CharacterName, request.UnmuteTime);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetSummonBuffs(RequestHandlerData requestHandler, GetSummonBuffsReq request, RequestProceedResultDelegate<GetSummonBuffsResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetSummonBuffsResp()
            {
                SummonBuffs = Database.GetSummonBuffs(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetSummonBuffs(RequestHandlerData requestHandler, SetSummonBuffsReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.SetSummonBuffs(request.CharacterId, request.SummonBuffs);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindEmail(RequestHandlerData requestHandler, FindEmailReq request, RequestProceedResultDelegate<FindEmailResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new FindEmailResp()
            {
                FoundAmount = FindEmail(request.Email),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ValidateEmailVerification(RequestHandlerData requestHandler, ValidateEmailVerificationReq request, RequestProceedResultDelegate<ValidateEmailVerificationResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new ValidateEmailVerificationResp()
            {
                IsPass = Database.ValidateEmailVerification(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetFriendRequestNotification(RequestHandlerData requestHandler, GetFriendRequestNotificationReq request, RequestProceedResultDelegate<GetFriendRequestNotificationResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            result.Invoke(AckResponseCode.Success, new GetFriendRequestNotificationResp()
            {
                NotificationCount = Database.GetFriendRequestNotification(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateUserCount(RequestHandlerData requestHandler, UpdateUserCountReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            Database.UpdateUserCount(request.UserCount);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

#if UNITY_EDITOR || UNITY_SERVER
        protected bool ValidateAccessToken(string userId, string accessToken)
        {
            if (!disableCacheReading && cachedUserAccessToken.ContainsKey(userId))
            {
                // Already cached access token, so validate access token from cache
                return accessToken.Equals(cachedUserAccessToken[userId]);
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                if (Database.ValidateAccessToken(userId, accessToken))
                {
                    // Pass, store access token to the dictionary
                    cachedUserAccessToken[userId] = accessToken;
                    return true;
                }
            }
            return false;
        }

        protected long FindUsername(string username)
        {
            long foundAmount;
            if (!disableCacheReading && cachedUsernames.Contains(username))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindUsername(username);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    cachedUsernames.Add(username);
            }
            return foundAmount;
        }

        protected long FindCharacterName(string characterName)
        {
            long foundAmount;
            if (!disableCacheReading && cachedCharacterNames.Contains(characterName))
            {
                // Already cached character name, so validate character name from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindCharacterName(characterName);
                // Cache character name, it will be used to validate later
                if (foundAmount > 0)
                    cachedCharacterNames.Add(characterName);
            }
            return foundAmount;
        }

        protected long FindGuildName(string guildName)
        {
            long foundAmount;
            if (!disableCacheReading && cachedGuildNames.Contains(guildName))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindGuildName(guildName);
                // Cache guild name, it will be used to validate later
                if (foundAmount > 0)
                    cachedGuildNames.Add(guildName);
            }
            return foundAmount;
        }

        protected long FindEmail(string email)
        {
            long foundAmount;
            if (!disableCacheReading && cachedEmails.Contains(email))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindEmail(email);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    cachedEmails.Add(email);
            }
            return foundAmount;
        }

        protected List<BuildingSaveData> ReadBuildings(string mapName)
        {
            List<BuildingSaveData> buildings = new List<BuildingSaveData>();
            if (!disableCacheReading && cachedBuilding.ContainsKey(mapName))
            {
                // Get buildings from cache
                buildings.AddRange(cachedBuilding[mapName].Values);
            }
            else
            {
                // Read buildings from database
                buildings.AddRange(Database.ReadBuildings(mapName));
                // Store buildings to cache
                if (cachedBuilding.TryAdd(mapName, new ConcurrentDictionary<string, BuildingSaveData>()))
                {
                    foreach (BuildingSaveData building in buildings)
                    {
                        cachedBuilding[mapName].TryAdd(building.Id, building);
                    }
                }
            }
            return buildings;
        }

        protected int ReadGold(string userId)
        {
            int gold;
            if (disableCacheReading || !cachedUserGold.TryGetValue(userId, out gold))
            {
                // Doesn't cached yet, so get data from database and cache it
                gold = Database.GetGold(userId);
                cachedUserGold[userId] = gold;
            }
            return gold;
        }

        protected int ReadCash(string userId)
        {
            int cash;
            if (disableCacheReading || !cachedUserCash.TryGetValue(userId, out cash))
            {
                // Doesn't cached yet, so get data from database and cache it
                cash = Database.GetCash(userId);
                cachedUserCash[userId] = cash;
            }
            return cash;
        }

        protected PlayerCharacterData ReadCharacter(string id)
        {
            PlayerCharacterData character;
            if (disableCacheReading || !cachedUserCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = Database.ReadCharacter(id);
                // Cache the data, it will be used later
                if (character != null)
                {
                    cachedUserCharacter[id] = character;
                    cachedCharacterNames.Add(character.CharacterName);
                }
            }
            return character;
        }

        protected PlayerCharacterData ReadCharacterWithUserIdValidation(string id, string userId)
        {
            PlayerCharacterData character;
            if (disableCacheReading || !cachedUserCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = Database.ReadCharacter(id);
                // Cache the data, it will be used later
                if (character != null)
                {
                    cachedUserCharacter[id] = character;
                    cachedCharacterNames.Add(character.CharacterName);
                }
            }
            if (character != null && character.UserId != userId)
                character = null;
            return character;
        }

        protected SocialCharacterData ReadSocialCharacter(string id)
        {
            SocialCharacterData character;
            if (disableCacheReading || !cachedSocialCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = SocialCharacterData.Create(Database.ReadCharacter(id, false, false, false, false, false, false, false, false, false, false));
                // Cache the data
                cachedSocialCharacter[id] = character;
            }
            return character;
        }

        protected PartyData ReadParty(int id)
        {
            PartyData party;
            if (disableCacheReading || !cachedParty.TryGetValue(id, out party))
            {
                // Doesn't cached yet, so get data from database
                party = Database.ReadParty(id);
                // Cache the data
                if (party != null)
                {
                    cachedParty[id] = party;
                    CacheSocialCharacters(party.GetMembers());
                }
            }
            return party;
        }

        protected GuildData ReadGuild(int id)
        {
            GuildData guild;
            if (disableCacheReading || !cachedGuild.TryGetValue(id, out guild))
            {
                // Doesn't cached yet, so get data from database
                guild = Database.ReadGuild(id, GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles);
                // Cache the data
                if (guild != null)
                {
                    cachedGuild[id] = guild;
                    cachedGuildNames.Add(guild.guildName);
                    CacheSocialCharacters(guild.GetMembers());
                }
            }
            return guild;
        }

        protected List<CharacterItem> ReadStorageItems(StorageId storageId)
        {
            List<CharacterItem> storageItems;
            if (disableCacheReading || !cachedStorageItems.TryGetValue(storageId, out storageItems))
            {
                // Doesn't cached yet, so get data from database
                storageItems = Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
                // Cache the data, it will be used later
                if (storageItems != null)
                    cachedStorageItems[storageId] = storageItems;
            }
            return storageItems;
        }

        protected void CacheSocialCharacters(IEnumerable<SocialCharacterData> socialCharacters)
        {
            foreach (SocialCharacterData socialCharacter in socialCharacters)
            {
                cachedSocialCharacter[socialCharacter.id] = socialCharacter;
            }
        }
#endif
    }
}