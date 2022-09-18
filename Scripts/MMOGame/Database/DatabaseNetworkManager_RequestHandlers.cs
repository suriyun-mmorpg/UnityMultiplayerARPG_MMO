using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        protected async UniTaskVoid ValidateUserLogin(RequestHandlerData requestHandler, ValidateUserLoginReq request, RequestProceedResultDelegate<ValidateUserLoginResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            bool isPass;
            if (cachedUserAccessToken.ContainsKey(request.UserId))
            {
                // Already cached access token, so validate access token from cache
                isPass = request.AccessToken.Equals(cachedUserAccessToken[request.UserId]);
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                isPass = Database.ValidateAccessToken(request.UserId, request.AccessToken);
                // Pass, store access token to the dictionary
                if (isPass)
                    cachedUserAccessToken[request.UserId] = request.AccessToken;
            }
            result.Invoke(AckResponseCode.Success, new ValidateAccessTokenResp()
            {
                IsPass = isPass
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserLevel(RequestHandlerData requestHandler, GetUserLevelReq request, RequestProceedResultDelegate<GetUserLevelResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetUserLevelResp()
            {
                UserLevel = Database.GetUserLevel(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetGold(RequestHandlerData requestHandler, GetGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GoldResp()
            {
                Gold = ReadGold(request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ChangeGold(RequestHandlerData requestHandler, ChangeGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new CashResp()
            {
                Cash = ReadCash(request.UserId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ChangeCash(RequestHandlerData requestHandler, ChangeCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            long foundAmount;
            if (cachedUsernames.Contains(request.Username))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindUsername(request.Username);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    cachedUsernames.Add(request.Username);
            }
            result.Invoke(AckResponseCode.Success, new FindUsernameResp()
            {
                FoundAmount = foundAmount
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateCharacter(RequestHandlerData requestHandler, CreateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = ReadCharacter(request.CharacterId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadCharacters(RequestHandlerData requestHandler, ReadCharactersReq request, RequestProceedResultDelegate<CharactersResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            long foundAmount;
            if (cachedCharacterNames.Contains(request.CharacterName))
            {
                // Already cached character name, so validate character name from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindCharacterName(request.CharacterName);
                // Cache character name, it will be used to validate later
                if (foundAmount > 0)
                    cachedCharacterNames.Add(request.CharacterName);
            }
            result.Invoke(AckResponseCode.Success, new FindCharacterNameResp()
            {
                FoundAmount = foundAmount
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindCharacters(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = Database.FindCharacters(request.FinderId, request.CharacterName, request.Skip, request.Limit)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateFriend(RequestHandlerData requestHandler, CreateFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            Database.CreateFriend(request.Character1Id, request.Character2Id, request.State);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DeleteFriend(RequestHandlerData requestHandler, DeleteFriendReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            Database.DeleteFriend(request.Character1Id, request.Character2Id);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadFriends(RequestHandlerData requestHandler, ReadFriendsReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = Database.ReadFriends(request.CharacterId, request.ReadById2, request.State, request.Skip, request.Limit),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateBuilding(RequestHandlerData requestHandler, CreateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            List<BuildingSaveData> buildings = new List<BuildingSaveData>();
            if (cachedBuilding.ContainsKey(request.MapName))
            {
                // Get buildings from cache
                buildings.AddRange(cachedBuilding[request.MapName].Values);
            }
            else if (cachedBuilding.TryAdd(request.MapName, new ConcurrentDictionary<string, BuildingSaveData>()))
            {
                // Store buildings to cache
                buildings.AddRange(Database.ReadBuildings(request.MapName));
                foreach (BuildingSaveData building in buildings)
                {
                    cachedBuilding[request.MapName].TryAdd(building.Id, building);
                }
            }
            result.Invoke(AckResponseCode.Success, new BuildingsResp()
            {
                List = buildings
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateParty(RequestHandlerData requestHandler, CreatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            Database.DeleteParty(request.PartyId);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateCharacterParty(RequestHandlerData requestHandler, UpdateCharacterPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = ReadParty(request.PartyId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid CreateGuild(RequestHandlerData requestHandler, CreateGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            long foundAmount;
            if (cachedGuildNames.Contains(request.GuildName))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindGuildName(request.GuildName);
                // Cache guild name, it will be used to validate later
                if (foundAmount > 0)
                    cachedGuildNames.Add(request.GuildName);
            }
            result.Invoke(AckResponseCode.Success, new FindGuildNameResp()
            {
                FoundAmount = foundAmount
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ReadGuild(RequestHandlerData requestHandler, ReadGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = ReadGuild(request.GuildId)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid IncreaseGuildExp(RequestHandlerData requestHandler, IncreaseGuildExpReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItems;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItems))
            {
                // Doesn't cached yet, so get data from database
                storageItems = Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
                // Cache data, it will be used to validate later
                if (storageItems != null)
                    cachedStorageItems[storageId] = storageItems;
            }
            result.Invoke(AckResponseCode.Success, new ReadStorageItemsResp()
            {
                StorageCharacterItems = storageItems
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid MoveItemToStorage(RequestHandlerData requestHandler, MoveItemToStorageReq request, RequestProceedResultDelegate<MoveItemToStorageResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND
                });
                return;
            }
            if (request.InventoryItemIndex < 0 ||
                request.InventoryItemIndex >= request.Inventory.Count)
            {
                // Invalid inventory index
                result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX
                });
                return;
            }
            character.NonEquipItems = request.Inventory;
            await UniTask.SwitchToMainThread();
            bool isLimitWeight = request.WeightLimit > 0;
            bool isLimitSlot = request.SlotLimit > 0;
            short weightLimit = request.WeightLimit;
            short slotLimit = request.SlotLimit;
            // Prepare character and item data
            CharacterItem movingItem = character.NonEquipItems[request.InventoryItemIndex].Clone(true);
            movingItem.id = GenericUtils.GetUniqueId();
            movingItem.amount = request.InventoryItemAmount;
            if (request.StorageItemIndex < 0 ||
                request.StorageItemIndex >= storageItemList.Count ||
                storageItemList[request.StorageItemIndex].dataId == movingItem.dataId)
            {
                // Add to storage or merge
                bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                    movingItem.dataId, movingItem.amount, isLimitWeight, weightLimit,
                    storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
                if (isOverwhelming || !storageItemList.IncreaseItems(movingItem))
                {
                    // Storage will overwhelming
                    result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
                    {
                        Error = UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING
                    });
                    return;
                }
                // Remove from inventory
                character.DecreaseItemsByIndex(request.InventoryItemIndex, request.InventoryItemAmount);
                character.FillEmptySlots();
            }
            else
            {
                CharacterItem storageItem = storageItemList[request.StorageItemIndex];
                CharacterItem nonEquipItem = character.NonEquipItems[request.InventoryItemIndex];
                if (storageItem.IsEmptySlot())
                {
                    // Add to storage or merge
                    bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                        movingItem.dataId, movingItem.amount, isLimitWeight, weightLimit,
                        storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
                    if (isOverwhelming)
                    {
                        // Storage will overwhelming
                        result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
                        {
                            Error = UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING
                        });
                        return;
                    }
                    // Increase to storage
                    movingItem.id = GenericUtils.GetUniqueId();
                    storageItemList[request.StorageItemIndex] = movingItem;
                    // Remove from inventory
                    character.DecreaseItemsByIndex(request.InventoryItemIndex, request.InventoryItemAmount);
                    character.FillEmptySlots();
                }
                else
                {
                    // Swapping
                    storageItem.id = GenericUtils.GetUniqueId();
                    nonEquipItem.id = GenericUtils.GetUniqueId();
                    storageItemList[request.StorageItemIndex] = nonEquipItem;
                    character.NonEquipItems[request.InventoryItemIndex] = storageItem;
                }
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
            {
                Error = UITextKeys.NONE,
                InventoryItemItems = new List<CharacterItem>(character.NonEquipItems),
                StorageCharacterItems = storageItemList,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid MoveItemFromStorage(RequestHandlerData requestHandler, MoveItemFromStorageReq request, RequestProceedResultDelegate<MoveItemFromStorageResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND
                });
                return;
            }
            if (request.StorageItemIndex < 0 ||
                request.StorageItemIndex >= storageItemList.Count)
            {
                // Invalid storage index
                result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX
                });
                return;
            }
            character.NonEquipItems = request.Inventory;
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = request.SlotLimit;
            // Prepare item data
            CharacterItem movingItem = storageItemList[request.StorageItemIndex].Clone(true);
            movingItem.amount = request.StorageItemAmount;
            if (request.InventoryItemIndex < 0 ||
                request.InventoryItemIndex >= character.NonEquipItems.Count ||
                character.NonEquipItems[request.InventoryItemIndex].dataId == movingItem.dataId)
            {
                // Add to inventory or merge
                bool isOverwhelming = character.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                if (isOverwhelming || !character.IncreaseItems(movingItem))
                {
                    // inventory will overwhelming
                    result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
                    {
                        Error = UITextKeys.UI_ERROR_WILL_OVERWHELMING
                    });
                    return;
                }
                // Remove from storage
                storageItemList.DecreaseItemsByIndex(request.StorageItemIndex, request.StorageItemAmount, isLimitSlot);
                storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            }
            else
            {
                CharacterItem storageItem = storageItemList[request.StorageItemIndex];
                CharacterItem nonEquipItem = character.NonEquipItems[request.InventoryItemIndex];
                if (nonEquipItem.IsEmptySlot())
                {
                    // Add to inventory or merge
                    bool isOverwhelming = character.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                    if (isOverwhelming)
                    {
                        // inventory will overwhelming
                        result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
                        {
                            Error = UITextKeys.UI_ERROR_WILL_OVERWHELMING
                        });
                        return;
                    }
                    // Increase to inventory
                    movingItem.id = GenericUtils.GetUniqueId();
                    character.NonEquipItems[request.InventoryItemIndex] = movingItem;
                    // Remove from storage
                    storageItemList.DecreaseItemsByIndex(request.StorageItemIndex, request.StorageItemAmount, isLimitSlot);
                    storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
                }
                else
                {
                    // Swapping
                    storageItem.id = GenericUtils.GetUniqueId();
                    nonEquipItem.id = GenericUtils.GetUniqueId();
                    storageItemList[request.StorageItemIndex] = nonEquipItem;
                    character.NonEquipItems[request.InventoryItemIndex] = storageItem;
                }
            }
            character.FillEmptySlots();
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
            {
                Error = UITextKeys.NONE,
                InventoryItemItems = new List<CharacterItem>(character.NonEquipItems),
                StorageCharacterItems = storageItemList,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SwapOrMergeStorageItem(RequestHandlerData requestHandler, SwapOrMergeStorageItemReq request, RequestProceedResultDelegate<SwapOrMergeStorageItemResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Success, new SwapOrMergeStorageItemResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = request.SlotLimit;
            // Prepare item data
            CharacterItem fromItem = storageItemList[request.FromIndex];
            CharacterItem toItem = storageItemList[request.ToIndex];
            fromItem.id = GenericUtils.GetUniqueId();
            toItem.id = GenericUtils.GetUniqueId();
            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    storageItemList[request.FromIndex] = CharacterItem.Empty;
                    storageItemList[request.ToIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    storageItemList[request.FromIndex] = fromItem;
                    storageItemList[request.ToIndex] = toItem;
                }
            }
            else
            {
                // Swap
                storageItemList[request.FromIndex] = toItem;
                storageItemList[request.ToIndex] = fromItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new SwapOrMergeStorageItemResp()
            {
                Error = UITextKeys.NONE,
                StorageCharacterItems = storageItemList,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid IncreaseStorageItems(RequestHandlerData requestHandler, IncreaseStorageItemsReq request, RequestProceedResultDelegate<IncreaseStorageItemsResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Success, new IncreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitWeight = request.WeightLimit > 0;
            bool isLimitSlot = request.SlotLimit > 0;
            short weightLimit = request.WeightLimit;
            short slotLimit = request.SlotLimit;
            CharacterItem addingItem = request.Item;
            // Increase item to storage
            bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                addingItem.dataId, addingItem.amount, isLimitWeight, weightLimit,
                storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
            if (isOverwhelming || !storageItemList.IncreaseItems(addingItem))
            {
                // Storage will overwhelming
                result.Invoke(AckResponseCode.Success, new IncreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING
                });
                return;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new IncreaseStorageItemsResp()
            {
                Error = UITextKeys.NONE,
                StorageCharacterItems = storageItemList,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid DecreaseStorageItems(RequestHandlerData requestHandler, DecreaseStorageItemsReq request, RequestProceedResultDelegate<DecreaseStorageItemsResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Success, new DecreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND,
                });
                return;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = request.SlotLimit;
            // Decrease item from storage
            Dictionary<int, short> decreasedItems;
            if (!storageItemList.DecreaseItems(request.DataId, request.Amount, isLimitSlot, out decreasedItems))
            {
                result.Invoke(AckResponseCode.Success, new DecreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS,
                });
                return;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            List<ItemIndexAmountMap> decreasedItemList = new List<ItemIndexAmountMap>();
            foreach (int itemIndex in decreasedItems.Keys)
            {
                decreasedItemList.Add(new ItemIndexAmountMap()
                {
                    Index = itemIndex,
                    Amount = decreasedItems[itemIndex]
                });
            }
            result.Invoke(AckResponseCode.Success, new DecreaseStorageItemsResp()
            {
                Error = UITextKeys.NONE,
                StorageCharacterItems = storageItemList,
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid MailList(RequestHandlerData requestHandler, MailListReq request, RequestProceedResultDelegate<MailListResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new MailListResp()
            {
                List = Database.MailList(request.UserId, request.OnlyNewMails)
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateReadMailState(RequestHandlerData requestHandler, UpdateReadMailStateReq request, RequestProceedResultDelegate<UpdateReadMailStateResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            long updated = Database.UpdateReadMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Success, new UpdateReadMailStateResp()
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
#if UNITY_SERVER || !MMO_BUILD
            long updated = Database.UpdateClaimMailItemsState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Success, new UpdateClaimMailItemsStateResp()
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
#if UNITY_SERVER || !MMO_BUILD
            long updated = Database.UpdateDeleteMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Success, new UpdateDeleteMailStateResp()
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
#if UNITY_SERVER || !MMO_BUILD
            Mail mail = request.Mail;
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                result.Invoke(AckResponseCode.Success, new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER
                });
                return;
            }
            long created = Database.CreateMail(mail);
            if (created <= 0)
            {
                result.Invoke(AckResponseCode.Success, new SendMailResp()
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
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetMailResp()
            {
                Mail = Database.GetMail(request.MailId, request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetMailNotification(RequestHandlerData requestHandler, GetMailNotificationReq request, RequestProceedResultDelegate<GetMailNotificationResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetMailNotificationResp()
            {
                NotificationCount = Database.GetMailNotification(request.UserId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetIdByCharacterName(RequestHandlerData requestHandler, GetIdByCharacterNameReq request, RequestProceedResultDelegate<GetIdByCharacterNameResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetIdByCharacterNameResp()
            {
                Id = Database.GetIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserIdByCharacterName(RequestHandlerData requestHandler, GetUserIdByCharacterNameReq request, RequestProceedResultDelegate<GetUserIdByCharacterNameResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetUserIdByCharacterNameResp()
            {
                UserId = Database.GetUserIdByCharacterName(request.CharacterName),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetUserUnbanTime(RequestHandlerData requestHandler, GetUserUnbanTimeReq request, RequestProceedResultDelegate<GetUserUnbanTimeResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
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
#if UNITY_SERVER || !MMO_BUILD
            Database.SetUserUnbanTimeByCharacterName(request.CharacterName, request.UnbanTime);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetCharacterUnmuteTimeByName(RequestHandlerData requestHandler, SetCharacterUnmuteTimeByNameReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            Database.SetCharacterUnmuteTimeByName(request.CharacterName, request.UnmuteTime);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetSummonBuffs(RequestHandlerData requestHandler, GetSummonBuffsReq request, RequestProceedResultDelegate<GetSummonBuffsResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetSummonBuffsResp()
            {
                SummonBuffs = Database.GetSummonBuffs(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid SetSummonBuffs(RequestHandlerData requestHandler, SetSummonBuffsReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            Database.SetSummonBuffs(request.CharacterId, request.SummonBuffs);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid FindEmail(RequestHandlerData requestHandler, FindEmailReq request, RequestProceedResultDelegate<FindEmailResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            long foundAmount;
            if (cachedEmails.Contains(request.Email))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = Database.FindEmail(request.Email);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    cachedEmails.Add(request.Email);
            }
            result.Invoke(AckResponseCode.Success, new FindEmailResp()
            {
                FoundAmount = foundAmount
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid ValidateEmailVerification(RequestHandlerData requestHandler, ValidateEmailVerificationReq request, RequestProceedResultDelegate<ValidateEmailVerificationResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            bool isPass = Database.ValidateEmailVerification(request.UserId);
            result.Invoke(AckResponseCode.Success, new ValidateEmailVerificationResp()
            {
                IsPass = isPass
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid GetFriendRequestNotification(RequestHandlerData requestHandler, GetFriendRequestNotificationReq request, RequestProceedResultDelegate<GetFriendRequestNotificationResp> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new GetFriendRequestNotificationResp()
            {
                NotificationCount = Database.GetFriendRequestNotification(request.CharacterId),
            });
            await UniTask.Yield();
#endif
        }

        protected async UniTaskVoid UpdateUserCount(RequestHandlerData requestHandler, UpdateUserCountReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_SERVER || !MMO_BUILD
            Database.UpdateUserCount(request.UserCount);
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
            await UniTask.Yield();
#endif
        }

#if UNITY_SERVER || !MMO_BUILD
        protected int ReadGold(string userId)
        {
            int gold;
            if (!cachedUserGold.TryGetValue(userId, out gold))
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
            if (!cachedUserCash.TryGetValue(userId, out cash))
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
            if (!cachedUserCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = Database.ReadCharacter(id);
                // Cache character, it will be used to validate later
                if (character != null)
                {
                    cachedUserCharacter[id] = character;
                    cachedCharacterNames.Add(character.CharacterName);
                }
            }
            return character;
        }

        protected SocialCharacterData ReadSocialCharacter(string id)
        {
            SocialCharacterData character;
            if (!cachedSocialCharacter.TryGetValue(id, out character))
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
            if (!cachedParty.TryGetValue(id, out party))
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
            if (!cachedGuild.TryGetValue(id, out guild))
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

        public void CacheSocialCharacters(IEnumerable<SocialCharacterData> socialCharacters)
        {
            foreach (SocialCharacterData socialCharacter in socialCharacters)
            {
                cachedSocialCharacter[socialCharacter.id] = socialCharacter;
            }
        }
#endif
    }
}