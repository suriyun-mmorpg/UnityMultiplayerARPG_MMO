using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class BuildingUpdateData
    {
        private TransactionUpdateBuildingState _updateState;
        private BuildingSaveData _buildingSaveData;

        public void Update(TransactionUpdateBuildingState appendState, IBuildingSaveData buildingSaveData)
        {
            _buildingSaveData = null;
            if (buildingSaveData == null)
                return;
            _updateState |= appendState;
            _buildingSaveData = buildingSaveData.CloneTo(new BuildingSaveData());
        }

        public async UniTask<bool> ProceedSave(MapNetworkManagerDataUpdater updater)
        {
            if (_buildingSaveData == null || _updateState == TransactionUpdateBuildingState.None)
            {
                updater.BuildingDataSaved(_buildingSaveData.Id);
                return true;
            }
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (await updater.Manager.SaveBuilding(_updateState, _buildingSaveData))
            {
                updater.BuildingDataSaved(_buildingSaveData.Id);
                _updateState = TransactionUpdateBuildingState.None;
                _buildingSaveData = null;
                return true;
            }
#endif
            return false;
        }
    }
}