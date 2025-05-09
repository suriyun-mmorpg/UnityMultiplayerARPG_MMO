using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(BasePlayerCharacterEntity))]
    [DisallowMultipleComponent]
    public class PlayerCharacterDataUpdater : MonoBehaviour
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public const float POSITION_CHANGE_THRESHOLD = 0.5f;

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
            // Register the updater to call for update later
            MapNetworkManagerDataUpdater.PlayerCharacterDataUpdaters.Add(this);
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
            // Register the updater to call for update later
            MapNetworkManagerDataUpdater.PlayerCharacterDataUpdaters.Remove(this);
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

        private void _entity_onAttributesOperation(LiteNetLibSyncListOp operation, int index, CharacterAttribute oldItem, CharacterAttribute newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Attributes;
        }

        private void _entity_onSkillsOperation(LiteNetLibSyncListOp operation, int index, CharacterSkill oldItem, CharacterSkill newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Skills;
        }

        private void _entity_onSkillUsagesOperation(LiteNetLibSyncListOp operation, int index, CharacterSkillUsage oldItem, CharacterSkillUsage newItem)
        {
            _updateState |= TransactionUpdateCharacterState.SkillUsages;
        }

        private void _entity_onBuffsOperation(LiteNetLibSyncListOp operation, int index, CharacterBuff oldItem, CharacterBuff newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Buffs;
        }

        private void _entity_onEquipItemsOperation(LiteNetLibSyncListOp operation, int index, CharacterItem oldItem, CharacterItem newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onNonEquipItemsOperation(LiteNetLibSyncListOp operation, int index, CharacterItem oldItem, CharacterItem newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onSelectableWeaponSetsOperation(LiteNetLibSyncListOp operation, int index, EquipWeapons oldItem, EquipWeapons newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Items;
        }

        private void _entity_onSummonsOperation(LiteNetLibSyncListOp operation, int index, CharacterSummon oldItem, CharacterSummon newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Summons;
        }

        private void _entity_onHotkeysOperation(LiteNetLibSyncListOp operation, int index, CharacterHotkey oldItem, CharacterHotkey newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Hotkeys;
        }

        private void _entity_onQuestsOperation(LiteNetLibSyncListOp operation, int index, CharacterQuest oldItem, CharacterQuest newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Quests;
        }

        private void _entity_onCurrenciesOperation(LiteNetLibSyncListOp operation, int index, CharacterCurrency oldItem, CharacterCurrency newItem)
        {
            _updateState |= TransactionUpdateCharacterState.Currencies;
        }

        private void _entity_onServerBoolsOperation(NotifiableCollection.NotifiableListAction operation, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onServerIntsOperation(NotifiableCollection.NotifiableListAction operation, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onServerFloatsOperation(NotifiableCollection.NotifiableListAction operation, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.ServerCustomData;
        }

        private void _entity_onPrivateBoolsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPrivateIntsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPrivateFloatsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PrivateCustomData;
        }

        private void _entity_onPublicBoolsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataBoolean oldItem, CharacterDataBoolean newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onPublicIntsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataInt32 oldItem, CharacterDataInt32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onPublicFloatsOperation(LiteNetLibSyncListOp operation, int index, CharacterDataFloat32 oldItem, CharacterDataFloat32 newItem)
        {
            _updateState |= TransactionUpdateCharacterState.PublicCustomData;
        }

        private void _entity_onMountChange(CharacterMount obj)
        {
            _updateState |= TransactionUpdateCharacterState.Mount;
        }

        internal void EnqueuePlayerCharacterSave(MapNetworkManagerDataUpdater updater)
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

            if (_updateState != TransactionUpdateCharacterState.None)
            {
                updater.EnqueuePlayerCharacterSave(_updateState, _entity);
                _updateState = TransactionUpdateCharacterState.None;
            }
        }

        public int GetCombinedHashCode()
        {
            int hash = 0;
            hash = System.HashCode.Combine(hash, _entity.Id);
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
#endif
    }
}
