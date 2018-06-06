using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public abstract class BaseDatabase : MonoBehaviour
    {
        public abstract void Connect();
        public abstract void Disconnect();
        public abstract bool ValidateUserLogin(string username, string password, out string id);
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
        public abstract void DeleteCharacter(string userId, string id);
        public abstract long FindCharacterName(string characterName);

        public abstract void CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons);
        public abstract EquipWeapons ReadCharacterEquipWeapons(string characterId);
        public abstract void UpdateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons);
        public abstract void DeleteCharacterEquipWeapons(string characterId);

        public abstract void CreateCharacterAttribute(string characterId, CharacterAttribute characterAttribute);
        public abstract CharacterAttribute ReadCharacterAttribute(string characterId, string attributeId);
        public abstract List<CharacterAttribute> ReadCharacterAttributes(string characterId);
        public abstract void UpdateCharacterAttribute(string characterId, CharacterAttribute characterAttribute);
        public abstract void DeleteCharacterAttribute(string characterId, string attributeId);

        public abstract void CreateCharacterSkill(string characterId, CharacterSkill characterSkill);
        public abstract CharacterSkill ReadCharacterSkill(string characterId, string skillId);
        public abstract List<CharacterSkill> ReadCharacterSkills(string characterId);
        public abstract void UpdateCharacterSkill(string characterId, CharacterSkill characterSkill);
        public abstract void DeleteCharacterSkill(string characterId, string skillId);

        public abstract void CreateCharacterBuff(string characterId, CharacterBuff characterBuff);
        public abstract CharacterBuff ReadCharacterBuff(string characterId, string id);
        public abstract List<CharacterBuff> ReadCharacterBuffs(string characterId);
        public abstract void UpdateCharacterBuff(string characterId, CharacterBuff characterBuff);
        public abstract void DeleteCharacterBuff(string characterId, string id);

        public abstract void CreateCharacterEquipItem(string characterId, CharacterItem characterItem);
        public abstract CharacterItem ReadCharacterEquipItem(string characterId, string id);
        public abstract List<CharacterItem> ReadCharacterEquipItems(string characterId);
        public abstract void UpdateCharacterEquipItem(string characterId, CharacterItem characterItem);
        public abstract void DeleteCharacterEquipItem(string characterId, string id);

        public abstract void CreateCharacterNonEquipItem(string characterId, CharacterItem characterItem);
        public abstract CharacterItem ReadCharacterNonEquipItem(string characterId, string id);
        public abstract List<CharacterItem> ReadCharacterNonEquipItems(string characterId);
        public abstract void UpdateCharacterNonEquipItem(string characterId, CharacterItem characterItem);
        public abstract void DeleteCharacterNonEquipItem(string characterId, string id);

        public abstract void CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey);
        public abstract CharacterHotkey ReadCharacterHotkey(string characterId, string hotkeyId);
        public abstract List<CharacterHotkey> ReadCharacterHotkeys(string characterId);
        public abstract void UpdateCharacterHotkey(string characterId, CharacterHotkey characterHotkey);
        public abstract void DeleteCharacterHotkey(string characterId, string hotkeyId);

        public abstract void CreateCharacterQuest(string characterId, CharacterQuest characterQuest);
        public abstract CharacterQuest ReadCharacterQuest(string characterId, string questId);
        public abstract List<CharacterQuest> ReadCharacterQuests(string characterId);
        public abstract void UpdateCharacterQuest(string characterId, CharacterQuest characterQuest);
        public abstract void DeleteCharacterQuest(string characterId, string questId);
    }
}
