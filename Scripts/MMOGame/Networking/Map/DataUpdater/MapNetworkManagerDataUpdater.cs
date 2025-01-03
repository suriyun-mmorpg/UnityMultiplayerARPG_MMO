using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MapNetworkManagerDataUpdater : MonoBehaviour
    {
        [SerializeField]
        private float buildingSaveInterval = 5f;
        [SerializeField]
        private float playerCharacterSaveInterval = 1f;

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal static readonly HashSet<BuildingDataUpdater> BuildingDataUpdaters = new HashSet<BuildingDataUpdater>();
        internal static readonly HashSet<PlayerCharacterDataUpdater> PlayerCharacterDataUpdaters = new HashSet<PlayerCharacterDataUpdater>();
        private ConcurrentDictionary<StorageId, StorageItemsUpdateData> _storageItemsUpdateDict = new ConcurrentDictionary<StorageId, StorageItemsUpdateData>();
        private ConcurrentDictionary<string, BuildingUpdateData> _buildingUpdateDataDict = new ConcurrentDictionary<string, BuildingUpdateData>();
        private ConcurrentDictionary<string, PlayerCharacterUpdateData> _playerCharacterUpdateDataDict = new ConcurrentDictionary<string, PlayerCharacterUpdateData>();
        public MapNetworkManager Manager { get; internal set; }

        private bool _updating = false;
        private float _lastBuildingUpdateTime;
        private float _lastPlayerCharacterUpdateTime;

        public void Clean()
        {
            _buildingUpdateDataDict.Clear();
            _playerCharacterUpdateDataDict.Clear();
            _updating = false;
        }

        public void EnqueueStorageItemsSave(StorageId storageId, IList<CharacterItem> storageItems)
        {
            if (storageItems == null)
                return;
            if (!_storageItemsUpdateDict.TryRemove(storageId, out StorageItemsUpdateData updateData))
            {
                updateData = new StorageItemsUpdateData();
            }
            updateData.Update(storageId, storageItems);
            _storageItemsUpdateDict[storageId] = updateData;
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

            float currentTime = Time.unscaledTime;
            if (currentTime - _lastBuildingUpdateTime > buildingSaveInterval)
            {
                _lastBuildingUpdateTime = currentTime;
                foreach (var updater in BuildingDataUpdaters)
                {
                    updater.EnqueueBuildingSave(this);
                }
            }

            if (currentTime - _lastPlayerCharacterUpdateTime > playerCharacterSaveInterval)
            {
                _lastPlayerCharacterUpdateTime = currentTime;
                foreach (var updater in PlayerCharacterDataUpdaters)
                {
                    updater.EnqueuePlayerCharacterSave(this);
                }
            }

            List<UniTask<bool>> tasks = new List<UniTask<bool>>();
            foreach (var storageItemsUpdaterData in _storageItemsUpdateDict.Values)
            {
                tasks.Add(storageItemsUpdaterData.ProceedSave(this));
            }

            foreach (var buildingUpdaterData in _buildingUpdateDataDict.Values)
            {
                tasks.Add(buildingUpdaterData.ProceedSave(this));
            }

            foreach (var playerCharacterUpdaterData in _playerCharacterUpdateDataDict.Values)
            {
                tasks.Add(playerCharacterUpdaterData.ProceedSave(this));
            }

            await UniTask.WhenAll(tasks);
            _updating = false;
        }

        internal void StorageItemsDateSaved(StorageId storageId)
        {
            _storageItemsUpdateDict.TryRemove(storageId, out _);
        }

        internal void BuildingDataSaved(string id)
        {
            _buildingUpdateDataDict.TryRemove(id, out _);
        }

        internal void PlayerCharacterDataSaved(string id)
        {
            _playerCharacterUpdateDataDict.TryRemove(id, out _);
        }
#endif
    }
}
