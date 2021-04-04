using System.Linq;
using UnityEngine;
using LiteNetLibManager;
using ConcurrentCollections;
using System.Collections.Concurrent;
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(-898)]
    public partial class DatabaseNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        [SerializeField]
        private BaseDatabase database;
        [SerializeField]
        private BaseDatabase[] databaseOptions;

#if UNITY_STANDALONE && !CLIENT_BUILD
        public BaseDatabase Database { get { return database == null ? databaseOptions.FirstOrDefault() : database; } }

        public void SetDatabaseByOptionIndex(int index)
        {
            if (databaseOptions != null &&
                databaseOptions.Length > 0 &&
                index >= 0 &&
                index < databaseOptions.Length)
                database = databaseOptions[index];
        }
#endif

        // TODO: I'm going to make in-memory database without Redis for now
        // In the future it may implements Redis
        // It's going to get some data from all tables but not every records
        // Just some records that players were requested
        private ConcurrentHashSet<string> updatingCharacterIds = new ConcurrentHashSet<string>();
        private ConcurrentHashSet<string> cachedUsernames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentHashSet<string> cachedCharacterNames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentHashSet<string> cachedGuildNames = new ConcurrentHashSet<string>(StringComparer.OrdinalIgnoreCase);
        private ConcurrentDictionary<string, string> cachedUserAccessToken = new ConcurrentDictionary<string, string>();
        private ConcurrentDictionary<string, int> cachedUserGold = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, int> cachedUserCash = new ConcurrentDictionary<string, int>();
        private ConcurrentDictionary<string, PlayerCharacterData> cachedUserCharacter = new ConcurrentDictionary<string, PlayerCharacterData>();
        private ConcurrentDictionary<string, SocialCharacterData> cachedSocialCharacter = new ConcurrentDictionary<string, SocialCharacterData>();
        private ConcurrentDictionary<string, ConcurrentDictionary<string, BuildingSaveData>> cachedBuilding = new ConcurrentDictionary<string, ConcurrentDictionary<string, BuildingSaveData>>();
        private ConcurrentDictionary<string, List<SocialCharacterData>> cachedFriend = new ConcurrentDictionary<string, List<SocialCharacterData>>();
        private ConcurrentDictionary<int, PartyData> cachedParty = new ConcurrentDictionary<int, PartyData>();
        private ConcurrentDictionary<int, GuildData> cachedGuild = new ConcurrentDictionary<int, GuildData>();
        private ConcurrentDictionary<StorageId, List<CharacterItem>> cachedStorageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();

        public async UniTaskVoid ValidateUserLogin(RequestHandlerData requestHandler, ValidateUserLoginReq request, RequestProceedResultDelegate<ValidateUserLoginResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            string userId = await Database.ValidateUserLogin(request.Username, request.Password);
            if (string.IsNullOrEmpty(userId))
            {
                result.Invoke(AckResponseCode.Error, new ValidateUserLoginResp());
                return;
            }
            result.Invoke(AckResponseCode.Success, new ValidateUserLoginResp()
            {
                UserId = userId,
            });
#endif
        }

        public async UniTaskVoid ValidateAccessToken(RequestHandlerData requestHandler, ValidateAccessTokenReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            bool isPass;
            if (cachedUserAccessToken.ContainsKey(request.UserId))
            {
                // Already cached access token, so validate access token from cache
                isPass = request.AccessToken.Equals(cachedUserAccessToken[request.UserId]);
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                isPass = await Database.ValidateAccessToken(request.UserId, request.AccessToken);
                // Pass, store access token to the dictionary
                if (isPass)
                    cachedUserAccessToken[request.UserId] = request.AccessToken;
            }
            if (!isPass)
            {
                result.Invoke(AckResponseCode.Error, new EmptyMessage());
                return;
            }
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid GetUserLevel(RequestHandlerData requestHandler, GetUserLevelReq request, RequestProceedResultDelegate<GetUserLevelResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GetUserLevelResp()
            {
                UserLevel = await Database.GetUserLevel(request.UserId),
            });
#endif
        }

        public async UniTaskVoid GetGold(RequestHandlerData requestHandler, GetGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GoldResp()
            {
                Gold = await ReadGold(request.UserId)
            });
