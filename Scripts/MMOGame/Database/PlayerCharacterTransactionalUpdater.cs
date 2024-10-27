using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DefaultExecutionOrder(int.MinValue)]
    [RequireComponent(typeof(BasePlayerCharacterEntity))]
    public class PlayerCharacterTransactionalUpdater : MonoBehaviour
    {
        public const float POSITION_CHANGE_THRESHOLD = 0.5f;
        public const int FRAMES_BEFORE_SAVE = 30;

        private int _lastSavedFrame;
        private BasePlayerCharacterEntity _entity;
        private TransactionUpdateCharacterState _updateState;
        // Social state
        private byte _dirtyGuildRole;
        // Current location state
        private string _dirtyCurrentChannel;
        private string _dirtyCurrentMapName;
        private Vector3 _dirtyCurrentPosition;
        // Respawn location state
        private string _dirtyRespawnMapName;
        private Vector3 _dirtyRespawnPosition;
        // Accessibility state
        private long _dirtyUnmuteTime;

        private void Awake()
        {
            _entity = GetComponent<BasePlayerCharacterEntity>();
            // Level and Exp
            _entity.onLevelChange += _entity_onLevelChange;
            _entity.onExpChange += _entity_onExpChange;
            // Generic Stats
            _entity.onCurrentHpChange += _entity_onCurrentHpChange;
            _entity.onCurrentMpChange += _entity_onCurrentMpChange;
            _entity.onCurrentStaminaChange += _entity_onCurrentStaminaChange;
            _entity.onCurrentFoodChange += _entity_onCurrentFoodChange;
            _entity.onCurrentWaterChange += _entity_onCurrentWaterChange;
            // Generic Points
            _entity.onStatPointChange += _entity_onStatPointChange;
            _entity.onSkillPointChange += _entity_onSkillPointChange;
            _entity.onReputationChange += _entity_onReputationChange;
            // Built-In Character Currncies
            _entity.onGoldChange += _entity_onGoldChange;
            // Built-In User Currncies
            _entity.onUserGoldChange += _entity_onUserGoldChange;
            _entity.onUserCashChange += _entity_onUserCashChange;
            // Social
            _entity.onIconDataIdChange += _entity_onIconDataIdChange;
            _entity.onFrameDataIdChange += _entity_onFrameDataIdChange;
            _entity.onTitleDataIdChange += _entity_onTitleDataIdChange;
            _entity.onFactionIdChange += _entity_onFactionIdChange;
            _entity.onPartyIdChange += _entity_onPartyIdChange;
            _entity.onGuildIdChange += _entity_onGuildIdChange;
            // Pk
            _entity.onIsPkOnChange += _entity_onIsPkOnChange;
            _entity.onPkPointChange += _entity_onPkPointChange;
            _entity.onConsecutivePkKillsChange += _entity_onConsecutivePkKillsChange;
            // Equip Weapons
            _entity.onEquipWeaponSetChange += _entity_onEquipWeaponSetChange;
            _entity.onIsWeaponsSheathedChange += _entity_onIsWeaponsSheathedChange;
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
            // Currencies
            _entity.onCurrenciesOperation += _entity_onCurrenciesOperation;
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
            // Mount
            _entity.onMountChange += _entity_onMountChange;
        }

        private void Start()
        {
            _lastSavedFrame = Time.frameCount;
        }

        private void OnDestroy()
        {
            // Level and Exp
            _entity.onLevelChange -= _entity_onLevelChange;
            _entity.onExpChange -= _entity_onExpChange;
            // Generic Stats
            _entity.onCurrentHpChange -= _entity_onCurrentHpChange;
            _entity.onCurrentMpChange -= _entity_onCurrentMpChange;
            _entity.onCurrentStaminaChange -= _entity_onCurrentStaminaChange;
            _entity.onCurrentFoodChange -= _entity_onCurrentFoodChange;
            _entity.onCurrentWaterChange -= _entity_onCurrentWaterChange;
            // Generic Points
            _entity.onStatPointChange -= _entity_onStatPointChange;
            _entity.onSkillPointChange -= _entity_onSkillPointChange;
            _entity.onReputationChange -= _entity_onReputationChange;
            // Built-In Character Currncies
            _entity.onGoldChange -= _entity_onGoldChange;
            // Built-In User Currncies
            _entity.onUserGoldChange -= _entity_onUserGoldChange;
            _entity.onUserCashChange -= _entity_onUserCashChange;
            // Social
            _entity.onIconDataIdChange -= _entity_onIconDataIdChange;
            _entity.onFrameDataIdChange -= _entity_onFrameDataIdChange;
            _entity.onTitleDataIdChange -= _entity_onTitleDataIdChange;
            _entity.onFactionIdChange -= _entity_onFactionIdChange;
            _entity.onPartyIdChange -= _entity_onPartyIdChange;
            _entity.onGuildIdChange -= _entity_onGuildIdChange;
            // Pk
            _entity.onIsPkOnChange -= _entity_onIsPkOnChange;
            _entity.onPkPointChange -= _entity_onPkPointChange;
            _entity.onConsecutivePkKillsChange -= _entity_onConsecutivePkKillsChange;
            // Equip Weapons
            _entity.onEquipWeaponSetChange -= _entity_onEquipWeaponSetChange;
            _entity.onIsWeaponsSheathedChange -= _entity_onIsWeaponsSheathedChange;
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
            // Currencies
            _entity.onCurrenciesOperation -= _entity_onCurrenciesOperation;
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
            // Mount
            _entity.onMountChange -= _entity_onMountChange;
        }

        private void _entity_onLevelChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.LevelAndExp;
        }

        private void _entity_onExpChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.LevelAndExp;
        }

        private void _entity_onCurrentHpChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericStats;
        }

        private void _entity_onCurrentMpChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericStats;
        }

        private void _entity_onCurrentStaminaChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericStats;
        }

        private void _entity_onCurrentFoodChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericStats;
        }

        private void _entity_onCurrentWaterChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericStats;
        }

        private void _entity_onReputationChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericPoints;
        }

        private void _entity_onStatPointChange(float obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericPoints;
        }

        private void _entity_onSkillPointChange(float obj)
        {
            _updateState |= TransactionUpdateCharacterState.GenericPoints;
        }

        private void _entity_onGoldChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.BuiltInCharacterCurrncies;
        }

        private void _entity_onUserGoldChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.BuiltInUserCurrencies;
        }

        private void _entity_onUserCashChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.BuiltInUserCurrencies;
        }

        private void _entity_onIconDataIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
        }

        private void _entity_onFrameDataIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
        }

        private void _entity_onTitleDataIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
        }

        private void _entity_onFactionIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
        }

        private void _entity_onPartyIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
        }

        private void _entity_onGuildIdChange(int obj)
        {
            _updateState |= TransactionUpdateCharacterState.Social;
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

        private void _entity_onEquipWeaponSetChange(byte obj)
        {
            _updateState |= TransactionUpdateCharacterState.EquipWeapons;
        }

        private void _entity_onIsWeaponsSheathedChange(bool obj)
        {
            _updateState |= TransactionUpdateCharacterState.EquipWeapons;
        }

        private void _entity_onSelectableWeaponSetsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.EquipWeapons;
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
            _updateState |= TransactionUpdateCharacterState.EquipItems;
        }

        private void _entity_onNonEquipItemsOperation(LiteNetLibManager.LiteNetLibSyncList.Operation arg1, int arg2)
        {
            _updateState |= TransactionUpdateCharacterState.NonEquipItems;
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
            if (_dirtyGuildRole != _entity.GuildRole)
            {
                _dirtyGuildRole = _entity.GuildRole;
                _updateState |= TransactionUpdateCharacterState.Social;
            }

            if (!string.Equals(_dirtyCurrentChannel, _entity.CurrentChannel))
            {
                _dirtyCurrentChannel = _entity.CurrentChannel;
                _updateState |= TransactionUpdateCharacterState.CurrentLocation;
            }

            if (!string.Equals(_dirtyCurrentMapName, _entity.CurrentMapName))
            {
                _dirtyCurrentMapName = _entity.CurrentMapName;
                _updateState |= TransactionUpdateCharacterState.CurrentLocation;
            }

            if (Vector3.Distance(_dirtyCurrentPosition, _entity.CurrentPosition) > POSITION_CHANGE_THRESHOLD)
            {
                _dirtyCurrentPosition = _entity.CurrentPosition;
                _updateState |= TransactionUpdateCharacterState.CurrentLocation;
            }

            if (!string.Equals(_dirtyRespawnMapName, _entity.RespawnMapName))
            {
                _dirtyRespawnMapName = _entity.RespawnMapName;
                _updateState |= TransactionUpdateCharacterState.RespawnLocation;
            }

            if (Vector3.Distance(_dirtyRespawnPosition, _entity.RespawnPosition) > POSITION_CHANGE_THRESHOLD)
            {
                _dirtyRespawnPosition = _entity.RespawnPosition;
                _updateState |= TransactionUpdateCharacterState.RespawnLocation;
            }

            if (_dirtyUnmuteTime != _entity.UnmuteTime)
            {
                _dirtyUnmuteTime = _entity.UnmuteTime;
                _updateState |= TransactionUpdateCharacterState.Accessibility;
            }

            if (_updateState != TransactionUpdateCharacterState.None &&
                Time.frameCount - _lastSavedFrame > FRAMES_BEFORE_SAVE)
            {
                _lastSavedFrame = Time.frameCount;
                _updateState = TransactionUpdateCharacterState.None;
                EnqueueSave();
            }
        }

        public async void EnqueueSave()
        {
        }
    }
}
