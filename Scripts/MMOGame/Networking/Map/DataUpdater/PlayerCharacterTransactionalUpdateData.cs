using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class PlayerCharacterTransactionalUpdateData
    {
        private TransactionUpdateCharacterState _updateState;
        private PlayerCharacterData _playerCharacterData;

        public void Update(TransactionUpdateCharacterState appendState, IPlayerCharacterData playerCharacterData)
        {
            _playerCharacterData = null;
            if (playerCharacterData == null)
                return;
            _updateState |= appendState;
            _playerCharacterData = playerCharacterData.CloneTo(new PlayerCharacterData());
        }

        public void ProceedSave()
        {
            _updateState = TransactionUpdateCharacterState.None;
        }
    }
}