#endif
        }

        public async UniTaskVoid ChangeGold(RequestHandlerData requestHandler, ChangeGoldReq request, RequestProceedResultDelegate<GoldResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int gold = await ReadGold(request.UserId);
            gold += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserGold[request.UserId] = gold;
            // Update data to database
            await Database.UpdateGold(request.UserId, gold);
            result.Invoke(AckResponseCode.Success, new GoldResp()
            {
                Gold = gold
            });
#endif
        }

        public async UniTaskVoid GetCash(RequestHandlerData requestHandler, GetCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new CashResp()
            {
                Cash = await ReadCash(request.UserId)
            });
#endif
        }

        public async UniTaskVoid ChangeCash(RequestHandlerData requestHandler, ChangeCashReq request, RequestProceedResultDelegate<CashResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            int cash = await ReadCash(request.UserId);
            cash += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserCash[request.UserId] = cash;
            // Update data to database
            await Database.UpdateCash(request.UserId, cash);
            result.Invoke(AckResponseCode.Success, new CashResp()
            {
                Cash = cash
            });
#endif
        }

        public async UniTaskVoid UpdateAccessToken(RequestHandlerData requestHandler, UpdateAccessTokenReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Store access token to the dictionary, it will be used to validate later
            cachedUserAccessToken[request.UserId] = request.AccessToken;
            // Update data to database
            await Database.UpdateAccessToken(request.UserId, request.AccessToken);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid CreateUserLogin(RequestHandlerData requestHandler, CreateUserLoginReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Cache username, it will be used to validate later
            cachedUsernames.Add(request.Username);
            // Insert new user login to database
            await Database.CreateUserLogin(request.Username, request.Password);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid FindUsername(RequestHandlerData requestHandler, FindUsernameReq request, RequestProceedResultDelegate<FindUsernameResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long foundAmount;
            if (cachedUsernames.Contains(request.Username))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = await Database.FindUsername(request.Username);
                // Cache username, it will be used to validate later
                if (foundAmount > 0)
                    cachedUsernames.Add(request.Username);
            }
            result.Invoke(AckResponseCode.Success, new FindUsernameResp()
            {
                FoundAmount = foundAmount
            });
#endif
        }

        public async UniTaskVoid CreateCharacter(RequestHandlerData requestHandler, CreateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PlayerCharacterData character = request.CharacterData;
            // Store character to the dictionary, it will be used later
            cachedUserCharacter[character.Id] = character;
            cachedCharacterNames.Add(character.CharacterName);
            // Insert new character to database
            await Database.CreateCharacter(request.UserId, character);
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = character
            });
#endif
        }

        public async UniTaskVoid ReadCharacter(RequestHandlerData requestHandler, ReadCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = await ReadCharacter(request.CharacterId)
            });
#endif
        }

        public async UniTaskVoid ReadCharacters(RequestHandlerData requestHandler, ReadCharactersReq request, RequestProceedResultDelegate<CharactersResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new CharactersResp()
            {
                List = await Database.ReadCharacters(request.UserId)
            });
