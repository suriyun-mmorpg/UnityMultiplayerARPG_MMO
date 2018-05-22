using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public abstract class BaseDatabase : MonoBehaviour
    {
        public abstract bool ValidateLogin(string username, string password);
        public abstract UserLoginData Register(string username, string password);
        public abstract long FindUsername(string username);
        public abstract bool CreateCharacter(string userId, PlayerCharacterData characterData);
        public abstract PlayerCharacterData ReadCharacter(string characterId);
        public abstract List<LitePlayerCharacterData> ReadCharacters(string userId);
        public abstract bool UpdateCharacter(PlayerCharacterData characterData);
        public abstract bool DeleteCharacter(string characterId);
        public abstract long FindCharacterName(string characterName);
    }
}
