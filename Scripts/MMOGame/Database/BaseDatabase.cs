using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Insthync.MMOG
{
    public abstract partial class BaseDatabase : MonoBehaviour
    {
        public abstract Task<string> ValidateUserLogin(string username, string password);
        public abstract Task<bool> ValidateAccessToken(string userId, string accessToken);
        public abstract Task UpdateAccessToken(string userId, string accessToken);
        public abstract Task CreateUserLogin(string username, string password);
        public abstract Task<long> FindUsername(string username);

        public abstract Task CreateCharacter(string userId, PlayerCharacterData characterData);
        public abstract Task<PlayerCharacterData> ReadCharacter(string userId, 
            string id,
            bool withEquipWeapons = true, 
            bool withAttributes = true, 
            bool withSkills = true, 
            bool withBuffs = true, 
            bool withEquipItems = true, 
            bool withNonEquipItems = true, 
            bool withHotkeys = true, 
            bool withQuests = true);
        public abstract Task<List<PlayerCharacterData>> ReadCharacters(string userId);
        public abstract Task UpdateCharacter(IPlayerCharacterData characterData);
        public abstract Task DeleteCharacter(string userId, string id);
        public abstract Task<long> FindCharacterName(string characterName);

        public abstract Task CreateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons);
        public abstract Task<EquipWeapons> ReadCharacterEquipWeapons(string characterId);
        public abstract Task UpdateCharacterEquipWeapons(string characterId, EquipWeapons equipWeapons);
        public abstract Task DeleteCharacterEquipWeapons(string characterId);

        public abstract Task CreateCharacterAttribute(string characterId, CharacterAttribute characterAttribute);
        public abstract Task<CharacterAttribute> ReadCharacterAttribute(string characterId, string attributeId);
        public abstract Task<List<CharacterAttribute>> ReadCharacterAttributes(string characterId);
        public abstract Task UpdateCharacterAttribute(string characterId, CharacterAttribute characterAttribute);
        public abstract Task DeleteCharacterAttribute(string characterId, string attributeId);

        public abstract Task CreateCharacterSkill(string characterId, CharacterSkill characterSkill);
        public abstract Task<CharacterSkill> ReadCharacterSkill(string characterId, string skillId);
        public abstract Task<List<CharacterSkill>> ReadCharacterSkills(string characterId);
        public abstract Task UpdateCharacterSkill(string characterId, CharacterSkill characterSkill);
        public abstract Task DeleteCharacterSkill(string characterId, string skillId);

        public abstract Task CreateCharacterBuff(string characterId, CharacterBuff characterBuff);
        public abstract Task<CharacterBuff> ReadCharacterBuff(string characterId, string id);
        public abstract Task<List<CharacterBuff>> ReadCharacterBuffs(string characterId);
        public abstract Task UpdateCharacterBuff(string characterId, CharacterBuff characterBuff);
        public abstract Task DeleteCharacterBuff(string characterId, string id);

        public abstract Task CreateCharacterEquipItem(string characterId, CharacterItem characterItem);
        public abstract Task<CharacterItem> ReadCharacterEquipItem(string characterId, string id);
        public abstract Task<List<CharacterItem>> ReadCharacterEquipItems(string characterId);
        public abstract Task UpdateCharacterEquipItem(string characterId, CharacterItem characterItem);
        public abstract Task DeleteCharacterEquipItem(string characterId, string id);

        public abstract Task CreateCharacterNonEquipItem(string characterId, CharacterItem characterItem);
        public abstract Task<CharacterItem> ReadCharacterNonEquipItem(string characterId, string id);
        public abstract Task<List<CharacterItem>> ReadCharacterNonEquipItems(string characterId);
        public abstract Task UpdateCharacterNonEquipItem(string characterId, CharacterItem characterItem);
        public abstract Task DeleteCharacterNonEquipItem(string characterId, string id);

        public abstract Task CreateCharacterHotkey(string characterId, CharacterHotkey characterHotkey);
        public abstract Task<CharacterHotkey> ReadCharacterHotkey(string characterId, string hotkeyId);
        public abstract Task<List<CharacterHotkey>> ReadCharacterHotkeys(string characterId);
        public abstract Task UpdateCharacterHotkey(string characterId, CharacterHotkey characterHotkey);
        public abstract Task DeleteCharacterHotkey(string characterId, string hotkeyId);

        public abstract Task CreateCharacterQuest(string characterId, CharacterQuest characterQuest);
        public abstract Task<CharacterQuest> ReadCharacterQuest(string characterId, string questId);
        public abstract Task<List<CharacterQuest>> ReadCharacterQuests(string characterId);
        public abstract Task UpdateCharacterQuest(string characterId, CharacterQuest characterQuest);
        public abstract Task DeleteCharacterQuest(string characterId, string questId);
    }
}