#endif
        }

        public async UniTaskVoid UpdateCharacter(RequestHandlerData requestHandler, UpdateCharacterReq request, RequestProceedResultDelegate<CharacterResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PlayerCharacterData character = request.CharacterData;
            // Avoid duplicating updates
            if (!updatingCharacterIds.Contains(character.Id))
            {
                updatingCharacterIds.Add(character.Id);
                // Cache the data, it will be used later
                cachedUserCharacter[character.Id] = character;
                // Update data to database
                // TODO: May update later to reduce amount of processes
                await Database.UpdateCharacter(character);
                updatingCharacterIds.TryRemove(character.Id);
            }
            result.Invoke(AckResponseCode.Success, new CharacterResp()
            {
                CharacterData = character
            });
#endif
        }

        public async UniTaskVoid DeleteCharacter(RequestHandlerData requestHandler, DeleteCharacterReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Remove data from cache
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
            {
                string characterName = cachedUserCharacter[request.CharacterId].CharacterName;
                cachedCharacterNames.TryRemove(characterName);
                cachedUserCharacter.TryRemove(request.CharacterId, out _);
            }
            // Delete data from database
            await Database.DeleteCharacter(request.UserId, request.CharacterId);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid FindCharacterName(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<FindCharacterNameResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long foundAmount;
            if (cachedCharacterNames.Contains(request.CharacterName))
            {
                // Already cached character name, so validate character name from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = await Database.FindCharacterName(request.CharacterName);
                // Cache character name, it will be used to validate later
                if (foundAmount > 0)
                    cachedCharacterNames.Add(request.CharacterName);
            }
            result.Invoke(AckResponseCode.Success, new FindCharacterNameResp()
            {
                FoundAmount = foundAmount
            });
#endif
        }

        public async UniTaskVoid FindCharacters(RequestHandlerData requestHandler, FindCharacterNameReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = await Database.FindCharacters(request.CharacterName)
            });
#endif
        }

        public async UniTaskVoid CreateFriend(RequestHandlerData requestHandler, CreateFriendReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            List<SocialCharacterData> friends = await ReadFriends(request.Character1Id);
            // Update to cache
            for (int i = 0; i < friends.Count; ++i)
            {
                if (friends[i].id.Equals(request.Character2Id))
                {
                    friends.RemoveAt(i);
                    break;
                }
            }
            SocialCharacterData character = await ReadSocialCharacter(request.Character2Id);
            friends.Add(character);
            cachedFriend[request.Character1Id] = friends;
            // Update to database
            await Database.CreateFriend(request.Character1Id, character.id);
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = friends
            });
#endif
        }

        public async UniTaskVoid DeleteFriend(RequestHandlerData requestHandler, DeleteFriendReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            List<SocialCharacterData> friends = await ReadFriends(request.Character1Id);
            // Update to cache
            for (int i = 0; i < friends.Count; ++i)
            {
                if (friends[i].id.Equals(request.Character2Id))
                {
                    friends.RemoveAt(i);
                    break;
                }
            }
            cachedFriend[request.Character1Id] = friends;
            // Update to database
            await Database.DeleteFriend(request.Character1Id, request.Character2Id);
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = friends
            });
#endif
        }

        public async UniTaskVoid ReadFriends(RequestHandlerData requestHandler, ReadFriendsReq request, RequestProceedResultDelegate<SocialCharactersResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new SocialCharactersResp()
            {
                List = await ReadFriends(request.CharacterId)
            });
