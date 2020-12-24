#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;
using System.Collections.Concurrent;
using ConcurrentCollections;
using System;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class DatabaseServiceImplement : DatabaseService.DatabaseServiceBase
    {
        public delegate Task<CustomResp> CustomRequestDelegate(int type, ByteString data);
        public static CustomRequestDelegate onCustomRequest;
        public BaseDatabase Database { get; private set; }
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

        public DatabaseServiceImplement(BaseDatabase database)
        {
            Database = database;
            database.Initialize();
        }

        public override async Task<ValidateUserLoginResp> ValidateUserLogin(ValidateUserLoginReq request, ServerCallContext context)
        {
            string userId = await Database.ValidateUserLogin(request.Username, request.Password);
            return new ValidateUserLoginResp()
            {
                UserId = userId
            };
        }

        public override async Task<ValidateAccessTokenResp> ValidateAccessToken(ValidateAccessTokenReq request, ServerCallContext context)
        {
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
            return new ValidateAccessTokenResp()
            {
                IsPass = isPass
            };
        }

        public override async Task<GetUserLevelResp> GetUserLevel(GetUserLevelReq request, ServerCallContext context)
        {
            byte userLevel = await Database.GetUserLevel(request.UserId);
            return new GetUserLevelResp()
            {
                UserLevel = userLevel
            };
        }

        public override async Task<GoldResp> GetGold(GetGoldReq request, ServerCallContext context)
        {
            return new GoldResp()
            {
                Gold = await ReadGold(request.UserId)
            };
        }

        public override async Task<GoldResp> ChangeGold(ChangeGoldReq request, ServerCallContext context)
        {
            int gold = await ReadGold(request.UserId);
            gold += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserGold[request.UserId] = gold;
            // Update data to database
            await Database.UpdateGold(request.UserId, gold);
            return new GoldResp()
            {
                Gold = gold
            };
        }

        public override async Task<CashResp> GetCash(GetCashReq request, ServerCallContext context)
        {
            return new CashResp()
            {
                Cash = await ReadCash(request.UserId)
            };
        }

        public override async Task<CashResp> ChangeCash(ChangeCashReq request, ServerCallContext context)
        {
            int cash = await ReadCash(request.UserId);
            cash += request.ChangeAmount;
            // Cache the data, it will be used later
            cachedUserCash[request.UserId] = cash;
            // Update data to database
            await Database.UpdateCash(request.UserId, cash);
            return new CashResp()
            {
                Cash = cash
            };
        }

        public override async Task<VoidResp> UpdateAccessToken(UpdateAccessTokenReq request, ServerCallContext context)
        {
            // Store access token to the dictionary, it will be used to validate later
            cachedUserAccessToken[request.UserId] = request.AccessToken;
            // Update data to database
            await Database.UpdateAccessToken(request.UserId, request.AccessToken);
            return new VoidResp();
        }

        public override async Task<VoidResp> CreateUserLogin(CreateUserLoginReq request, ServerCallContext context)
        {
            // Cache username, it will be used to validate later
            cachedUsernames.Add(request.Username);
            // Insert new user login to database
            await Database.CreateUserLogin(request.Username, request.Password);
            return new VoidResp();
        }

        public override async Task<FindUsernameResp> FindUsername(FindUsernameReq request, ServerCallContext context)
        {
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
            return new FindUsernameResp()
            {
                FoundAmount = foundAmount
            };
        }

        public override async Task<CharacterResp> CreateCharacter(CreateCharacterReq request, ServerCallContext context)
        {
            PlayerCharacterData character = DatabaseServiceUtils.FromByteString<PlayerCharacterData>(request.CharacterData);
            // Store character to the dictionary, it will be used later
            cachedUserCharacter[character.Id] = character;
            cachedCharacterNames.Add(character.CharacterName);
            // Insert new character to database
            await Database.CreateCharacter(request.UserId, character);
            return new CharacterResp()
            {
                CharacterData = DatabaseServiceUtils.ToByteString(character)
            };
        }

        public override async Task<CharacterResp> ReadCharacter(ReadCharacterReq request, ServerCallContext context)
        {
            return new CharacterResp()
            {
                CharacterData = DatabaseServiceUtils.ToByteString(await ReadCharacter(request.CharacterId))
            };
        }

        public override async Task<CharactersResp> ReadCharacters(ReadCharactersReq request, ServerCallContext context)
        {
            CharactersResp resp = new CharactersResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(await Database.ReadCharacters(request.UserId), resp.List);
            return resp;
        }

        public override async Task<CharacterResp> UpdateCharacter(UpdateCharacterReq request, ServerCallContext context)
        {
            PlayerCharacterData character = DatabaseServiceUtils.FromByteString<PlayerCharacterData>(request.CharacterData);
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
            return new CharacterResp()
            {
                CharacterData = DatabaseServiceUtils.ToByteString(character)
            };
        }

        public override async Task<VoidResp> DeleteCharacter(DeleteCharacterReq request, ServerCallContext context)
        {
            // Remove data from cache
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
            {
                string characterName = cachedUserCharacter[request.CharacterId].CharacterName;
                cachedCharacterNames.TryRemove(characterName);
                cachedUserCharacter.TryRemove(request.CharacterId, out _);
            }
            // Delete data from database
            await Database.DeleteCharacter(request.UserId, request.CharacterId);
            return new VoidResp();
        }

        public override async Task<FindCharacterNameResp> FindCharacterName(FindCharacterNameReq request, ServerCallContext context)
        {
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
            return new FindCharacterNameResp()
            {
                FoundAmount = foundAmount
            };
        }

        public override async Task<FindCharactersResp> FindCharacters(FindCharactersReq request, ServerCallContext context)
        {
            FindCharactersResp resp = new FindCharactersResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(await Database.FindCharacters(request.CharacterName), resp.List);
            return resp;
        }

        public override async Task<ReadFriendsResp> CreateFriend(CreateFriendReq request, ServerCallContext context)
        {
            List<SocialCharacterData> friends = await ReadFriends(request.Character1Id);
            // Update to cache
            SocialCharacterData character = await ReadSocialCharacter(request.Character2Id);
            friends.Add(character);
            cachedFriend[request.Character1Id] = friends;
            // Update to database
            await Database.CreateFriend(request.Character1Id, character.id);
            ReadFriendsResp resp = new ReadFriendsResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(friends, resp.List);
            return resp;
        }

        public override async Task<ReadFriendsResp> DeleteFriend(DeleteFriendReq request, ServerCallContext context)
        {
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
            // Update to database
            await Database.DeleteFriend(request.Character1Id, request.Character2Id);
            ReadFriendsResp resp = new ReadFriendsResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(friends, resp.List);
            return resp;
        }

        public override async Task<ReadFriendsResp> ReadFriends(ReadFriendsReq request, ServerCallContext context)
        {
            ReadFriendsResp resp = new ReadFriendsResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(await ReadFriends(request.CharacterId), resp.List);
            return resp;
        }

        public override async Task<BuildingResp> CreateBuilding(CreateBuildingReq request, ServerCallContext context)
        {
            BuildingSaveData building = DatabaseServiceUtils.FromByteString<BuildingSaveData>(request.BuildingData);
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
            return new BuildingResp()
            {
                BuildingData = request.BuildingData
            };
        }

        public override async Task<BuildingResp> UpdateBuilding(UpdateBuildingReq request, ServerCallContext context)
        {
            BuildingSaveData building = DatabaseServiceUtils.FromByteString<BuildingSaveData>(request.BuildingData);
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
            return new BuildingResp()
            {
                BuildingData = request.BuildingData
            };
        }

        public override async Task<VoidResp> DeleteBuilding(DeleteBuildingReq request, ServerCallContext context)
        {
            // Remove from cache
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName].TryRemove(request.BuildingId, out _);
            // Remove from database
            await Database.DeleteBuilding(request.MapName, request.BuildingId);
            return new VoidResp();
        }

        public override async Task<BuildingsResp> ReadBuildings(ReadBuildingsReq request, ServerCallContext context)
        {
            BuildingsResp resp = new BuildingsResp();
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
            DatabaseServiceUtils.CopyToRepeatedByteString(buildings, resp.List);
            return resp;
        }

        public override async Task<PartyResp> CreateParty(CreatePartyReq request, ServerCallContext context)
        {
            // Insert to database
            int partyId = await Database.CreateParty(request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            // Cached the data
            PartyData party = new PartyData(partyId, request.ShareExp, request.ShareItem, request.LeaderCharacterId);
            cachedParty[partyId] = party;
            return new PartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(party)
            };
        }

        public override async Task<PartyResp> UpdateParty(UpdatePartyReq request, ServerCallContext context)
        {
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            party.Setting(request.ShareExp, request.ShareItem);
            cachedParty[request.PartyId] = party;
            // Update to database
            await Database.UpdateParty(request.PartyId, request.ShareExp, request.ShareItem);
            return new PartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(party)
            };
        }

        public override async Task<PartyResp> UpdatePartyLeader(UpdatePartyLeaderReq request, ServerCallContext context)
        {
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            party.SetLeader(request.LeaderCharacterId);
            cachedParty[request.PartyId] = party;
            // Update to database
            await Database.UpdatePartyLeader(request.PartyId, request.LeaderCharacterId);
            return new PartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(party)
            };
        }

        public override async Task<VoidResp> DeleteParty(DeletePartyReq request, ServerCallContext context)
        {
            await Database.DeleteParty(request.PartyId);
            return new VoidResp();
        }

        public override async Task<PartyResp> UpdateCharacterParty(UpdateCharacterPartyReq request, ServerCallContext context)
        {
            PartyData party = await ReadParty(request.PartyId);
            // Update to cache
            SocialCharacterData character = DatabaseServiceUtils.FromByteString<SocialCharacterData>(request.SocialCharacterData);
            party.AddMember(character);
            cachedParty[request.PartyId] = party;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].PartyId = request.PartyId;
            // Update to database
            await Database.UpdateCharacterParty(character.id, request.PartyId);
            return new PartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(party)
            };
        }

        public override async Task<VoidResp> ClearCharacterParty(ClearCharacterPartyReq request, ServerCallContext context)
        {
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
            return new VoidResp();
        }

        public override async Task<PartyResp> ReadParty(ReadPartyReq request, ServerCallContext context)
        {
            return new PartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(await ReadParty(request.PartyId))
            };
        }

        public override async Task<GuildResp> CreateGuild(CreateGuildReq request, ServerCallContext context)
        {
            // Insert to database
            int guildId = await Database.CreateGuild(request.GuildName, request.LeaderCharacterId);
            // Cached the data
            GuildData guild = new GuildData(guildId, request.GuildName, request.LeaderCharacterId);
            cachedGuild[guildId] = guild;
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildResp> UpdateGuildLeader(UpdateGuildLeaderReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetLeader(request.LeaderCharacterId);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildLeader(request.GuildId, request.LeaderCharacterId);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildResp> UpdateGuildMessage(UpdateGuildMessageReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.guildMessage = request.GuildMessage;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildMessage(request.GuildId, request.GuildMessage);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildResp> UpdateGuildRole(UpdateGuildRoleReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetRole((byte)request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, (byte)request.ShareExpPercentage);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildRole(request.GuildId, (byte)request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, (byte)request.ShareExpPercentage);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildResp> UpdateGuildMemberRole(UpdateGuildMemberRoleReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.SetMemberRole(request.MemberCharacterId, (byte)request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildMemberRole(request.MemberCharacterId, (byte)request.GuildRole);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<VoidResp> DeleteGuild(DeleteGuildReq request, ServerCallContext context)
        {
            // Remove data from cache
            if (cachedGuild.ContainsKey(request.GuildId))
            {
                string guildName = cachedGuild[request.GuildId].guildName;
                cachedGuildNames.TryRemove(guildName);
                cachedGuild.TryRemove(request.GuildId, out _);
            }
            // Remove data from database
            await Database.DeleteGuild(request.GuildId);
            return new VoidResp();
        }

        public override async Task<GuildResp> UpdateCharacterGuild(UpdateCharacterGuildReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            SocialCharacterData character = DatabaseServiceUtils.FromByteString<SocialCharacterData>(request.SocialCharacterData);
            guild.AddMember(character, (byte)request.GuildRole);
            cachedGuild[request.GuildId] = guild;
            // Update to cached character
            if (cachedUserCharacter.ContainsKey(character.id))
                cachedUserCharacter[character.id].GuildId = request.GuildId;
            // Update to database
            await Database.UpdateCharacterGuild(character.id, request.GuildId, (byte)request.GuildRole);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<VoidResp> ClearCharacterGuild(ClearCharacterGuildReq request, ServerCallContext context)
        {
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
            return new VoidResp();
        }

        public override async Task<FindGuildNameResp> FindGuildName(FindGuildNameReq request, ServerCallContext context)
        {
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
            return new FindGuildNameResp()
            {
                FoundAmount = foundAmount
            };
        }

        public override async Task<GuildResp> ReadGuild(ReadGuildReq request, ServerCallContext context)
        {
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(await ReadGuild(request.GuildId))
            };
        }

        public override async Task<GuildResp> IncreaseGuildExp(IncreaseGuildExpReq request, ServerCallContext context)
        {
            // TODO: May validate guild by character
            GuildData guild = await ReadGuild(request.GuildId);
            await UniTask.SwitchToMainThread();
            guild = GameInstance.Singleton.SocialSystemSetting.IncreaseGuildExp(guild, request.Exp);
            // Update to cache
            cachedGuild.TryAdd(guild.id, guild);
            // Update to database
            await Database.UpdateGuildLevel(request.GuildId, guild.level, guild.exp, guild.skillPoint);
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildResp> AddGuildSkill(AddGuildSkillReq request, ServerCallContext context)
        {
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
            return new GuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(guild)
            };
        }

        public override async Task<GuildGoldResp> GetGuildGold(GetGuildGoldReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            return new GuildGoldResp()
            {
                GuildGold = guild.gold
            };
        }

        public override async Task<GuildGoldResp> ChangeGuildGold(ChangeGuildGoldReq request, ServerCallContext context)
        {
            GuildData guild = await ReadGuild(request.GuildId);
            // Update to cache
            guild.gold += request.ChangeAmount;
            cachedGuild[request.GuildId] = guild;
            // Update to database
            await Database.UpdateGuildGold(request.GuildId, guild.gold);
            return new GuildGoldResp()
            {
                GuildGold = guild.gold
            };
        }

        public override async Task<ReadStorageItemsResp> ReadStorageItems(ReadStorageItemsReq request, ServerCallContext context)
        {
            ReadStorageItemsResp resp = new ReadStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItems;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItems))
            {
                // Doesn't cached yet, so get data from database
                storageItems = await Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
                // Cache data, it will be used to validate later
                if (storageItems != null)
                    cachedStorageItems[storageId] = storageItems;
            }
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItems, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<MoveItemToStorageResp> MoveItemToStorage(MoveItemToStorageReq request, ServerCallContext context)
        {
            MoveItemToStorageResp resp = new MoveItemToStorageResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                resp.Error = EStorageError.StorageErrorInvalidCharacter;
                return resp;
            }
            if (request.InventoryItemIndex < 0 ||
                request.InventoryItemIndex >= character.NonEquipItems.Count)
            {
                // Invalid inventory index
                resp.Error = EStorageError.StorageErrorInvalidInventoryIndex;
                return resp;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitWeight = request.WeightLimit > 0;
            bool isLimitSlot = request.SlotLimit > 0;
            short weightLimit = (short)request.WeightLimit;
            short slotLimit = (short)request.SlotLimit;
            // Prepare character and item data
            CharacterItem movingItem = character.NonEquipItems[request.InventoryItemIndex].Clone(true);
            movingItem.id = GenericUtils.GetUniqueId();
            movingItem.amount = (short)request.InventoryItemAmount;
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
                    resp.Error = EStorageError.StorageErrorStorageWillOverwhelming;
                    return resp;
                }
                // Remove from inventory
                character.DecreaseItemsByIndex(request.InventoryItemIndex, (short)request.InventoryItemAmount);
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
            await Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(character.NonEquipItems, resp.InventoryItemItems);
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<MoveItemFromStorageResp> MoveItemFromStorage(MoveItemFromStorageReq request, ServerCallContext context)
        {
            MoveItemFromStorageResp resp = new MoveItemFromStorageResp();
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            PlayerCharacterData character;
            if (!cachedUserCharacter.TryGetValue(request.CharacterId, out character))
            {
                // Cannot find character
                resp.Error = EStorageError.StorageErrorInvalidCharacter;
                return resp;
            }
            if (request.StorageItemIndex < 0 ||
                request.StorageItemIndex >= storageItemList.Count)
            {
                // Invalid storage index
                resp.Error = EStorageError.StorageErrorInvalidStorageIndex;
                return resp;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = (short)request.SlotLimit;
            // Prepare item data
            CharacterItem movingItem = storageItemList[request.StorageItemIndex].Clone(true);
            movingItem.amount = (short)request.StorageItemAmount;
            if (request.InventoryItemIndex < 0 ||
                request.InventoryItemIndex >= character.NonEquipItems.Count ||
                character.NonEquipItems[request.InventoryItemIndex].dataId == movingItem.dataId)
            {
                // Add to inventory or merge
                bool isOverwhelming = character.IncreasingItemsWillOverwhelming(movingItem.dataId, movingItem.amount);
                if (isOverwhelming || !character.IncreaseItems(movingItem))
                {
                    // inventory will overwhelming
                    resp.Error = EStorageError.StorageErrorInventoryWillOverwhelming;
                    return resp;
                }
                // Remove from storage
                storageItemList.DecreaseItemsByIndex(request.StorageItemIndex, (short)request.StorageItemAmount, isLimitSlot);
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
            await Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(character.NonEquipItems, resp.InventoryItemItems);
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<SwapOrMergeStorageItemResp> SwapOrMergeStorageItem(SwapOrMergeStorageItemReq request, ServerCallContext context)
        {
            SwapOrMergeStorageItemResp resp = new SwapOrMergeStorageItemResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = (short)request.SlotLimit;
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
            await Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<IncreaseStorageItemsResp> IncreaseStorageItems(IncreaseStorageItemsReq request, ServerCallContext context)
        {
            IncreaseStorageItemsResp resp = new IncreaseStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitWeight = request.WeightLimit > 0;
            bool isLimitSlot = request.SlotLimit > 0;
            short weightLimit = (short)request.WeightLimit;
            short slotLimit = (short)request.SlotLimit;
            CharacterItem addingItem = DatabaseServiceUtils.FromByteString<CharacterItem>(request.Item);
            // Increase item to storage
            bool isOverwhelming = storageItemList.IncreasingItemsWillOverwhelming(
                addingItem.dataId, addingItem.amount, isLimitWeight, weightLimit,
                storageItemList.GetTotalItemWeight(), isLimitSlot, slotLimit);
            if (isOverwhelming || !storageItemList.IncreaseItems(addingItem))
            {
                // Storage will overwhelming
                resp.Error = EStorageError.StorageErrorStorageWillOverwhelming;
                return resp;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<DecreaseStorageItemsResp> DecreaseStorageItems(DecreaseStorageItemsReq request, ServerCallContext context)
        {
            DecreaseStorageItemsResp resp = new DecreaseStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItemList;
            if (!cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            await UniTask.SwitchToMainThread();
            bool isLimitSlot = request.SlotLimit > 0;
            short slotLimit = (short)request.SlotLimit;
            // Increase item to storage
            Dictionary<int, short> decreaseItems;
            if (!storageItemList.DecreaseItems(request.DataId, (short)request.Amount, isLimitSlot, out decreaseItems))
            {
                resp.Error = EStorageError.StorageErrorDecreaseItemNotEnough;
                return resp;
            }
            storageItemList.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage list
            // TODO: May update later to reduce amount of processes
            await Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            foreach (int itemIndex in decreaseItems.Keys)
            {
                resp.DecreasedItems.Add(new ItemIndexAmountMap()
                {
                    Index = itemIndex,
                    Amount = decreaseItems[itemIndex]
                });
            }
            return resp;
        }

        public override async Task<MailListResp> MailList(MailListReq request, ServerCallContext context)
        {
            MailListResp resp = new MailListResp();
            List<MailListEntry> list = await Database.MailList(request.UserId, request.OnlyNewMails);
            DatabaseServiceUtils.CopyToRepeatedByteString(list, resp.List);
            return resp;
        }

        public override async Task<UpdateReadMailStateResp> UpdateReadMailState(UpdateReadMailStateReq request, ServerCallContext context)
        {
            UpdateReadMailStateResp resp = new UpdateReadMailStateResp();
            long updated = await Database.UpdateReadMailState(request.MailId, request.UserId);
            if (updated > 0)
                resp.Mail = DatabaseServiceUtils.ToByteString(await Database.GetMail(request.MailId, request.UserId));
            else
                resp.Error = EUpdateReadMailStateError.ReadMailErrorNotAllowed;
            return resp;
        }

        public override async Task<UpdateClaimMailItemsStateResp> UpdateClaimMailItemsState(UpdateClaimMailItemsStateReq request, ServerCallContext context)
        {
            UpdateClaimMailItemsStateResp resp = new UpdateClaimMailItemsStateResp();
            long updated = await Database.UpdateClaimMailItemsState(request.MailId, request.UserId);
            if (updated > 0)
                resp.Mail = DatabaseServiceUtils.ToByteString(await Database.GetMail(request.MailId, request.UserId));
            else
                resp.Error = EUpdateClaimMailItemsStateError.ClaimMailItemsErrorNotAllowed;
            return resp;
        }

        public override async Task<UpdateDeleteMailStateResp> UpdateDeleteMailState(UpdateDeleteMailStateReq request, ServerCallContext context)
        {
            UpdateDeleteMailStateResp resp = new UpdateDeleteMailStateResp();
            long updated = await Database.UpdateDeleteMailState(request.MailId, request.UserId);
            if (updated <= 0)
                resp.Error = EUpdateDeleteMailStateError.DeleteMailErrorNotAllowed;
            return resp;
        }

        public override async Task<SendMailResp> SendMail(SendMailReq request, ServerCallContext context)
        {
            Mail mail = DatabaseServiceUtils.FromByteString<Mail>(request.Mail);
            if (string.IsNullOrEmpty(mail.ReceiverId))
            {
                return new SendMailResp()
                {
                    Error = ESendMailError.SendMailErrorNoReceiver,
                };
            }
            await Database.CreateMail(mail);
            return new SendMailResp();
        }

        public override async Task<GetMailResp> GetMail(GetMailReq request, ServerCallContext context)
        {
            return new GetMailResp()
            {
                Mail = DatabaseServiceUtils.ToByteString(await Database.GetMail(request.MailId, request.UserId)),
            };
        }

        public override async Task<GetIdByCharacterNameResp> GetIdByCharacterName(GetIdByCharacterNameReq request, ServerCallContext context)
        {
            return new GetIdByCharacterNameResp()
            {
                Id = await Database.GetIdByCharacterName(request.CharacterName),
            };
        }

        public override async Task<GetUserIdByCharacterNameResp> GetUserIdByCharacterName(GetUserIdByCharacterNameReq request, ServerCallContext context)
        {
            return new GetUserIdByCharacterNameResp()
            {
                UserId = await Database.GetUserIdByCharacterName(request.CharacterName),
            };
        }

        public override async Task<CustomResp> Custom(CustomReq request, ServerCallContext context)
        {
            return await onCustomRequest.Invoke(request.Type, request.Data);
        }

        public async Task<int> ReadGold(string userId)
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

        public async Task<int> ReadCash(string userId)
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

        public async Task<PlayerCharacterData> ReadCharacter(string id)
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

        public async Task<SocialCharacterData> ReadSocialCharacter(string id)
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

        public async Task<List<SocialCharacterData>> ReadFriends(string id)
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

        public async Task<PartyData> ReadParty(int id)
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

        public async Task<GuildData> ReadGuild(int id)
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
#endif