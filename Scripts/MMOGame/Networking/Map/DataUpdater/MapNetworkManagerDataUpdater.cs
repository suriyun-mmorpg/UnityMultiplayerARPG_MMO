using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MapNetworkManagerDataUpdater : MonoBehaviour
    {
        private ConcurrentDictionary<string, BuildingUpdateData> _buildingUpdateDataDict = new ConcurrentDictionary<string, BuildingUpdateData>();
        private ConcurrentDictionary<string, PlayerCharacterUpdateData> _playerCharacterUpdateDataDict = new ConcurrentDictionary<string, PlayerCharacterUpdateData>();
        public MapNetworkManager Manager { get; set; }
        private bool _updating = false;

        public void Clean()
        {
            _buildingUpdateDataDict.Clear();
            _playerCharacterUpdateDataDict.Clear();
            _updating = false;
        }

        public void EnqueueBuildingSave(TransactionUpdateBuildingState appendState, IBuildingSaveData buildingSaveData)
        {
            if (buildingSaveData == null)
                return;
            string id = buildingSaveData.Id;
            if (!_buildingUpdateDataDict.TryRemove(id, out BuildingUpdateData updateData))
            {
                updateData = new BuildingUpdateData();
            }
            updateData.Update(appendState, buildingSaveData);
            _buildingUpdateDataDict[id] = updateData;
        }

        public void EnqueuePlayerCharacterSave(TransactionUpdateCharacterState appendState, IPlayerCharacterData playerCharacterData)
        {
            if (playerCharacterData == null)
                return;
            string id = playerCharacterData.Id;
            if (!_playerCharacterUpdateDataDict.TryRemove(id, out PlayerCharacterUpdateData updateData))
            {
                updateData = new PlayerCharacterUpdateData();
            }
            updateData.Update(appendState, playerCharacterData);
            _playerCharacterUpdateDataDict[id] = updateData;
        }

        public async void ProceedSaving()
        {
            if (_updating)
                return;
            _updating = true;
            List<UniTask<bool>> tasks = new List<UniTask<bool>>();
            List<string> playerCharacterIds = new List<string>(_playerCharacterUpdateDataDict.Keys);
            foreach (string id in playerCharacterIds)
            {
                tasks.Add(_playerCharacterUpdateDataDict[id].ProceedSave(this));
            }

            List<string> buildingIds = new List<string>(_buildingUpdateDataDict.Keys);
            foreach (string id in buildingIds)
            {
                tasks.Add(_buildingUpdateDataDict[id].ProceedSave(this));
            }
            await UniTask.WhenAll(tasks);
            _updating = false;
        }

        internal void PlayerCharacterDataSaved(string id)
        {
            _playerCharacterUpdateDataDict.TryRemove(id, out _);
        }

        internal void BuildingDataSaved(string id)
        {
            _buildingUpdateDataDict.TryRemove(id, out _);
        }
    }
}
