using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MapNetworkManagerDataUpdater : MonoBehaviour
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        internal static readonly HashSet<BuildingDataUpdater> BuildingDataUpdaters = new HashSet<BuildingDataUpdater>();
        internal static readonly HashSet<PlayerCharacterDataUpdater> PlayerCharacterDataUpdaters = new HashSet<PlayerCharacterDataUpdater>();
        private ConcurrentDictionary<string, BuildingUpdateData> _buildingUpdateDataDict = new ConcurrentDictionary<string, BuildingUpdateData>();
        private ConcurrentDictionary<string, PlayerCharacterUpdateData> _playerCharacterUpdateDataDict = new ConcurrentDictionary<string, PlayerCharacterUpdateData>();
        public MapNetworkManager Manager { get; internal set; }

        [SerializeField]
        private float buildingSaveInterval = 5f;
        [SerializeField]
        private float playerCharacterSaveInterval = 1f;

        [SerializeField]
        private float buildingSaveProceedInterval = 10f;
        [SerializeField]
        private float playerCharacterSaveProceedInterval = 10f;

        private bool _updating = false;

        private float _lastBuildingSaveTime;
        private float _lastPlayerCharacterSaveTime;

        private float _lastBuildingSaveProceedTime;
        private float _lastPlayerCharacterSaveProceedTime;

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

            float currentTime = Time.unscaledTime;
            if (currentTime - _lastBuildingSaveTime > buildingSaveInterval)
            {
                _lastBuildingSaveTime = currentTime;
                foreach (var updater in BuildingDataUpdaters)
                {
                    updater.EnqueueBuildingSave(this);
                }
            }

            if (currentTime - _lastPlayerCharacterSaveTime > playerCharacterSaveInterval)
            {
                _lastPlayerCharacterSaveTime = currentTime;
                foreach (var updater in PlayerCharacterDataUpdaters)
                {
                    updater.EnqueuePlayerCharacterSave(this);
                }
            }

            List<UniTask<bool>> tasks = new List<UniTask<bool>>();

            if (currentTime - _lastBuildingSaveProceedTime > buildingSaveProceedInterval)
            {
                _lastBuildingSaveProceedTime = currentTime;
                foreach (var buildingUpdaterData in _buildingUpdateDataDict.Values)
                {
                    tasks.Add(buildingUpdaterData.ProceedSave(this));
                }
            }

            if (currentTime - _lastPlayerCharacterSaveProceedTime > playerCharacterSaveProceedInterval)
            {
                _lastPlayerCharacterSaveProceedTime = currentTime;
                foreach (var playerCharacterUpdaterData in _playerCharacterUpdateDataDict.Values)
                {
                    tasks.Add(playerCharacterUpdaterData.ProceedSave(this));
                }
            }

            await UniTask.WhenAll(tasks);
            _updating = false;
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
