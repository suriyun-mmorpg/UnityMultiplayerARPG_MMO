using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask LoadStorageRoutine(StorageId storageId)
        {
            if (!loadingStorageIds.Contains(storageId))
            {
                loadingStorageIds.Add(storageId);
                ReadStorageItemsResp readStorageItemsResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
                {
                    StorageType = (EStorageType)storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId
                });
                ServerStorageHandlers.SetStorageItems(storageId, readStorageItemsResp.StorageCharacterItems.MakeListFromRepeatedByteString<CharacterItem>());
                loadingStorageIds.Remove(storageId);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                PartyResp resp = await DbServiceClient.ReadPartyAsync(new ReadPartyReq()
                {
                    PartyId = id
                });
                ServerPartyHandlers.SetParty(id, resp.PartyData.FromByteString<PartyData>());
                loadingPartyIds.Remove(id);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask LoadGuildRoutine(int id)
        {
            if (id > 0 && !loadingGuildIds.Contains(id))
            {
                loadingGuildIds.Add(id);
                GuildResp resp = await DbServiceClient.ReadGuildAsync(new ReadGuildReq()
                {
                    GuildId = id
                });
                ServerGuildHandlers.SetGuild(id, resp.GuildData.FromByteString<GuildData>());
                loadingGuildIds.Remove(id);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask SaveCharacterRoutine(PlayerCharacterData playerCharacterData, string userId)
        {
            if (playerCharacterData != null && !savingCharacters.Contains(playerCharacterData.Id))
            {
                savingCharacters.Add(playerCharacterData.Id);
                // Update character
                await DbServiceClient.UpdateCharacterAsync(new UpdateCharacterReq()
                {
                    CharacterData = playerCharacterData.ToByteString()
                });
                savingCharacters.Remove(playerCharacterData.Id);
                if (LogInfo)
                    Logging.Log(LogTag, "Character [" + playerCharacterData.Id + "] Saved");
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid SaveCharactersRoutine()
        {
            if (savingCharacters.Count == 0)
            {
                int i = 0;
                List<UniTask> tasks = new List<UniTask>();
                foreach (BasePlayerCharacterEntity playerCharacter in ServerUserHandlers.GetPlayerCharacters())
                {
                    if (playerCharacter == null) continue;
                    tasks.Add(SaveCharacterRoutine(playerCharacter.CloneTo(new PlayerCharacterData()), playerCharacter.UserId));
                    ++i;
                }
                await UniTask.WhenAll(tasks);
                if (LogInfo)
                    Logging.Log(LogTag, "Saved " + i + " character(s)");
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask SaveBuildingRoutine(BuildingSaveData buildingSaveData)
        {
            if (!savingBuildings.Contains(buildingSaveData.Id))
            {
                savingBuildings.Add(buildingSaveData.Id);
                // Update building
                await DbServiceClient.UpdateBuildingAsync(new UpdateBuildingReq()
                {
                    MapName = Assets.onlineScene.SceneName,
                    BuildingData = buildingSaveData.ToByteString()
                });
                savingBuildings.Remove(buildingSaveData.Id);
                if (LogInfo)
                    Logging.Log(LogTag, "Building [" + buildingSaveData.Id + "] Saved");
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid SaveBuildingsRoutine()
        {
            if (savingBuildings.Count == 0)
            {
                int i = 0;
                List<UniTask> tasks = new List<UniTask>();
                foreach (BuildingEntity buildingEntity in BuildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    tasks.Add(SaveBuildingRoutine(buildingEntity.CloneTo(new BuildingSaveData())));
                    ++i;
                }
                await UniTask.WhenAll(tasks);
                if (LogInfo)
                    Logging.Log(LogTag, "Saved " + i + " building(s)");
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override BuildingEntity CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            CreateBuildingEntityRoutine(saveData, initialize).Forget();
            return base.CreateBuildingEntity(saveData, initialize);
        }

        private async UniTask CreateBuildingEntityRoutine(BuildingSaveData saveData, bool initialize)
        {
            if (!initialize)
            {
                await DbServiceClient.CreateBuildingAsync(new CreateBuildingReq()
                {
                    MapName = Assets.onlineScene.SceneName,
                    BuildingData = saveData.ToByteString()
                });
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            DestroyBuildingEntityRoutine(id).Forget();
        }

        private async UniTask DestroyBuildingEntityRoutine(string id)
        {
            await DbServiceClient.DeleteBuildingAsync(new DeleteBuildingReq()
            {
                MapName = Assets.onlineScene.SceneName,
                BuildingId = id
            });
        }
#endif
    }
}
