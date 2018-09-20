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
        public const byte AUTH_TYPE_GOOGLE_PLAY = 3;

        public virtual void Initialize() { }
        public virtual void Destroy() { }

        public abstract Task<string> ValidateUserLogin(string username, string password);
        public abstract Task<bool> ValidateAccessToken(string userId, string accessToken);
        public abstract Task<int> GetCash(string userId);
        public abstract Task<int> IncreaseCash(string userId, int amount);
        public abstract Task<int> DecreaseCash(string userId, int amount);
        public abstract Task UpdateAccessToken(string userId, string accessToken);
        public abstract Task CreateUserLogin(string username, string password);
        public abstract Task<long> FindUsername(string username);
        public abstract Task<string> FacebookLogin(string fbId, string accessToken);
        public abstract Task<string> GooglePlayLogin(string idToken);

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
        public abstract Task UpdateCharacter(IPlayerCharacterData character);
        public abstract Task DeleteCharacter(string userId, string id);
        public abstract Task<long> FindCharacterName(string characterName);

        public abstract Task CreateBuilding(string mapName, BuildingSaveData saveData);
        public abstract Task<List<BuildingSaveData>> ReadBuildings(string mapName);
        public abstract Task UpdateBuilding(string mapName, IBuildingSaveData building);
        public abstract Task DeleteBuilding(string mapName, string id);

        public abstract Task<int> CreateParty(bool shareExp, bool shareItem, string leaderId);
        public abstract Task<PartyData> ReadParty(int id);
        public abstract Task UpdateParty(int id, bool shareExp, bool shareItem);
        public abstract Task DeleteParty(int id);
        public abstract Task SetCharacterParty(string characterId, int partyId);
    }
}
