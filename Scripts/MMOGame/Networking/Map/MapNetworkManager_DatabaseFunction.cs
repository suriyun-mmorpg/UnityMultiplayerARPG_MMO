using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        private async Task LoadStorageRoutine(StorageId storageId)
        {
            if (!loadingStorageIds.Contains(storageId))
            {
                loadingStorageIds.Add(storageId);
                ReadStorageItemsResp readStorageItemsResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
                {
                    StorageType = (EStorageType)storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId
                });

                storageItems[storageId] = readStorageItemsResp.StorageCharacterItems.MakeListFromRepeatedBytes<CharacterItem>();
                loadingStorageIds.Remove(storageId);
            }
        }

        private async Task LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                ReadPartyResp resp = await DbServiceClient.ReadPartyAsync(new ReadPartyReq()
                {
                    PartyId = id
                });
                parties[id] = resp.PartyData.FromBytes<PartyData>();
                loadingPartyIds.Remove(id);
            }
        }

        private async Task LoadGuildRoutine(int id)
        {
            if (id > 0 && !loadingGuildIds.Contains(id))
            {
                loadingGuildIds.Add(id);
                ReadGuildResp resp = await DbServiceClient.ReadGuildAsync(new ReadGuildReq()
                {
                    GuildId = id
                });
                guilds[id] = resp.GuildData.FromBytes<GuildData>();
                loadingGuildIds.Remove(id);
            }
        }

        private async Task SaveCharacterRoutine(PlayerCharacterData playerCharacterData, string userId)
        {
            if (playerCharacterData != null && !savingCharacters.Contains(playerCharacterData.Id))
            {
                savingCharacters.Add(playerCharacterData.Id);
                // Update character
                await DbServiceClient.UpdateCharacterAsync(new UpdateCharacterReq()
                {
                    CharacterData = playerCharacterData.ToBytes()
                });
                // Update storage items
                StorageId storageId = new StorageId(StorageType.Player, userId);
                if (storageItems.ContainsKey(storageId))
                {
                    UpdateStorageItemsReq req = new UpdateStorageItemsReq()
                    {
                        StorageType = (EStorageType)storageId.storageType,
                        StorageOwnerId = storageId.storageOwnerId
                    };
                    DatabaseServiceUtils.CopyToRepeatedBytes(storageItems[storageId], req.StorageCharacterItems);
                    await DbServiceClient.UpdateStorageItemsAsync(req);
                }
                savingCharacters.Remove(playerCharacterData.Id);
                if (LogInfo)
                    Logging.Log(LogTag, "Character [" + playerCharacterData.Id + "] Saved");
            }
        }

        private async void SaveCharactersRoutine()
        {
            if (savingCharacters.Count == 0)
            {
                int i = 0;
                foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
                {
                    await SaveCharacterRoutine(playerCharacter.CloneTo(new PlayerCharacterData()), playerCharacter.UserId);
                    ++i;
                }
                while (savingCharacters.Count > 0)
                {
                    await Task.Yield();
                }
                if (LogInfo)
                    Logging.Log(LogTag, "Saved " + i + " character(s)");
            }
        }

        private async Task SaveBuildingRoutine(BuildingSaveData buildingSaveData)
        {
            if (!savingBuildings.Contains(buildingSaveData.Id))
            {
                savingBuildings.Add(buildingSaveData.Id);
                // Update building
                await DbServiceClient.UpdateBuildingAsync(new UpdateBuildingReq()
                {
                    MapName = Assets.onlineScene.SceneName,
                    BuildingData = buildingSaveData.ToBytes()
                });
                // Update storage items
                StorageId storageId = new StorageId(StorageType.Building, buildingSaveData.Id);
                if (storageItems.ContainsKey(storageId))
                {
                    UpdateStorageItemsReq req = new UpdateStorageItemsReq()
                    {
                        StorageType = (EStorageType)storageId.storageType,
                        StorageOwnerId = storageId.storageOwnerId
                    };
                    DatabaseServiceUtils.CopyToRepeatedBytes(storageItems[storageId], req.StorageCharacterItems);
                    await DbServiceClient.UpdateStorageItemsAsync(req);
                }
                savingBuildings.Remove(buildingSaveData.Id);
                if (LogInfo)
                    Logging.Log(LogTag, "Building [" + buildingSaveData.Id + "] Saved");
            }
        }

        private async void SaveBuildingsRoutine()
        {
            if (savingBuildings.Count == 0)
            {
                int i = 0;
                foreach (BuildingEntity buildingEntity in buildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    await SaveBuildingRoutine(buildingEntity.CloneTo(new BuildingSaveData()));
                    ++i;
                }
                while (savingBuildings.Count > 0)
                {
                    await Task.Yield();
                }
                if (LogInfo)
                    Logging.Log(LogTag, "Saved " + i + " building(s)");
            }
        }

        public override BuildingEntity CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            if (!initialize)
            {
                DbServiceClient.CreateBuildingAsync(new CreateBuildingReq()
                {
                    MapName = Assets.onlineScene.SceneName,
                    BuildingData = saveData.ToBytes()
                });
            }
            return base.CreateBuildingEntity(saveData, initialize);
        }

        public override void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            DbServiceClient.DeleteBuildingAsync(new DeleteBuildingReq()
            {
                MapName = Assets.onlineScene.SceneName,
                BuildingId = id
            });
        }
    }
}
