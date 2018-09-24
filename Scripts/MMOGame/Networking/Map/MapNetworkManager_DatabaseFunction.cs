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
        private readonly Dictionary<int, Task> loadingPartyIds = new Dictionary<int, Task>();
        private readonly Dictionary<int, Task> loadingGuildIds = new Dictionary<int, Task>();
        private readonly Dictionary<string, Task> savingCharacters = new Dictionary<string, Task>();

        #region Load Functions
        private async Task LoadPartyDataFromDatabase(int partyId)
        {
            // If there are other party loading which is not completed, it will not load again
            if (partyId <= 0 || loadingPartyIds.ContainsKey(partyId))
                return;
            var task = Database.ReadParty(partyId);
            loadingPartyIds.Add(partyId, task);
            var party = await task;
            if (party != null)
                parties[partyId] = party;
            else
                parties.Remove(partyId);
            loadingPartyIds.Remove(partyId);
        }

        private async Task LoadGuildDataFromDatabase(int guildId)
        {
            // If there are other party loading which is not completed, it will not load again
            if (guildId <= 0 || loadingGuildIds.ContainsKey(guildId))
                return;
            var task = Database.ReadGuild(guildId);
            loadingGuildIds.Add(guildId, task);
            var guild = await task;
            if (guild != null)
                guilds[guildId] = guild;
            else
                guilds.Remove(guildId);
            loadingGuildIds.Remove(guildId);
        }
        #endregion

        #region Save functions
        private async Task SaveCharacter(IPlayerCharacterData playerCharacterData)
        {
            if (playerCharacterData == null)
                return;
            Task task;
            if (savingCharacters.TryGetValue(playerCharacterData.Id, out task) && !task.IsCompleted)
                await task;
            task = Database.UpdateCharacter(playerCharacterData);
            savingCharacters[playerCharacterData.Id] = task;
            Debug.Log("Character [" + playerCharacterData.Id + "] Saved");
            await task;
        }

        private async Task SaveCharacters()
        {
            if (saveCharactersTask != null && !saveCharactersTask.IsCompleted)
                await saveCharactersTask;
            var tasks = new List<Task>();
            foreach (var playerCharacter in playerCharacters.Values)
            {
                tasks.Add(SaveCharacter(playerCharacter));
            }
            await Task.WhenAll(tasks);
            Debug.Log("Characters Saved " + tasks.Count + " character(s)");
        }

        private async Task SaveWorld()
        {
            if (saveWorldTask != null && !saveWorldTask.IsCompleted)
                await saveWorldTask;
            var tasks = new List<Task>();
            foreach (var buildingEntity in buildingEntities.Values)
            {
                tasks.Add(Database.UpdateBuilding(Assets.onlineScene.SceneName, buildingEntity));
            }
            await Task.WhenAll(tasks);
            Debug.Log("World Saved " + tasks.Count + " building(s)");
        }

        public override async void CreateBuildingEntity(BuildingSaveData saveData, bool initialize)
        {
            base.CreateBuildingEntity(saveData, initialize);
            if (!initialize)
                await Database.CreateBuilding(Assets.onlineScene.SceneName, saveData);
        }

        public override async void DestroyBuildingEntity(string id)
        {
            base.DestroyBuildingEntity(id);
            await Database.DeleteBuilding(Assets.onlineScene.SceneName, id);
        }
        #endregion
    }
}
