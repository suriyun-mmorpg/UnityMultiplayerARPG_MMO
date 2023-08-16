using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns></returns>
        private async UniTask LoadStorageRoutine(StorageId storageId)
        {
            if (_loadingStorageIds.Contains(storageId))
            {
                do
                {
                    await UniTask.Yield();
                }
                while (_loadingStorageIds.Contains(storageId));
                return;
            }
            _loadingStorageIds.Add(storageId);
            DatabaseApiResult<ReadStorageItemsResp> resp;
            do
            {
                resp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
                {
                    StorageType = storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId,
                });
            } while (!resp.IsSuccess);
            ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageItems);
            _loadingStorageIds.Remove(storageId);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async UniTask LoadPartyRoutine(int id)
        {
            if (id <= 0)
                return;
            if (_loadingPartyIds.Contains(id))
            {
                do
                {
                    await UniTask.Yield();
                }
                while (_loadingPartyIds.Contains(id));
                return;
            }
            _loadingPartyIds.Add(id);
            DatabaseApiResult<PartyResp> resp;
            do
            {
                resp = await DbServiceClient.ReadPartyAsync(new ReadPartyReq()
                {
                    PartyId = id,
                });
            } while (!resp.IsSuccess);
            ServerPartyHandlers.SetParty(id, resp.Response.PartyData);
            _loadingPartyIds.Remove(id);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async UniTask LoadGuildRoutine(int id)
        {
            if (id <= 0)
                return;
            if (_loadingGuildIds.Contains(id))
            {
                do
                {
                    await UniTask.Yield();
                }
                while (_loadingGuildIds.Contains(id));
                return;
            }
            _loadingGuildIds.Add(id);
            DatabaseApiResult<GuildResp> resp;
            do
            {
                resp = await DbServiceClient.ReadGuildAsync(new ReadGuildReq()
                {
                    GuildId = id,
                });
            } while (!resp.IsSuccess);
            ServerGuildHandlers.SetGuild(id, resp.Response.GuildData);
            _loadingGuildIds.Remove(id);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTask<bool> SaveCharacter(BasePlayerCharacterEntity playerCharacterEntity,
            bool changeMap = false, string mapName = "",
            Vector3 position = default, bool overrideRotation = false, Vector3 rotation = default)
        {
            if (savingCharacters.Contains(playerCharacterEntity.Id))
                return false;
            // Prepare player character data
            PlayerCharacterData savingCharacterData = playerCharacterEntity.CloneTo(new PlayerCharacterData());
            if (changeMap)
            {
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                if (overrideRotation)
                    savingCharacterData.CurrentRotation = rotation;
            }
            // Prepare storage items
            StorageId storageId = new StorageId(StorageType.Player, savingCharacterData.UserId);
            List<CharacterItem> storageItems = new List<CharacterItem>();
            if (ServerStorageHandlers.IsStorageSavePending(storageId))
                storageItems.AddRange(ServerStorageHandlers.GetStorageItems(storageId));
            // Prepare summon buffs
            List<CharacterBuff> summonBuffs = new List<CharacterBuff>();
            CharacterSummon tempSummon;
            CharacterBuff tempBuff;
            for (int i = 0; i < playerCharacterEntity.Summons.Count; ++i)
            {
                tempSummon = playerCharacterEntity.Summons[i];
                if (tempSummon == null || tempSummon.CacheEntity == null || tempSummon.CacheEntity.Buffs == null || tempSummon.CacheEntity.Buffs.Count == 0) continue;
                for (int j = 0; j < tempSummon.CacheEntity.Buffs.Count; ++j)
                {
                    tempBuff = tempSummon.CacheEntity.Buffs[j];
                    summonBuffs.Add(new CharacterBuff()
                    {
                        id = ZString.Concat(savingCharacterData.Id, '_', i, '_', j),
                        type = tempBuff.type,
                        dataId = tempBuff.dataId,
                        level = tempBuff.level,
                        buffRemainsDuration = tempBuff.buffRemainsDuration,
                    });
                }
            }
            savingCharacters.Add(savingCharacterData.Id);
            // Update character
            await DbServiceClient.UpdateCharacterAsync(new UpdateCharacterReq()
            {
                CharacterData = savingCharacterData,
                ExtraContent = (storageItems.Count > 0 ? UpdateCharacterReq.EExtraContent.StorageItems : UpdateCharacterReq.EExtraContent.None) | (summonBuffs.Count > 0 ? UpdateCharacterReq.EExtraContent.SummonBuffs : UpdateCharacterReq.EExtraContent.None),
                StorageItems = storageItems,
                SummonBuffs = summonBuffs,
            });
            // Update done, clear pending status data
            storageItems.Clear();
            summonBuffs.Clear();
            ServerStorageHandlers.SetStorageSavePending(storageId, false);
            savingCharacters.Remove(savingCharacterData.Id);
            if (LogDebug)
                Logging.Log(LogTag, "Character [" + savingCharacterData.Id + "] Saved");
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTaskVoid SaveAllCharacters()
        {
            if (savingCharacters.Count > 0)
                return;
            int i = 0;
            List<UniTask> tasks = new List<UniTask>();
            foreach (BasePlayerCharacterEntity playerCharacterEntity in ServerUserHandlers.GetPlayerCharacters())
            {
                if (playerCharacterEntity == null) continue;
                tasks.Add(SaveCharacter(playerCharacterEntity));
                ++i;
            }
            await UniTask.WhenAll(tasks);
            if (LogDebug)
                Logging.Log(LogTag, "Saved " + i + " character(s)");
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTask<bool> SaveBuilding(BuildingEntity buildingEntity)
        {
            if (savingBuildings.Contains(buildingEntity.Id))
                return false;
            // Prepare building data
            BuildingSaveData savingBuildingData = buildingEntity.CloneTo(new BuildingSaveData());
            // Prepare storage items
            StorageId storageId = new StorageId(StorageType.Building, savingBuildingData.Id);
            List<CharacterItem> storageItems = new List<CharacterItem>();
            if (ServerStorageHandlers.IsStorageSavePending(storageId))
                storageItems.AddRange(ServerStorageHandlers.GetStorageItems(storageId));
            savingBuildings.Add(savingBuildingData.Id);
            // Update building
            await DbServiceClient.UpdateBuildingAsync(new UpdateBuildingReq()
            {
                ChannelId = ChannelId,
                MapName = CurrentMapInfo.Id,
                BuildingData = savingBuildingData,
                ExtraContent = storageItems.Count > 0 ? UpdateBuildingReq.EExtraContent.StorageItems : UpdateBuildingReq.EExtraContent.None,
                StorageItems = storageItems,
            });
            // Update done, clear pending status data
            storageItems.Clear();
            ServerStorageHandlers.SetStorageSavePending(storageId, false);
            savingBuildings.Remove(savingBuildingData.Id);
            if (LogDebug)
                Logging.Log(LogTag, "Building [" + savingBuildingData.Id + "] Saved");
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTaskVoid SaveAllBuildings()
        {
            if (savingBuildings.Count == 0)
                return;
            int i = 0;
            List<UniTask> tasks = new List<UniTask>();
            foreach (BuildingEntity buildingEntity in ServerBuildingHandlers.GetBuildings())
            {
                if (buildingEntity == null) continue;
                tasks.Add(SaveBuilding(buildingEntity));
                ++i;
            }
            await UniTask.WhenAll(tasks);
            if (LogDebug)
                Logging.Log(LogTag, "Saved " + i + " building(s)");
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
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
                    ChannelId = ChannelId,
                    MapName = CurrentMapInfo.Id,
                    BuildingData = saveData,
                });
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            DestroyBuildingEntityRoutine(id).Forget();
        }

        private async UniTask DestroyBuildingEntityRoutine(string id)
        {
            await DbServiceClient.DeleteBuildingAsync(new DeleteBuildingReq()
            {
                ChannelId = ChannelId,
                MapName = CurrentMapInfo.Id,
                BuildingId = id
            });
        }
#endif
    }
}
