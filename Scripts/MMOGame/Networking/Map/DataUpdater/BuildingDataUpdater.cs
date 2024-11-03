using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(BuildingEntity))]
    [DisallowMultipleComponent]
    public class BuildingDataUpdater : MonoBehaviour
    {
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
                EnqueueSave();
            }
        }

        public int GetCombinedHashCode()
        {
            int hash = _entity.Id.GetHashCode();
            hash = System.HashCode.Combine(hash, _entity.ParentId.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.EntityId.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.CurrentHp.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.RemainsLifeTime.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.IsLocked.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.LockPassword.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.CreatorId.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.CreatorName.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.ExtraData.GetHashCode());
            hash = System.HashCode.Combine(hash, _entity.IsSceneObject.GetHashCode());
            return hash;
        }

        public void EnqueueSave()
        {
            if (BaseGameNetworkManager.Singleton.TryGetComponent(out MapNetworkManagerDataUpdater updater))
            {
                updater.EnqueueBuildingSave(_updateState, _entity);
                _updateState = TransactionUpdateBuildingState.None;
            }
        }
    }
}