#endif
        }

        public async UniTaskVoid CreateBuilding(RequestHandlerData requestHandler, CreateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
            await Database.CreateBuilding(request.MapName, building);
            result.Invoke(AckResponseCode.Success, new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
#endif
        }

        public async UniTaskVoid UpdateBuilding(RequestHandlerData requestHandler, UpdateBuildingReq request, RequestProceedResultDelegate<BuildingResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
            await Database.UpdateBuilding(request.MapName, building);
            result.Invoke(AckResponseCode.Success, new BuildingResp()
            {
                BuildingData = request.BuildingData
            });
#endif
        }

        public async UniTaskVoid DeleteBuilding(RequestHandlerData requestHandler, DeleteBuildingReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Remove from cache
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName].TryRemove(request.BuildingId, out _);
            // Remove from database
            await Database.DeleteBuilding(request.MapName, request.BuildingId);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid ReadBuildings(RequestHandlerData requestHandler, ReadBuildingsReq request, RequestProceedResultDelegate<BuildingsResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            List<BuildingSaveData> buildings = new List<BuildingSaveData>();
            if (cachedBuilding.ContainsKey(request.MapName))
            {
                // Get buildings from cache
                buildings.AddRange(cachedBuilding[request.MapName].Values);
            }
            else if (cachedBuilding.TryAdd(request.MapName, new ConcurrentDictionary<string, BuildingSaveData>()))
            {
                // Store buildings to cache
                buildings.AddRange(await Database.ReadBuildings(request.MapName));
                foreach (BuildingSaveData building in buildings)
                {
                    cachedBuilding[request.MapName].TryAdd(building.Id, building);
                }
            }
            result.Invoke(AckResponseCode.Success, new BuildingsResp()
            {
                List = buildings
            });
#endif
        }

        public async UniTaskVoid CreateParty(RequestHandlerData requestHandler, CreatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Insert to database
            int partyId = await Database.CreateParty(request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            // Cached the data
            PartyData party = new PartyData(partyId, request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            cachedParty[partyId] = party;
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        public async UniTaskVoid UpdateParty(RequestHandlerData requestHandler, UpdatePartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            party.Setting(request.ShareExp, request.ShareItem);
            cachedParty[request.PartyId] = party;
            // Update to database
            await Database.UpdateParty(request.PartyId, request.ShareExp, request.ShareItem);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        public async UniTaskVoid UpdatePartyLeader(RequestHandlerData requestHandler, UpdatePartyLeaderReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            party.SetLeader(request.LeaderCharacterId);
            cachedParty[request.PartyId] = party;
            // Update to database
            await Database.UpdatePartyLeader(request.PartyId, request.LeaderCharacterId);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        public async UniTaskVoid DeleteParty(RequestHandlerData requestHandler, DeletePartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            await Database.DeleteParty(request.PartyId);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid UpdateCharacterParty(RequestHandlerData requestHandler, UpdateCharacterPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            SocialCharacterData character = request.SocialCharacterData;
            party.AddMember(character);
            cachedParty[request.PartyId] = party;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].PartyId = request.PartyId;
            // Update to database
            await Database.UpdateCharacterParty(character.id, request.PartyId);
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = party
            });
#endif
        }

        public async UniTaskVoid ClearCharacterParty(RequestHandlerData requestHandler, ClearCharacterPartyReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PlayerCharacterData character = await ReadCharacter(request.CharacterId);
            PartyData party = await ReadParty(character.PartyId);
            // Update to cache
            party.RemoveMember(request.CharacterId);
            cachedParty[character.PartyId] = party;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
                cachedUserCharacter[request.CharacterId].PartyId = 0;
            // Update to database
            await Database.UpdateCharacterParty(request.CharacterId, 0);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid ReadParty(RequestHandlerData requestHandler, ReadPartyReq request, RequestProceedResultDelegate<PartyResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new PartyResp()
            {
                PartyData = await ReadParty(request.PartyId)
            });
#endif
        }

        public async UniTaskVoid CreateGuild(RequestHandlerData requestHandler, CreateGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Insert to database
            int guildId = await Database.CreateGuild(request.GuildName, request.LeaderCharacterId);
            // Cached the data
            GuildData guild = new GuildData(guildId, request.GuildName, request.LeaderCharacterId);
            cachedGuild[guildId] = guild;
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid UpdateGuildLeader(RequestHandlerData requestHandler, UpdateGuildLeaderReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetLeader(request.LeaderCharacterId);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildLeader(request.GuildId, request.LeaderCharacterId);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid UpdateGuildMessage(RequestHandlerData requestHandler, UpdateGuildMessageReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.guildMessage = request.GuildMessage;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildMessage(request.GuildId, request.GuildMessage);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid UpdateGuildRole(RequestHandlerData requestHandler, UpdateGuildRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetRole(request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, request.ShareExpPercentage);
            cachedGuild[request.GuildId] = guild;
            // Update to
            await Database.UpdateGuildRole(request.GuildId, request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, request.ShareExpPercentage);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid UpdateGuildMemberRole(RequestHandlerData requestHandler, UpdateGuildMemberRoleReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetMemberRole(request.MemberCharacterId, request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildMemberRole(request.MemberCharacterId, request.GuildRole);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid DeleteGuild(RequestHandlerData requestHandler, DeleteGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Remove data from cache
            if (cachedGuild.ContainsKey(request.GuildId))
            {
                string guildName = cachedGuild[request.GuildId].guildName;
                cachedGuildNames.TryRemove(guildName);
                cachedGuild.TryRemove(request.GuildId, out _);
            }
            // Remove data from database
            await Database.DeleteGuild(request.GuildId);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid UpdateCharacterGuild(RequestHandlerData requestHandler, UpdateCharacterGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            SocialCharacterData character = request.SocialCharacterData;
            guild.AddMember(character, request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].GuildId = request.GuildId;
            // Update to database
            await Database.UpdateCharacterGuild(character.id, request.GuildId, request.GuildRole);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = guild
            });
#endif
        }

        public async UniTaskVoid ClearCharacterGuild(RequestHandlerData requestHandler, ClearCharacterGuildReq request, RequestProceedResultDelegate<EmptyMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            PlayerCharacterData character = await ReadCharacter(request.CharacterId);
            GuildData guild = await ReadGuild(character.GuildId);
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
            await Database.UpdateCharacterGuild(request.CharacterId, 0, 0);
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
#endif
        }

        public async UniTaskVoid FindGuildName(RequestHandlerData requestHandler, FindGuildNameReq request, RequestProceedResultDelegate<FindGuildNameResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long foundAmount;
            if (cachedGuildNames.Contains(request.GuildName))
            {
                // Already cached username, so validate username from cache
                foundAmount = 1;
            }
            else
            {
                // Doesn't cached yet, so try validate from database
                foundAmount = await Database.FindGuildName(request.GuildName);
                // Cache guild name, it will be used to validate later
                if (foundAmount > 0)
                    cachedGuildNames.Add(request.GuildName);
            }
            result.Invoke(AckResponseCode.Success, new FindGuildNameResp()
            {
                FoundAmount = foundAmount
            });
#endif
        }

        public async UniTaskVoid ReadGuild(RequestHandlerData requestHandler, ReadGuildReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        public async UniTaskVoid IncreaseGuildExp(RequestHandlerData requestHandler, IncreaseGuildExpReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // TODO: May validate guild by character
            GuildData guild = await ReadGuild(request.GuildId);
            await UniTask.SwitchToMainThread();
            guild = GameInstance.Singleton.SocialSystemSetting.IncreaseGuildExp(guild, request.Exp);
            // Update to cache
            cachedGuild.TryAdd(guild.id, guild);
            // Update to database
            await Database.UpdateGuildLevel(request.GuildId, guild.level, guild.exp, guild.skillPoint);
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        public async UniTaskVoid AddGuildSkill(RequestHandlerData requestHandler, AddGuildSkillReq request, RequestProceedResultDelegate<GuildResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // TODO: May validate guild by character
            GuildData guild = await ReadGuild(request.GuildId);
            await UniTask.SwitchToMainThread();
            if (!guild.IsSkillReachedMaxLevel(request.SkillId) && guild.skillPoint > 0)
            {
                guild.AddSkillLevel(request.SkillId);
                // Update to cache
                cachedGuild[guild.id] = guild;
                // Update to database
                await Database.UpdateGuildSkillLevel(request.GuildId, request.SkillId, guild.GetSkillLevel(request.SkillId), guild.skillPoint);
            }
            result.Invoke(AckResponseCode.Success, new GuildResp()
            {
                GuildData = await ReadGuild(request.GuildId)
            });
#endif
        }

        public async UniTaskVoid GetGuildGold(RequestHandlerData requestHandler, GetGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            result.Invoke(AckResponseCode.Success, new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
#endif
        }

        public async UniTaskVoid ChangeGuildGold(RequestHandlerData requestHandler, ChangeGuildGoldReq request, RequestProceedResultDelegate<GuildGoldResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.gold += request.ChangeAmount;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildGold(request.GuildId, guild.gold);
            result.Invoke(AckResponseCode.Success, new GuildGoldResp()
            {
                GuildGold = guild.gold
            });
#endif
        }

        public async UniTaskVoid ReadStorageItems(RequestHandlerData requestHandler, ReadStorageItemsReq request, RequestProceedResultDelegate<ReadStorageItemsResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItems;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItems))
            {
                // Doesn't cached yet, so get data from database
                storageItems = await Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
                // Cache data, it will be used to validate later
                if (storageItems != null)
                    cachedStorageItems[storageId] = storageItems;
            }
            result.Invoke(AckResponseCode.Success, new ReadStorageItemsResp()
            {
                StorageCharacterItems = storageItems
            });
#endif
        }

        public async UniTaskVoid MoveItemToStorage(RequestHandlerData requestHandler, MoveItemToStorageReq request, RequestProceedResultDelegate<MoveItemToStorageResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Error, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                result.Invoke(AckResponseCode.Error, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND
                });
                return;
            }
            if (request.InventoryItemIndex < 0 ||
                request.InventoryItemIndex >= character.NonEquipItems.Count)
            {
                // Invalid inventory index
                result.Invoke(AckResponseCode.Error, new MoveItemToStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX
                });
                return;
            }
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
                    result.Invoke(AckResponseCode.Error, new MoveItemToStorageResp()
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
                // Swapping
                CharacterItem storageItem = storageItemList[request.StorageItemIndex];
                CharacterItem nonEquipItem = character.NonEquipItems[request.InventoryItemIndex];
                storageItem.id = GenericUtils.GetUniqueId();
                nonEquipItem.id = GenericUtils.GetUniqueId();
                storageItemList[request.StorageItemIndex] = nonEquipItem;
                character.NonEquipItems[request.InventoryItemIndex] = storageItem;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new MoveItemToStorageResp()
            {
                Error = UITextKeys.NONE,
                InventoryItemItems = new List<CharacterItem>(character.NonEquipItems),
                StorageCharacterItems = storageItemList,
            });
#endif
        }

        public async UniTaskVoid MoveItemFromStorage(RequestHandlerData requestHandler, MoveItemFromStorageReq request, RequestProceedResultDelegate<MoveItemFromStorageResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Error, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND
                });
                return;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                result.Invoke(AckResponseCode.Error, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_CHARACTER_NOT_FOUND
                });
                return;
            }
            if (request.StorageItemIndex < 0 ||
                request.StorageItemIndex >= storageItemList.Count)
            {
                // Invalid storage index
                result.Invoke(AckResponseCode.Error, new MoveItemFromStorageResp()
                {
                    Error = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX
                });
                return;
            }
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
                    result.Invoke(AckResponseCode.Error, new MoveItemFromStorageResp()
                    {
                        Error = UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING
                    });
                    return;
                }
                // Remove from storage
                storageItemList.DecreaseItemsByIndex(request.StorageItemIndex, request.StorageItemAmount, isLimitSlot);
                storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            }
            else
            {
                // Swapping
                CharacterItem storageItem = storageItemList[request.StorageItemIndex];
                CharacterItem nonEquipItem = character.NonEquipItems[request.InventoryItemIndex];
                storageItem.id = GenericUtils.GetUniqueId();
                nonEquipItem.id = GenericUtils.GetUniqueId();
                storageItemList[request.StorageItemIndex] = nonEquipItem;
                character.NonEquipItems[request.InventoryItemIndex] = storageItem;
            }
            character.FillEmptySlots();
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new MoveItemFromStorageResp()
            {
                Error = UITextKeys.NONE,
                InventoryItemItems = new List<CharacterItem>(character.NonEquipItems),
                StorageCharacterItems = storageItemList,
            });
#endif
        }

        public async UniTaskVoid SwapOrMergeStorageItem(RequestHandlerData requestHandler, SwapOrMergeStorageItemReq request, RequestProceedResultDelegate<SwapOrMergeStorageItemResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Error, new SwapOrMergeStorageItemResp()
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
            await Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new SwapOrMergeStorageItemResp()
            {
                Error = UITextKeys.NONE,
                StorageCharacterItems = storageItemList,
            });
