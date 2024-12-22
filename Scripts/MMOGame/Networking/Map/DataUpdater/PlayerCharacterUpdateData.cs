using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class PlayerCharacterUpdateData
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public TransactionUpdateCharacterState _updateState;
        public PlayerCharacterData _playerCharacterData;

        public void Update(TransactionUpdateCharacterState appendState, IPlayerCharacterData playerCharacterData)
        {
            _playerCharacterData = null;
            if (playerCharacterData == null)
                return;
            _updateState |= appendState;
            _playerCharacterData = playerCharacterData.CloneTo(new PlayerCharacterData());
        }

        public async UniTask<bool> ProceedSave(MapNetworkManagerDataUpdater updater)
        {
            if (_playerCharacterData == null || _updateState == TransactionUpdateCharacterState.None)
            {
                updater.PlayerCharacterDataSaved(_playerCharacterData.Id);
                return true;
            }
            if (await updater.Manager.SaveCharacter(_updateState, _playerCharacterData))
            {
                updater.PlayerCharacterDataSaved(_playerCharacterData.Id);
                _updateState = TransactionUpdateCharacterState.None;
                _playerCharacterData = null;
                return true;
            }
            return false;
        }
#endif
    }
}