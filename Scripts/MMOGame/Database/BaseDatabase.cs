using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public abstract partial class BaseDatabase : MonoBehaviour
    {
        public const byte AUTH_TYPE_NORMAL = 1;
        public const byte AUTH_TYPE_FACEBOOK = 2;
        public const byte AUTH_TYPE_GGPLAY = 3;

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

        public abstract Task CreateBuilding(string mapName, BuildingSaveData saveData);
        public abstract Task<List<BuildingSaveData>> ReadBuildings(string mapName);
        public abstract Task UpdateBuilding(string mapName, IBuildingSaveData saveData);
        public abstract Task DeleteBuilding(string mapName, string id);
    }
}