#endif
        }

        public async UniTaskVoid IncreaseStorageItems(RequestHandlerData requestHandler, IncreaseStorageItemsReq request, RequestProceedResultDelegate<IncreaseStorageItemsResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Error, new IncreaseStorageItemsResp()
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
                result.Invoke(AckResponseCode.Error, new IncreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_STORAGE_WILL_OVERWHELMING
                });
                return;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
            result.Invoke(AckResponseCode.Success, new IncreaseStorageItemsResp()
            {
                Error = UITextKeys.NONE,
                StorageCharacterItems = storageItemList,
            });
#endif
        }

        public async UniTaskVoid DecreaseStorageItems(RequestHandlerData requestHandler, DecreaseStorageItemsReq request, RequestProceedResultDelegate<DecreaseStorageItemsResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            // Prepare storage data
            StorageId storageId = new StorageId(request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                result.Invoke(AckResponseCode.Error, new DecreaseStorageItemsResp()
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
                result.Invoke(AckResponseCode.Error, new DecreaseStorageItemsResp()
                {
                    Error = UITextKeys.UI_ERROR_NOT_ENOUGH_ITEMS,
                });
                return;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems(request.StorageType, request.StorageOwnerId, storageItemList);
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
#endif
        }

        public async UniTaskVoid MailList(RequestHandlerData requestHandler, MailListReq request, RequestProceedResultDelegate<MailListResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new MailListResp()
            {
                List = await Database.MailList(request.UserId, request.OnlyNewMails)
            });
#endif
        }

        public async UniTaskVoid UpdateReadMailState(RequestHandlerData requestHandler, UpdateReadMailStateReq request, RequestProceedResultDelegate<UpdateReadMailStateResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long updated = await Database.UpdateReadMailState(request.MailId, request.UserId);
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
                Mail = await Database.GetMail(request.MailId, request.UserId)
            });
