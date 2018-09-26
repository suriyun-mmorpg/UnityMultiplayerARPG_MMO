using LiteNetLib;
using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        protected readonly HashSet<int> loadingPartyIds = new HashSet<int>();
        protected readonly HashSet<int> loadingGuildIds = new HashSet<int>();
        protected readonly HashSet<string> savingCharacters = new HashSet<string>();
        protected readonly HashSet<string> savingBuildings = new HashSet<string>();
        
        private IEnumerator LoadPartyRoutine(int id)
        {
            if (id > 0 && !loadingPartyIds.Contains(id))
            {
                loadingPartyIds.Add(id);
                var job = new LoadPartyJob(Database, id);
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
                var job = new LoadGuildJob(Database, id);
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
                var job = new UpdateCharacterJob(Database, playerCharacterData);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                savingCharacters.Remove(playerCharacterData.Id);
                Debug.Log("Character [" + playerCharacterData.Id + "] Saved");
            }
        }

        private IEnumerator SaveCharactersRoutine()
        {
            if (savingCharacters.Count == 0)
            {
                var i = 0;
                foreach (var playerCharacter in playerCharacters.Values)
                {
                    StartCoroutine(SaveCharacterRoutine(playerCharacter.CloneTo(new PlayerCharacterData())));
                    ++i;
                }
                while (savingCharacters.Count > 0)
                {
                    yield return 0;
                }
                Debug.Log("Saved " + i + " character(s)");
            }
        }

        private IEnumerator SaveBuildingRoutine(IBuildingSaveData saveData)
        {
            if (saveData != null && !savingBuildings.Contains(saveData.Id))
            {
                savingBuildings.Add(saveData.Id);
                var job = new UpdateBuildingJob(Database, Assets.onlineScene.SceneName, saveData);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                savingBuildings.Remove(saveData.Id);
                Debug.Log("Building [" + saveData.Id + "] Saved");
            }
        }

        private IEnumerator SaveBuildingsRoutine()
        {
            if (savingBuildings.Count == 0)
            {
                var i = 0;
                foreach (var buildingEntity in buildingEntities.Values)
                {
                    StartCoroutine(SaveBuildingRoutine(buildingEntity));
                    ++i;
                }
                while (savingBuildings.Count > 0)
                {
                    yield return 0;
                }
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
