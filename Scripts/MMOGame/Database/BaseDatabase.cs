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
        public abstract PlayerCharacterData ReadCharacter(string characterId, 
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
        public abstract void DeleteCharacter(string characterId);
        public abstract long FindCharacterName(string characterName);
    }
}
