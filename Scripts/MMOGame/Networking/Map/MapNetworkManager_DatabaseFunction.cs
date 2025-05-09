﻿using Cysharp.Text;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="storageId"></param>
        /// <returns></returns>
        internal async UniTask LoadStorageRoutine(StorageId storageId)
        {
            if (_loadingStorageIds.Contains(storageId))
            {
                do { await UniTask.Delay(100); } while (_loadingStorageIds.Contains(storageId));
                return;
            }
            _loadingStorageIds.Add(storageId);
            DatabaseApiResult<GetStorageItemsResp> resp;
            do
            {
                resp = await DatabaseClient.GetStorageItemsAsync(new GetStorageItemsReq()
                {
                    StorageType = storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId,
                });
            } while (!resp.IsSuccess);
            ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageItems);
            _loadingStorageIds.TryRemove(storageId);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async UniTask LoadPartyRoutine(int id)
        {
            if (id <= 0)
                return;
            if (_loadingPartyIds.Contains(id))
            {
                do { await UniTask.Delay(100); } while (_loadingPartyIds.Contains(id));
                return;
            }
            _loadingPartyIds.Add(id);
            DatabaseApiResult<PartyResp> resp;
            do
            {
                resp = await DatabaseClient.GetPartyAsync(new GetPartyReq()
                {
                    PartyId = id,
                });
            } while (!resp.IsSuccess);
            ServerPartyHandlers.SetParty(id, resp.Response.PartyData);
            _loadingPartyIds.TryRemove(id);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        /// <summary>
        /// Load data repeatedly until it loaded
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal async UniTask LoadGuildRoutine(int id)
        {
            if (id <= 0)
                return;
            if (_loadingGuildIds.Contains(id))
            {
                do { await UniTask.Delay(100); } while (_loadingGuildIds.Contains(id));
                return;
            }
            _loadingGuildIds.Add(id);
            DatabaseApiResult<GuildResp> resp;
            do
            {
                resp = await DatabaseClient.GetGuildAsync(new GetGuildReq()
                {
                    GuildId = id,
                });
            } while (!resp.IsSuccess);
            ServerGuildHandlers.SetGuild(id, resp.Response.GuildData);
            _loadingGuildIds.TryRemove(id);
        }
#endif


#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> SaveStorage(StorageId storageId, List<CharacterItem> storageItems, bool deleteStorageReservation)
        {
            if (savingStorageIds.Contains(storageId))
                return false;
            savingStorageIds.Add(storageId);
            // Update building
            var updateResult = await DatabaseClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = storageId.storageType,
                StorageOwnerId = storageId.storageOwnerId,
                StorageItems = storageItems,
                DeleteStorageReservation = deleteStorageReservation,
            });
            // Update done, clear pending status data
            savingStorageIds.TryRemove(storageId);
            if (LogDebug)
                Logging.Log(LogTag, $"Storage [{storageId}] Saved, Success? {updateResult.IsSuccess}");
            return updateResult.IsSuccess;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveStorage(StorageId storageId, List<CharacterItem> storageItems, bool deleteStorageReservation, CancellationToken cancellationToken, byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveStorage(storageId, storageItems, deleteStorageReservation))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs, cancellationToken: cancellationToken);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveStorage(StorageId storageId, List<CharacterItem> storageItems, bool deleteStorageReservation, byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveStorage(storageId, storageItems, deleteStorageReservation))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> SaveCharacter(TransactionUpdateCharacterState state, PlayerCharacterData savingCharacterData,
            bool changeMap = false, string mapName = "",
            Vector3 position = default, bool overrideRotation = false, Vector3 rotation = default)
        {
            if (savingCharacters.Contains(savingCharacterData.Id))
                return false;
            // Prepare player character data
            if (changeMap)
            {
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                if (overrideRotation)
                    savingCharacterData.CurrentRotation = rotation;
            }
            // Prepare summon buffs
            List<CharacterBuff> summonBuffs = new List<CharacterBuff>();
            if (state.Has(TransactionUpdateCharacterState.Summons))
            {
                CharacterSummon tempSummon;
                CharacterBuff tempBuff;
                for (int i = 0; i < savingCharacterData.Summons.Count; ++i)
                {
                    tempSummon = savingCharacterData.Summons[i];
                    if (tempSummon.CacheEntity == null || tempSummon.CacheEntity.Buffs == null || tempSummon.CacheEntity.Buffs.Count == 0) continue;
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
            }
            savingCharacters.Add(savingCharacterData.Id);
            // Update character
            var updateResult = await DatabaseClient.UpdateCharacterAsync(new UpdateCharacterReq()
            {
                State = state,
                CharacterData = savingCharacterData,
                SummonBuffs = summonBuffs,
                DeleteStorageReservation = cancellingReserveStorageCharacterIds.Contains(savingCharacterData.Id),
            });
            cancellingReserveStorageCharacterIds.TryRemove(savingCharacterData.Id);
            savingCharacters.TryRemove(savingCharacterData.Id);
            if (LogDebug)
                Logging.Log(LogTag, $"Character [{savingCharacterData.Id}] Saved, Success? {updateResult.IsSuccess}");
            return updateResult.IsSuccess;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveCharacter(TransactionUpdateCharacterState state, PlayerCharacterData savingCharacterData, CancellationToken cancellationToken,
            bool changeMap = false, string mapName = "",
            Vector3 position = default, bool overrideRotation = false, Vector3 rotation = default,
            byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveCharacter(state, savingCharacterData, changeMap, mapName, position, overrideRotation, rotation))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs, cancellationToken: cancellationToken);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveCharacter(TransactionUpdateCharacterState state, PlayerCharacterData savingCharacterData,
            bool changeMap = false, string mapName = "",
            Vector3 position = default, bool overrideRotation = false, Vector3 rotation = default,
            byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveCharacter(state, savingCharacterData, changeMap, mapName, position, overrideRotation, rotation))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> SaveBuilding(TransactionUpdateBuildingState state, BuildingSaveData savingBuildingData)
        {
            if (savingBuildings.Contains(savingBuildingData.Id))
                return false;
            savingBuildings.Add(savingBuildingData.Id);
            // Update building
            var updateResult = await DatabaseClient.UpdateBuildingAsync(new UpdateBuildingReq()
            {
                State = state,
                ChannelId = ChannelId,
                MapName = CurrentMapInfo.Id,
                BuildingData = savingBuildingData,
            });
            // Update done, clear pending status data
            savingBuildings.TryRemove(savingBuildingData.Id);
            if (LogDebug)
                Logging.Log(LogTag, $"Building [{savingBuildingData.Id}] Saved, Success? {updateResult.IsSuccess}");
            return updateResult.IsSuccess;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveBuilding(TransactionUpdateBuildingState state, BuildingSaveData savingBuildingData, CancellationToken cancellationToken, byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveBuilding(state, savingBuildingData))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs, cancellationToken: cancellationToken);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal async UniTask<bool> WaitAndSaveBuilding(TransactionUpdateBuildingState state, BuildingSaveData savingBuildingData, byte retireAttempts = 30, int retireDelayMs = 100)
        {
            int count = 0;
            while (!await SaveBuilding(state, savingBuildingData))
            {
                count++;
                if (count > retireAttempts)
                    return false;
                await UniTask.Delay(retireDelayMs);
            }
            return true;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override async UniTask<BuildingEntity> CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            if (!initialize)
            {
                await DatabaseClient.CreateBuildingAsync(new CreateBuildingReq()
                {
                    ChannelId = ChannelId,
                    MapName = CurrentMapInfo.Id,
                    BuildingData = saveData,
                });
            }
            BuildingEntity entity = await base.CreateBuildingEntity(saveData, initialize);
            // Add updater if it is not existed
            if (!entity.TryGetComponent<BuildingDataUpdater>(out _))
                entity.gameObject.AddComponent<BuildingDataUpdater>();
            return entity;
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override async UniTask DestroyBuildingEntity(string id, bool isSceneObject)
        {
            if (!isSceneObject)
                await DestroyBuildingEntityRoutine(id);
            await base.DestroyBuildingEntity(id, isSceneObject);
        }

        internal async UniTask DestroyBuildingEntityRoutine(string id)
        {
            await DatabaseClient.DeleteBuildingAsync(new DeleteBuildingReq()
            {
                ChannelId = ChannelId,
                MapName = CurrentMapInfo.Id,
                BuildingId = id
            });
        }
#endif
    }
}
