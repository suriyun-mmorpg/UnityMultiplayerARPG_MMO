using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(BasePlayerCharacterEntity))]
    [DisallowMultipleComponent]
    public class PlayerCharacterDataUpdater : MonoBehaviour
    {
        public const float POSITION_CHANGE_THRESHOLD = 0.5f;
        public const int SAVE_DELAY = 1;

        private float _lastSavedTime;
        private BasePlayerCharacterEntity _entity;
        private TransactionUpdateCharacterState _updateState;
        // Combined hash
        private int _dirtyCombinedHash;
        // Current location state
        private Vector3 _dirtyCurrentPosition;
        // Respawn location state
        private Vector3 _dirtyRespawnPosition;

        private void Awake()
        {
            _entity = GetComponent<BasePlayerCharacterEntity>();
#if !DISABLE_CLASSIC_PK
            // Pk
            _entity.onIsPkOnChange += _entity_onIsPkOnChange;
            _entity.onPkPointChange += _entity_onPkPointChange;
            _entity.onConsecutivePkKillsChange += _entity_onConsecutivePkKillsChange;
#endif
            // Selectable Weapon Sets
            _entity.onSelectableWeaponSetsOperation += _entity_onSelectableWeaponSetsOperation;
            // Attributes
            _entity.onAttributesOperation += _entity_onAttributesOperation;
            // Skills
            _entity.onSkillsOperation += _entity_onSkillsOperation;
            // Skill Usages
            _entity.onSkillUsagesOperation += _entity_onSkillUsagesOperation;
            // Buffs
            _entity.onBuffsOperation += _entity_onBuffsOperation;
            // Equip Items
            _entity.onEquipItemsOperation += _entity_onEquipItemsOperation;
            // Non Equip Items
            _entity.onNonEquipItemsOperation += _entity_onNonEquipItemsOperation;
            // Summons
            _entity.onSummonsOperation += _entity_onSummonsOperation;
            // Hotkeys
            _entity.onHotkeysOperation += _entity_onHotkeysOperation;
            // Quests
            _entity.onQuestsOperation += _entity_onQuestsOperation;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            // Currencies
            _entity.onCurrenciesOperation += _entity_onCurrenciesOperation;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            // Server Bools
            _entity.onServerBoolsOperation += _entity_onServerBoolsOperation;
            _entity.onServerIntsOperation += _entity_onServerIntsOperation;
            _entity.onServerFloatsOperation += _entity_onServerFloatsOperation;
            // Private Bools
            _entity.onPrivateBoolsOperation += _entity_onPrivateBoolsOperation;
            _entity.onPrivateIntsOperation += _entity_onPrivateIntsOperation;
            _entity.onPrivateFloatsOperation += _entity_onPrivateFloatsOperation;
            // Public Bools
            _entity.onPublicBoolsOperation += _entity_onPublicBoolsOperation;
            _entity.onPublicIntsOperation += _entity_onPublicIntsOperation;
            _entity.onPublicFloatsOperation += _entity_onPublicFloatsOperation;
#endif
            // Mount
            _entity.onMountChange += _entity_onMountChange;
        }

        private void Start()
        {
            _lastSavedTime = Time.unscaledTime;
        }

        private void OnDestroy()
        {
#if !DISABLE_CLASSIC_PK
            // Pk
            _entity.onIsPkOnChange -= _entity_onIsPkOnChange;
            _entity.onPkPointChange -= _entity_onPkPointChange;
            _entity.onConsecutivePkKillsChange -= _entity_onConsecutivePkKillsChange;
#endif
            // Selectable Weapon Sets
            _entity.onSelectableWeaponSetsOperation -= _entity_onSelectableWeaponSetsOperation;
            // Attributes
            _entity.onAttributesOperation -= _entity_onAttributesOperation;
            // Skills
            _entity.onSkillsOperation -= _entity_onSkillsOperation;
            // Skill Usages
            _entity.onSkillUsagesOperation -= _entity_onSkillUsagesOperation;
            // Buffs
            _entity.onBuffsOperation -= _entity_onBuffsOperation;
            // Equip Items
            _entity.onEquipItemsOperation -= _entity_onEquipItemsOperation;
            // Non Equip Items
            _entity.onNonEquipItemsOperation -= _entity_onNonEquipItemsOperation;
            // Summons
            _entity.onSummonsOperation -= _entity_onSummonsOperation;
            // Hotkeys
            _entity.onHotkeysOperation -= _entity_onHotkeysOperation;
            // Quests
            _entity.onQuestsOperation -= _entity_onQuestsOperation;
#if !DISABLE_CUSTOM_CHARACTER_CURRENCIES
            // Currencies
            _entity.onCurrenciesOperation -= _entity_onCurrenciesOperation;
#endif
#if !DISABLE_CUSTOM_CHARACTER_DATA
            // Server Bools
            _entity.onServerBoolsOperation -= _entity_onServerBoolsOperation;
            _entity.onServerIntsOperation -= _entity_onServerIntsOperation;
            _entity.onServerFloatsOperation -= _entity_onServerFloatsOperation;
            // Private Bools
            _entity.onPrivateBoolsOperation -= _entity_onPrivateBoolsOperation;
            _entity.onPrivateIntsOperation -= _entity_onPrivateIntsOperation;
            _entity.onPrivateFloatsOperation -= _entity_onPrivateFloatsOperation;
            // Public Bools
            _entity.onPublicBoolsOperation -= _entity_onPublicBoolsOperation;
            _entity.onPublicIntsOperation -= _entity_onPublicIntsOperation;
            _entity.onPublicFloatsOperation -= _entity_onPublicFloatsOperation;
#endif
            // Mount
            _entity.onMountChange -= _entity_onMountChange;
        }

        private void _entity_onIsPkOnChange(bool obj)
        {
            _updateState |= TransactionUpdateCharacterState.Pk;
        }

        private void _entity_onPkPointChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Pk;
        }

        private void _entity_onConsecutivePkKillsChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Pk;
        }

        private void _entity_onAttributesOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Attributes;
        }

        private void _entity_onSkillsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Skills;
        }

        private void _entity_onSkillUsagesOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.SkillUsages;
        }

        private void _entity_onBuffsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Buffs;
        }

        private void _entity_onEquipItemsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onNonEquipItemsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onSelectableWeaponSetsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onSummonsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Summons;
        }

        private void _entity_onHotkeysOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Hotkeys;
        }

        private void _entity_onQuestsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Quests;
        }

        private void _entity_onCurrenciesOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.Currencies;
        }

        private void _entity_onServerBoolsOperation(NotifiableCollection.NotifiableListAction arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onServerIntsOperation(NotifiableCollection.NotifiableListAction arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onServerFloatsOperation(NotifiableCollection.NotifiableListAction arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onPrivateBoolsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPrivateIntsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPrivateFloatsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPublicBoolsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onPublicIntsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onPublicFloatsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onMountChange(CharacterMount obj)
        {
            _updateState |= TransactionUpdateCharacterState.Mount;
        }

        private void Update()
        {
            int combinedHash = GetCombinedHashCode();
            if (_dirtyCombinedHash != combinedHash)
            {
                _dirtyCombinedHash = combinedHash;
                _updateState |= TransactionUpdateCharacterState.Character;
            }

            if (Vector3.Distance(_dirtyCurrentPosition, _entity.CurrentPosition) > POSITION_CHANGE_THRESHOLD)
            {
                _dirtyCurrentPosition = _entity.CurrentPosition;
                _updateState |= TransactionUpdateCharacterState.Character;
            }

#if !DISABLE_DIFFER_MAP_RESPAWNING
            if (Vector3.Distance(_dirtyRespawnPosition, _entity.RespawnPosition) > POSITION_CHANGE_THRESHOLD)
            {
                _dirtyRespawnPosition = _entity.RespawnPosition;
                _updateState |= TransactionUpdateCharacterState.Character;
            }
#endif

            if (_updateState != TransactionUpdateCharacterState.None &&
                Time.unscaledTime - _lastSavedTime > SAVE_DELAY)
            {
                _lastSavedTime = Time.unscaledTime;
                EnqueueSave();
            }
        }

        public int GetCombinedHashCode()
        {
            int hash = _entity.Id.GetHashCode();
            hash = System.HashCode.Combine(hash, _entity.DataId);
            hash = System.HashCode.Combine(hash, _entity.EntityId);
            hash = System.HashCode.Combine(hash, _entity.UserId);
            hash = System.HashCode.Combine(hash, _entity.FactionId);
            hash = System.HashCode.Combine(hash, _entity.CharacterName);
            hash = System.HashCode.Combine(hash, _entity.Level);
            hash = System.HashCode.Combine(hash, _entity.Exp);
            hash = System.HashCode.Combine(hash, _entity.CurrentHp);
            hash = System.HashCode.Combine(hash, _entity.CurrentMp);
            hash = System.HashCode.Combine(hash, _entity.CurrentStamina);
            hash = System.HashCode.Combine(hash, _entity.CurrentFood);
            hash = System.HashCode.Combine(hash, _entity.CurrentWater);
            hash = System.HashCode.Combine(hash, _entity.StatPoint);
            hash = System.HashCode.Combine(hash, _entity.SkillPoint);
            hash = System.HashCode.Combine(hash, _entity.Gold);
            hash = System.HashCode.Combine(hash, _entity.PartyId);
            hash = System.HashCode.Combine(hash, _entity.GuildId);
            hash = System.HashCode.Combine(hash, _entity.GuildRole);
            hash = System.HashCode.Combine(hash, _entity.EquipWeaponSet);
            hash = System.HashCode.Combine(hash, _entity.CurrentChannel);
            hash = System.HashCode.Combine(hash, _entity.CurrentMapName);
            hash = System.HashCode.Combine(hash, _entity.CurrentSafeArea);
#if !DISABLE_DIFFER_MAP_RESPAWNING
            hash = System.HashCode.Combine(hash, _entity.RespawnMapName);
#endif
            hash = System.HashCode.Combine(hash, _entity.IconDataId);
            hash = System.HashCode.Combine(hash, _entity.FrameDataId);
            hash = System.HashCode.Combine(hash, _entity.TitleDataId);
            hash = System.HashCode.Combine(hash, _entity.LastDeadTime);
            hash = System.HashCode.Combine(hash, _entity.UnmuteTime);
            hash = System.HashCode.Combine(hash, _entity.LastUpdate);
            hash = System.HashCode.Combine(hash, _entity.Reputation);
            return hash;
        }

        public void EnqueueSave()
        {
            if (BaseGameNetworkManager.Singleton.TryGetComponent(out MapNetworkManagerDataUpdater updater))
            {
                updater.EnqueuePlayerCharacterSave(_updateState, _entity);
                _updateState = TransactionUpdateCharacterState.None;
            }
        }
    }
}
