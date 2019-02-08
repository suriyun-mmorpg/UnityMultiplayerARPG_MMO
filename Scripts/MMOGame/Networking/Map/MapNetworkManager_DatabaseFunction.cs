using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        private IEnumerator LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                ReadPartyJob job = new ReadPartyJob(Database, id);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                if (job.result != null)
                    parties[id] = job.result;
                else
                    parties.Remove(id);
                loadingPartyIds.Remove(id);
            }
        }

        private IEnumerator LoadGuildRoutine(int id)
        {
            if (id > 0 && !loadingGuildIds.Contains(id))
            {
                loadingGuildIds.Add(id);
                ReadGuildJob job = new ReadGuildJob(Database, id, gameInstance.SocialSystemSetting.GuildMemberRoles);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                if (job.result != null)
                    guilds[id] = job.result;
                else
                    guilds.Remove(id);
                loadingGuildIds.Remove(id);
            }
        }

        private IEnumerator SaveCharacterRoutine(IPlayerCharacterData playerCharacterData)
        {
            if (playerCharacterData != null && !savingCharacters.Contains(playerCharacterData.Id))
            {
                savingCharacters.Add(playerCharacterData.Id);
                UpdateCharacterJob job = new UpdateCharacterJob(Database, playerCharacterData);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                savingCharacters.Remove(playerCharacterData.Id);
                if (LogInfo)
                    Debug.Log("Character [" + playerCharacterData.Id + "] Saved");
            }
        }

        private IEnumerator SaveCharactersRoutine()
        {
            if (savingCharacters.Count == 0)
            {
                int i = 0;
                foreach (BasePlayerCharacterEntity playerCharacter in playerCharacters.Values)
                {
                    StartCoroutine(SaveCharacterRoutine(playerCharacter.CloneTo(new PlayerCharacterData())));
                    ++i;
                }
                while (savingCharacters.Count > 0)
                {
                    yield return 0;
                }
                if (LogInfo)
                    Debug.Log("Saved " + i + " character(s)");
            }
        }

        private IEnumerator SaveBuildingRoutine(IBuildingSaveData buildingSaveData)
        {
            if (buildingSaveData != null && !savingBuildings.Contains(buildingSaveData.Id))
            {
                savingBuildings.Add(buildingSaveData.Id);
                UpdateBuildingJob job = new UpdateBuildingJob(Database, Assets.onlineScene.SceneName, buildingSaveData);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                savingBuildings.Remove(buildingSaveData.Id);
                if (LogInfo)
                    Debug.Log("Building [" + buildingSaveData.Id + "] Saved");
            }
        }

        private IEnumerator SaveBuildingsRoutine()
        {
            if (savingBuildings.Count == 0)
            {
                int i = 0;
                foreach (BuildingEntity buildingEntity in buildingEntities.Values)
                {
                    if (buildingEntity == null) continue;
                    StartCoroutine(SaveBuildingRoutine(buildingEntity.CloneTo(new BuildingSaveData())));
                    ++i;
                }
                while (savingBuildings.Count > 0)
                {
                    yield return 0;
                }
                if (LogInfo)
                    Debug.Log("Saved " + i + " building(s)");
            }
        }

        public override void CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            base.CreateBuildingEntity(saveData, initialize);
            if (!initialize)
                new CreateBuildingJob(Database, Assets.onlineScene.SceneName, saveData).Start();
        }

        public override void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            new DeleteBuildingJob(Database, Assets.onlineScene.SceneName, id).Start();
        }
    }
}
