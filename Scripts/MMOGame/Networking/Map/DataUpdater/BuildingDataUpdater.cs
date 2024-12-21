using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(BuildingEntity))]
    [DisallowMultipleComponent]
    public class BuildingDataUpdater : MonoBehaviour
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public const float POSITION_CHANGE_THRESHOLD = 0.5f;
        public const int SAVE_DELAY = 1;

        private float _lastSavedTime;
        private BuildingEntity _entity;
        private TransactionUpdateBuildingState _updateState;
        // Combined hash
        private int _dirtyCombinedHash;
        // Current location state
        private Vector3 _dirtyCurrentPosition;

        private void Awake()
        {
            _entity = GetComponent<BuildingEntity>();
        }

        private void Start()
        {
            _lastSavedTime = Time.unscaledTime;
        }

        private void Update()
        {
            MapNetworkManager mapNetworkManager = BaseGameNetworkManager.Singleton as MapNetworkManager;
            if (!mapNetworkManager)
                return;

            int combinedHash = GetCombinedHashCode();
            if (_dirtyCombinedHash != combinedHash)
            {
                _dirtyCombinedHash = combinedHash;
                _updateState |= TransactionUpdateBuildingState.Building;
            }

            if (Vector3.Distance(_dirtyCurrentPosition, _entity.Position) > POSITION_CHANGE_THRESHOLD)
            {
                _dirtyCurrentPosition = _entity.Position;
                _updateState |= TransactionUpdateBuildingState.Building;
            }

            if (_updateState != TransactionUpdateBuildingState.None &&
                Time.unscaledTime - _lastSavedTime > SAVE_DELAY)
            {
                _lastSavedTime = Time.unscaledTime;
                mapNetworkManager.DataUpdater.EnqueueBuildingSave(_updateState, _entity);
                _updateState = TransactionUpdateBuildingState.None;
            }
        }

        public int GetCombinedHashCode()
        {
            int hash = 0;
            hash = System.HashCode.Combine(hash, _entity.Id);
            hash = System.HashCode.Combine(hash, _entity.ParentId);
            hash = System.HashCode.Combine(hash, _entity.EntityId);
            hash = System.HashCode.Combine(hash, _entity.CurrentHp);
            hash = System.HashCode.Combine(hash, _entity.RemainsLifeTime);
            hash = System.HashCode.Combine(hash, _entity.IsLocked);
            hash = System.HashCode.Combine(hash, _entity.LockPassword);
            hash = System.HashCode.Combine(hash, _entity.CreatorId);
            hash = System.HashCode.Combine(hash, _entity.CreatorName);
            hash = System.HashCode.Combine(hash, _entity.ExtraData);
            hash = System.HashCode.Combine(hash, _entity.IsSceneObject);
            return hash;
        }
#endif
    }
}
