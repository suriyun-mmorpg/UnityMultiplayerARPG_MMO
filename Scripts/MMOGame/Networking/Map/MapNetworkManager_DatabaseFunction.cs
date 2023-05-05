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
        private async UniTask LoadStorageRoutine(StorageId storageId)
        {
            if (!_loadingStorageIds.Contains(storageId))
            {
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
                ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageCharacterItems);
                _loadingStorageIds.Remove(storageId);
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTask LoadPartyRoutine(int id)
        {
            if (id > 0 && !_loadingPartyIds.Contains(id))
            {
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
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTask LoadGuildRoutine(int id)
        {
            if (id > 0 && !_loadingGuildIds.Contains(id))
            {
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
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTask<bool> SaveCharacter(BasePlayerCharacterEntity playerCharacterEntity,
            bool changeMap = false, string mapName = "",
            Vector3 position = new Vector3(), bool overrideRotation = false, Vector3 rotation = new Vector3())
        {
            if (savingCharacters.Contains(playerCharacterEntity.Id))
                return false;
            PlayerCharacterData savingCharacterData = playerCharacterEntity.CloneTo(new PlayerCharacterData());
            if (changeMap)
            {
                savingCharacterData.CurrentMapName = mapName;
                savingCharacterData.CurrentPosition = position;
                if (overrideRotation)
                    savingCharacterData.CurrentRotation = rotation;
            }
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
                        id = ZString.Concat(savingCharacterData.Id, "_", i, "_", j),
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
            });
            await DbServiceClient.SetSummonBuffsAsync(new SetSummonBuffsReq()
            {
                CharacterId = savingCharacterData.Id,
                SummonBuffs = summonBuffs,
            });
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
        private async UniTask<bool> SaveBuilding(BuildingSaveData buildingSaveData)
        {
            if (savingBuildings.Contains(buildingSaveData.Id))
                return false;
            savingBuildings.Add(buildingSaveData.Id);
            // Update building
            await DbServiceClient.UpdateBuildingAsync(new UpdateBuildingReq()
            {
                MapName = CurrentMapInfo.Id,
                BuildingData = buildingSaveData,
            });
            savingBuildings.Remove(buildingSaveData.Id);
            if (LogDebug)
                Logging.Log(LogTag, "Building [" + buildingSaveData.Id + "] Saved");
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
                tasks.Add(SaveBuilding(buildingEntity.CloneTo(new BuildingSaveData())));
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
                MapName = CurrentMapInfo.Id,
                BuildingId = id
            });
        }
#endif
    }
}
