using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf;

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
        private HashSet<string> cachedUsernames = new HashSet<string>();
        private HashSet<string> cachedCharacterNames = new HashSet<string>();
        private Dictionary<string, string> cachedUserAccessToken = new Dictionary<string, string>();
        private Dictionary<string, int> cachedUserGold = new Dictionary<string, int>();
        private Dictionary<string, int> cachedUserCash = new Dictionary<string, int>();
        private Dictionary<string, PlayerCharacterData> cachedUserCharacter = new Dictionary<string, PlayerCharacterData>();
        private Dictionary<string, Dictionary<string, BuildingSaveData>> cachedBuilding = new Dictionary<string, Dictionary<string, BuildingSaveData>>();
        private Dictionary<StorageId, List<CharacterItem>> cachedStorageItems = new Dictionary<StorageId, List<CharacterItem>>();

        public DatabaseServiceImplement(BaseDatabase database)
        {
            Database = database;
        }

        public override async Task<ValidateUserLoginResp> ValidateUserLogin(ValidateUserLoginReq request, ServerCallContext context)
        {
            await Task.Yield();
            string userId = Database.ValidateUserLogin(request.Username, request.Password);
            return new ValidateUserLoginResp()
            {
                UserId = userId
            };
        }

        public override async Task<ValidateAccessTokenResp> ValidateAccessToken(ValidateAccessTokenReq request, ServerCallContext context)
        {
            await Task.Yield();
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
            return new ValidateAccessTokenResp()
            {
                IsPass = isPass
            };
        }

        public override async Task<GetUserLevelResp> GetUserLevel(GetUserLevelReq request, ServerCallContext context)
        {
            await Task.Yield();
            byte userLevel = Database.GetUserLevel(request.UserId);
            return new GetUserLevelResp()
            {
                UserLevel = userLevel
            };
        }

        public override async Task<GoldResp> GetGold(GetGoldReq request, ServerCallContext context)
        {
            await Task.Yield();
            int gold;
            if (cachedUserGold.ContainsKey(request.UserId))
            {
                // Already cached data, so get data from cache
                gold = cachedUserGold[request.UserId];
            }
            else
            {
                // Doesn't cached yet, so get data from database and cache it
                gold = Database.GetGold(request.UserId);
                cachedUserGold[request.UserId] = gold;
            }
            return new GoldResp()
            {
                Gold = gold
            };
        }

        public override async Task<VoidResp> UpdateGold(UpdateGoldReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Cache the data, it will be used later
            cachedUserGold[request.UserId] = request.Amount;
            // Update data to database
            Database.UpdateGold(request.UserId, request.Amount);
            return new VoidResp();
        }

        public override async Task<CashResp> GetCash(GetCashReq request, ServerCallContext context)
        {
            await Task.Yield();
            int cash;
            if (cachedUserCash.ContainsKey(request.UserId))
            {
                // Already cached data, so get data from cache
                cash = cachedUserCash[request.UserId];
            }
            else
            {
                // Doesn't cached yet, so get data from database and cache it
                cash = Database.GetCash(request.UserId);
                cachedUserCash[request.UserId] = cash;
            }
            return new CashResp()
            {
                Cash = cash
            };
        }

        public override async Task<VoidResp> UpdateCash(UpdateCashReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Cache the data, it will be used later
            cachedUserCash[request.UserId] = request.Amount;
            // Update data to database
            Database.UpdateCash(request.UserId, request.Amount);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateAccessToken(UpdateAccessTokenReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Store access token to the dictionary, it will be used to validate later
            cachedUserAccessToken[request.UserId] = request.AccessToken;
            // Update data to database
            Database.UpdateAccessToken(request.UserId, request.AccessToken);
            return new VoidResp();
        }

        public override async Task<VoidResp> CreateUserLogin(CreateUserLoginReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Cache username, it will be used to validate later
            cachedUsernames.Add(request.Username);
            // Insert new user login to database
            Database.CreateUserLogin(request.Username, request.Password);
            return new VoidResp();
        }

        public override async Task<FindUsernameResp> FindUsername(FindUsernameReq request, ServerCallContext context)
        {
            await Task.Yield();
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
                cachedUsernames.Add(request.Username);
            }
            return new FindUsernameResp()
            {
                FoundAmount = foundAmount
            };
        }

        public override async Task<VoidResp> CreateCharacter(CreateCharacterReq request, ServerCallContext context)
        {
            await Task.Yield();
            PlayerCharacterData character = DatabaseServiceUtils.FromByteString<PlayerCharacterData>(request.CharacterData);
            // Store character to the dictionary, it will be used later
            cachedUserCharacter[request.UserId] = character;
            cachedCharacterNames.Add(character.CharacterName);
            // Insert new character to database
            Database.CreateCharacter(request.UserId, character);
            return new VoidResp();
        }

        public override async Task<ReadCharacterResp> ReadCharacter(ReadCharacterReq request, ServerCallContext context)
        {
            await Task.Yield();
            PlayerCharacterData character;
            if (cachedUserCharacter.ContainsKey(request.UserId))
            {
                // Already cached data, so get data from cache
                character = cachedUserCharacter[request.UserId];
            }
            else
            {
                // Doesn't cached yet, so get data from database
                character = Database.ReadCharacter(request.UserId, request.CharacterId);
                // Cache character, it will be used to validate later
                if (character != null)
                {
                    cachedUserCharacter[request.UserId] = character;
                    cachedCharacterNames.Add(character.CharacterName);
                }
            }
            return new ReadCharacterResp()
            {
                CharacterData = DatabaseServiceUtils.ToByteString(character)
            };
        }

        public override async Task<ReadCharactersResp> ReadCharacters(ReadCharactersReq request, ServerCallContext context)
        {
            await Task.Yield();
            ReadCharactersResp resp = new ReadCharactersResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(Database.ReadCharacters(request.UserId), resp.List);
            return resp;
        }

        public override async Task<VoidResp> UpdateCharacter(UpdateCharacterReq request, ServerCallContext context)
        {
            await Task.Yield();
            PlayerCharacterData character = DatabaseServiceUtils.FromByteString<PlayerCharacterData>(request.CharacterData);
            // Cache the data, it will be used later
            cachedUserCharacter[character.Id] = character;
            // Update data to database
            // TODO: May update later to reduce amount of processes
            Database.UpdateCharacter(character);
            return new VoidResp();
        }

        public override async Task<VoidResp> DeleteCharacter(DeleteCharacterReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Remove data from cache
            if (cachedUserCharacter.ContainsKey(request.CharacterId))
            {
                string characterName = cachedUserCharacter[request.CharacterId].CharacterName;
                cachedCharacterNames.Remove(characterName);
                cachedUserCharacter.Remove(request.CharacterId);
            }
            // Delete data from database
            Database.DeleteCharacter(request.UserId, request.CharacterId);
            return new VoidResp();
        }

        public override async Task<FindCharacterNameResp> FindCharacterName(FindCharacterNameReq request, ServerCallContext context)
        {
            await Task.Yield();
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
                cachedCharacterNames.Add(request.CharacterName);
            }
            return new FindCharacterNameResp()
            {
                FoundAmount = foundAmount
            };
        }

        public override async Task<FindCharactersResp> FindCharacters(FindCharactersReq request, ServerCallContext context)
        {
            await Task.Yield();
            FindCharactersResp resp = new FindCharactersResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(Database.FindCharacters(request.CharacterName), resp.List);
            return resp;
        }

        public override async Task<VoidResp> CreateFriend(CreateFriendReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.CreateFriend(request.CharacterId1, request.CharacterId2);
            return new VoidResp();
        }

        public override async Task<VoidResp> DeleteFriend(DeleteFriendReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.DeleteFriend(request.CharacterId1, request.CharacterId2);
            return new VoidResp();
        }

        public override async Task<ReadFriendsResp> ReadFriends(ReadFriendsReq request, ServerCallContext context)
        {
            await Task.Yield();
            ReadFriendsResp resp = new ReadFriendsResp();
            DatabaseServiceUtils.CopyToRepeatedByteString(Database.ReadFriends(request.CharacterId), resp.List);
            return resp;
        }

        public override async Task<VoidResp> CreateBuilding(CreateBuildingReq request, ServerCallContext context)
        {
            await Task.Yield();
            BuildingSaveData building = DatabaseServiceUtils.FromByteString<BuildingSaveData>(request.BuildingData);
            // Cache building data
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName][building.Id] = building;
            // Insert data to database
            Database.CreateBuilding(request.MapName, building);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateBuilding(UpdateBuildingReq request, ServerCallContext context)
        {
            await Task.Yield();
            BuildingSaveData building = DatabaseServiceUtils.FromByteString<BuildingSaveData>(request.BuildingData);
            // Cache building data
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName][building.Id] = building;
            // Update data to database
            Database.UpdateBuilding(request.MapName, building);
            return new VoidResp();
        }

        public override async Task<VoidResp> DeleteBuilding(DeleteBuildingReq request, ServerCallContext context)
        {
            await Task.Yield();
            // Remove from cache
            if (cachedBuilding.ContainsKey(request.MapName))
                cachedBuilding[request.MapName].Remove(request.BuildingId);
            // Remove from database
            Database.DeleteBuilding(request.MapName, request.BuildingId);
            return new VoidResp();
        }

        public override async Task<ReadBuildingsResp> ReadBuildings(ReadBuildingsReq request, ServerCallContext context)
        {
            await Task.Yield();
            ReadBuildingsResp resp = new ReadBuildingsResp();
            List<BuildingSaveData> buildings;
            if (cachedBuilding.ContainsKey(request.MapName))
            {
                // Get buildings from cache
                buildings = new List<BuildingSaveData>(cachedBuilding[request.MapName].Values);
            }
            else
            {
                // Store buildings to cache
                cachedBuilding[request.MapName] = new Dictionary<string, BuildingSaveData>();
                buildings = Database.ReadBuildings(request.MapName);
                foreach (BuildingSaveData building in buildings)
                {
                    cachedBuilding[request.MapName][building.Id] = building;
                }
            }
            DatabaseServiceUtils.CopyToRepeatedByteString(buildings, resp.List);
            return resp;
        }

        public override async Task<CreatePartyResp> CreateParty(CreatePartyReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new CreatePartyResp()
            {
                PartyId = Database.CreateParty(request.ShareExp, request.ShareItem, request.LeaderCharacterId)
            };
        }

        public override async Task<VoidResp> UpdateParty(UpdatePartyReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateParty(request.PartyId, request.ShareExp, request.ShareItem);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdatePartyLeader(UpdatePartyLeaderReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdatePartyLeader(request.PartyId, request.LeaderCharacterId);
            return new VoidResp();
        }

        public override async Task<VoidResp> DeleteParty(DeletePartyReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.DeleteParty(request.PartyId);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateCharacterParty(UpdateCharacterPartyReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateCharacterParty(request.CharacterId, request.PartyId);
            return new VoidResp();
        }

        public override async Task<ReadPartyResp> ReadParty(ReadPartyReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new ReadPartyResp()
            {
                PartyData = DatabaseServiceUtils.ToByteString(Database.ReadParty(request.PartyId))
            };
        }

        public override async Task<CreateGuildResp> CreateGuild(CreateGuildReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new CreateGuildResp()
            {
                GuildId = Database.CreateGuild(request.GuildName, request.LeaderCharacterId)
            };
        }

        public override async Task<VoidResp> UpdateGuildLevel(UpdateGuildLevelReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildLevel(request.GuildId, (short)request.Level, request.Exp, (short)request.SkillPoint);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateGuildLeader(UpdateGuildLeaderReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildLeader(request.GuildId, request.LeaderCharacterId);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateGuildMessage(UpdateGuildMessageReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildMessage(request.GuildId, request.GuildMessage);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateGuildRole(UpdateGuildRoleReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildRole(request.GuildId, (byte)request.GuildRole, request.RoleName, request.CanInvite, request.CanKick, (byte)request.ShareExpPercentage);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateGuildMemberRole(UpdateGuildMemberRoleReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildMemberRole(request.MemberCharacterId, (byte)request.GuildRole);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateGuildSkillLevel(UpdateGuildSkillLevelReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildSkillLevel(request.GuildId, request.DataId, (short)request.SkillLevel, (short)request.SkillPoint);
            return new VoidResp();
        }

        public override async Task<VoidResp> UpdateCharacterGuild(UpdateCharacterGuildReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateCharacterGuild(request.CharacterId, request.GuildId, (byte)request.GuildRole);
            return new VoidResp();
        }

        public override async Task<VoidResp> DeleteGuild(DeleteGuildReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.DeleteGuild(request.GuildId);
            return new VoidResp();
        }

        public override async Task<FindGuildNameResp> FindGuildName(FindGuildNameReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new FindGuildNameResp()
            {
                FoundAmount = Database.FindGuildName(request.GuildName)
            };
        }

        public override async Task<ReadGuildResp> ReadGuild(ReadGuildReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new ReadGuildResp()
            {
                GuildData = DatabaseServiceUtils.ToByteString(Database.ReadGuild(request.GuildId, DatabaseServiceUtils.MakeArrayFromRepeatedByteString<GuildRoleData>(request.DefaultGuildRoles)))
            };
        }

        public override async Task<GuildGoldResp> GetGuildGold(GetGuildGoldReq request, ServerCallContext context)
        {
            await Task.Yield();
            return new GuildGoldResp()
            {
                GuildGold = Database.GetGuildGold(request.GuildId)
            };
        }

        public override async Task<VoidResp> UpdateGuildGold(UpdateGuildGoldReq request, ServerCallContext context)
        {
            await Task.Yield();
            Database.UpdateGuildGold(request.GuildId, request.Amount);
            return new VoidResp();
        }

        public override async Task<ReadStorageItemsResp> ReadStorageItems(ReadStorageItemsReq request, ServerCallContext context)
        {
            await Task.Yield();
            ReadStorageItemsResp resp = new ReadStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            List<CharacterItem> storageItems;
            if (cachedStorageItems.ContainsKey(storageId))
            {
                // Already cached data, so get data from cache
                storageItems = cachedStorageItems[storageId];
            }
            else
            {
                // Doesn't cached yet, so get data from database
                storageItems = Database.ReadStorageItems(storageId.storageType, storageId.storageOwnerId);
                // Cache data, it will be used to validate later
                if (storageItems != null)
                    cachedStorageItems[storageId] = storageItems;
            }
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItems, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<MoveItemToStorageResp> MoveItemToStorage(MoveItemToStorageReq request, ServerCallContext context)
        {
            await Task.Yield();
            MoveItemToStorageResp resp = new MoveItemToStorageResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            Storage storage;
            List<CharacterItem> storageItemList;
            if (!GetStorage(storageId, request.MapName, out storage) ||
                !cachedStorageItems.TryGetValue(storageId, out storageItemList))
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
            if (request.InventoryItemIndex <= 0 || 
                request.InventoryItemIndex > character.NonEquipItems.Count)
            {
                // Invalid inventory index
                resp.Error = EStorageError.StorageErrorInvalidInventoryIndex;
                return resp;
            }
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            // Prepare character and item data
            CharacterItem movingItem = character.NonEquipItems[request.InventoryItemIndex].Clone();
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
            Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(character.NonEquipItems, resp.InventoryItemItems);
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<MoveItemFromStorageResp> MoveItemFromStorage(MoveItemFromStorageReq request, ServerCallContext context)
        {
            await Task.Yield();
            MoveItemFromStorageResp resp = new MoveItemFromStorageResp();
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            Storage storage;
            List<CharacterItem> storageItemList;
            if (!GetStorage(storageId, request.MapName, out storage) ||
                !cachedStorageItems.TryGetValue(storageId, out storageItemList))
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
            if (request.StorageItemIndex <= 0 ||
                request.StorageItemIndex > storageItemList.Count)
            {
                // Invalid storage index
                resp.Error = EStorageError.StorageErrorInvalidStorageIndex;
                return resp;
            }
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem movingItem = storageItemList[request.StorageItemIndex].Clone();
            movingItem.id = GenericUtils.GetUniqueId();
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
            character.FillEmptySlots();
            // Update storage list
            // TODO: May update later to reduce amount of processes
            Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(character.NonEquipItems, resp.InventoryItemItems);
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<SwapOrMergeStorageItemResp> SwapOrMergeStorageItem(SwapOrMergeStorageItemReq request, ServerCallContext context)
        {
            await Task.Yield();
            SwapOrMergeStorageItemResp resp = new SwapOrMergeStorageItemResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            Storage storage;
            List<CharacterItem> storageItemList;
            if (!GetStorage(storageId, request.MapName, out storage) ||
                !cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
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
            Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<IncreaseStorageItemsResp> IncreaseStorageItems(IncreaseStorageItemsReq request, ServerCallContext context)
        {
            await Task.Yield();
            IncreaseStorageItemsResp resp = new IncreaseStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            Storage storage;
            List<CharacterItem> storageItemList;
            if (!GetStorage(storageId, request.MapName, out storage) ||
                !cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
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
            Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
            resp.Error = EStorageError.StorageErrorNone;
            DatabaseServiceUtils.CopyToRepeatedByteString(storageItemList, resp.StorageCharacterItems);
            return resp;
        }

        public override async Task<DecreaseStorageItemsResp> DecreaseStorageItems(DecreaseStorageItemsReq request, ServerCallContext context)
        {
            await Task.Yield();
            DecreaseStorageItemsResp resp = new DecreaseStorageItemsResp();
            // Prepare storage data
            StorageId storageId = new StorageId((StorageType)request.StorageType, request.StorageOwnerId);
            Storage storage;
            List<CharacterItem> storageItemList;
            if (!GetStorage(storageId, request.MapName, out storage) ||
                !cachedStorageItems.TryGetValue(storageId, out storageItemList))
            {
                // Cannot find storage
                resp.Error = EStorageError.StorageErrorInvalidStorage;
                return resp;
            }
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
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
            Database.UpdateStorageItems((StorageType)request.StorageType, request.StorageOwnerId, storageItemList);
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

        public override async Task<CustomResp> Custom(CustomReq request, ServerCallContext context)
        {
            return await onCustomRequest.Invoke(request.Type, request.Data);
        }

        public bool GetStorage(StorageId storageId, string mapName, out Storage storage)
        {
            storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    // Get storage setting from game instance
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    // Get storage setting from game instance
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    // Get building from cache, then get building entity from game instance
                    // And get storage setting from building entity
                    BuildingSaveData building;
                    BuildingEntity buildingEntity;
                    if (cachedBuilding.ContainsKey(mapName) &&
                        cachedBuilding[mapName].TryGetValue(storageId.storageOwnerId, out building) &&
                        GameInstance.BuildingEntities.TryGetValue(building.EntityId, out buildingEntity) &&
                        buildingEntity is StorageEntity)
                        storage = (buildingEntity as StorageEntity).storage;
                    else
                        return false;
                    break;
            }
            return true;
        }
    }
}