#endif
        }

        public async UniTaskVoid UpdateClaimMailItemsState(RequestHandlerData requestHandler, UpdateClaimMailItemsStateReq request, RequestProceedResultDelegate<UpdateClaimMailItemsStateResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long updated = await Database.UpdateClaimMailItemsState(request.MailId, request.UserId);
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
                Mail = await Database.GetMail(request.MailId, request.UserId)
            });
#endif
        }

        public async UniTaskVoid UpdateDeleteMailState(RequestHandlerData requestHandler, UpdateDeleteMailStateReq request, RequestProceedResultDelegate<UpdateDeleteMailStateResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            long updated = await Database.UpdateDeleteMailState(request.MailId, request.UserId);
            if (updated <= 0)
            {
                result.Invoke(AckResponseCode.Error, new UpdateDeleteMailStateResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_READ_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new UpdateDeleteMailStateResp());
#endif
        }

        public async UniTaskVoid SendMail(RequestHandlerData requestHandler, SendMailReq request, RequestProceedResultDelegate<SendMailResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Mail mail = request.Mail;
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                result.Invoke(AckResponseCode.Error, new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NO_RECEIVER
                });
                return;
            }
            long created = await Database.CreateMail(mail);
            if (created <= 0)
            {
                result.Invoke(AckResponseCode.Error, new SendMailResp()
                {
                    Error = UITextKeys.UI_ERROR_MAIL_SEND_NOT_ALLOWED
                });
                return;
            }
            result.Invoke(AckResponseCode.Success, new SendMailResp());
