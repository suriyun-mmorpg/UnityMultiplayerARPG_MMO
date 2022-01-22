using Cysharp.Threading.Tasks;
using LiteNetLibManager;
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

                AsyncResponseData<ReadStorageItemsResp> resp;
                do
                {
                    resp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
                    {
                        StorageType = storageId.storageType,
                        StorageOwnerId = storageId.storageOwnerId,
                    });
                } while (resp.IsSuccess);
                ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageCharacterItems);
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
                AsyncResponseData<PartyResp> resp;
                do
                {
                    resp = await DbServiceClient.ReadPartyAsync(new ReadPartyReq()
                    {
                        PartyId = id,
                    });
                } while (resp.IsSuccess);
                ServerPartyHandlers.SetParty(id, resp.Response.PartyData);
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
                AsyncResponseData<GuildResp> resp;
                do
                {
                    resp = await DbServiceClient.ReadGuildAsync(new ReadGuildReq()
                    {
                        GuildId = id,
                    });
                } while (resp.IsSuccess);
                ServerGuildHandlers.SetGuild(id, resp.Response.GuildData);
                loadingGuildIds.Remove(id);
            }
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask SaveCharacter(BasePlayerCharacterEntity playerCharacterEntity, 
            bool changeMap = false, string mapName = "", 
            Vector3 position = new Vector3(), bool overrideRotation = false, Vector3 rotation = new Vector3())
        {
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
                        id = i + "_" + j,
                        type = tempBuff.type,
                        dataId = tempBuff.dataId,
                        level = tempBuff.level,
                        buffRemainsDuration = tempBuff.buffRemainsDuration,
                    });
                }
            }
            await SaveCharacterRoutine(savingCharacterData, summonBuffs);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTask SaveCharacterRoutine(PlayerCharacterData playerCharacterData, List<CharacterBuff> summonBuffs)
        {
            if (playerCharacterData != null && !savingCharacters.Contains(playerCharacterData.Id))
            {
                savingCharacters.Add(playerCharacterData.Id);
                // Update character
                await DbServiceClient.UpdateCharacterAsync(new UpdateCharacterReq()
                {
                    CharacterData = playerCharacterData,
                });
                await DbServiceClient.SetSummonBuffsAsync(new SetSummonBuffsReq()
                {
                    CharacterId = playerCharacterData.Id,
                    SummonBuffs = summonBuffs,
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
                foreach (BasePlayerCharacterEntity playerCharacterEntity in ServerUserHandlers.GetPlayerCharacters())
                {
                    if (playerCharacterEntity == null) continue;
                    tasks.Add(SaveCharacter(playerCharacterEntity));
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
                    BuildingData = buildingSaveData,
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
                foreach (BuildingEntity buildingEntity in ServerBuildingHandlers.GetBuildings())
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
                    BuildingData = saveData,
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
