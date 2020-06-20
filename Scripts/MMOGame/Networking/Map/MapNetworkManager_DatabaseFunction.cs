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
        private async Task LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                PartyResp resp = await DbServiceClient.ReadPartyAsync(new ReadPartyReq()
                {
                    PartyId = id
                });
                parties[id] = resp.PartyData.FromByteString<PartyData>();
                loadingPartyIds.Remove(id);
            }
        }

        private async Task LoadGuildRoutine(int id)
        {
            if (id > 0 && !loadingGuildIds.Contains(id))
            {
                loadingGuildIds.Add(id);
                GuildResp resp = await DbServiceClient.ReadGuildAsync(new ReadGuildReq()
                {
                    GuildId = id
                });
                guilds[id] = resp.GuildData.FromByteString<GuildData>();
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
                    CharacterData = playerCharacterData.ToByteString()
                });
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
                    BuildingData = buildingSaveData.ToByteString()
                });
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
                    BuildingData = saveData.ToByteString()
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