#endif
        }

        public async UniTaskVoid GetMail(RequestHandlerData requestHandler, GetMailReq request, RequestProceedResultDelegate<GetMailResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GetMailResp()
            {
                Mail = await Database.GetMail(request.MailId, request.UserId),
            });
#endif
        }

        public async UniTaskVoid GetIdByCharacterName(RequestHandlerData requestHandler, GetIdByCharacterNameReq request, RequestProceedResultDelegate<GetIdByCharacterNameResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GetIdByCharacterNameResp()
            {
                Id = await Database.GetIdByCharacterName(request.CharacterName),
            });
#endif
        }

        public async UniTaskVoid GetUserIdByCharacterName(RequestHandlerData requestHandler, GetUserIdByCharacterNameReq request, RequestProceedResultDelegate<GetUserIdByCharacterNameResp> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            result.Invoke(AckResponseCode.Success, new GetUserIdByCharacterNameResp()
            {
                UserId = await Database.GetUserIdByCharacterName(request.CharacterName),
            });
#endif
        }

        public async UniTask<int> ReadGold(string userId)
        {
            int gold;
            if (!cachedUserGold.TryGetValue(userId, out gold))
            {
                // Doesn't cached yet, so get data from database and cache it
                gold = await Database.GetGold(userId);
                cachedUserGold[userId] = gold;
            }
            return gold;
        }

        public async UniTask<int> ReadCash(string userId)
        {
            int cash;
            if (!cachedUserCash.TryGetValue(userId, out cash))
            {
                // Doesn't cached yet, so get data from database and cache it
                cash = await Database.GetCash(userId);
                cachedUserCash[userId] = cash;
            }
            return cash;
        }

        public async UniTask<PlayerCharacterData> ReadCharacter(string id)
        {
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = await Database.ReadCharacter(id);
                // Cache character, it will be used to validate later
                if (character != null)
                {
                    cachedUserCharacter[id] = character;
                    cachedCharacterNames.Add(character.CharacterName);
                }
            }
            return character;
        }

        public async UniTask<SocialCharacterData> ReadSocialCharacter(string id)
        {
            //cachedSocialCharacter
            SocialCharacterData character;
            if (!cachedSocialCharacter.TryGetValue(id, out character))
            {
                // Doesn't cached yet, so get data from database
                character = SocialCharacterData.Create(await Database.ReadCharacter(id, false, false, false, false, false, false, false, false, false, false));
                // Cache the data
                cachedSocialCharacter[id] = character;
            }
            return character;
        }

        public async UniTask<List<SocialCharacterData>> ReadFriends(string id)
        {
            List<SocialCharacterData> friends;
            if (!cachedFriend.TryGetValue(id, out friends))
            {
                // Doesn't cached yet, so get data from database
                friends = await Database.ReadFriends(id);
                // Cache the data
                if (friends != null)
                {
                    cachedFriend[id] = friends;
                    CacheSocialCharacters(friends);
                }
            }
            return friends;
        }

        public async UniTask<PartyData> ReadParty(int id)
        {
            PartyData party;
            if (!cachedParty.TryGetValue(id, out party))
            {
                // Doesn't cached yet, so get data from database
                party = await Database.ReadParty(id);
                // Cache the data
                if (party != null)
                {
                    cachedParty[id] = party;
                    CacheSocialCharacters(party.GetMembers());
                }
            }
            return party;
        }

        public async UniTask<GuildData> ReadGuild(int id)
        {
            GuildData guild;
            if (!cachedGuild.TryGetValue(id, out guild))
            {
                // Doesn't cached yet, so get data from database
                guild = await Database.ReadGuild(id, GameInstance.Singleton.SocialSystemSetting.GuildMemberRoles);
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
    }
}