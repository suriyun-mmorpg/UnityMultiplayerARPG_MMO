using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public abstract class BaseDatabase : MonoBehaviour
    {
        public abstract bool ValidateUserLogin(string username, string password);
        public abstract void CreateUserLogin(string username, string password);
        public abstract long FindUsername(string username);
        public abstract void CreateCharacter(string userId, PlayerCharacterData characterData);
        public abstract PlayerCharacterData ReadCharacter(string id, 
            bool withEquipWeapons = true, 
            bool withAttributes = true, 
            bool withSkills = true, 
            bool withBuffs = true, 
            bool withEquipItems = true, 
            bool withNonEquipItems = true, 
            bool withHotkeys = true, 
            bool withQuests = true);
        public abstract List<PlayerCharacterData> ReadCharacters(string userId);
        public abstract void UpdateCharacter(PlayerCharacterData characterData);
        public abstract void DeleteCharacter(string id);
        public abstract long FindCharacterName(string characterName);
        public abstract CharacterItem ReadCharacterEquipWeapon(string id);
        public abstract EquipWeapons ReadCharacterEquipWeapons(string characterId);
        public abstract CharacterAttribute ReadCharacterAttribute(string characterId, string attributeId);
        public abstract List<CharacterAttribute> ReadCharacterAttributes(string characterId);
        public abstract CharacterSkill ReadCharacterSkill(string characterId, string skillId);
        public abstract List<CharacterSkill> ReadCharacterSkills(string characterId);
        public abstract CharacterBuff ReadCharacterBuff(string id);
        public abstract List<CharacterBuff> ReadCharacterBuffs(string characterId);
        public abstract CharacterItem ReadCharacterEquipItem(string id);
        public abstract List<CharacterItem> ReadCharacterEquipItems(string characterId);
        public abstract CharacterItem ReadCharacterNonEquipItem(string id);
        public abstract List<CharacterItem> ReadCharacterNonEquipItems(string characterId);
        public abstract CharacterHotkey ReadCharacterHotkey(string characterId, string hotkeyId);
        public abstract List<CharacterHotkey> ReadCharacterHotkeys(string characterId);
        public abstract CharacterQuest ReadCharacterQuest(string characterId, string questId);
        public abstract List<CharacterQuest> ReadCharacterQuests(string characterId);
    }
}
