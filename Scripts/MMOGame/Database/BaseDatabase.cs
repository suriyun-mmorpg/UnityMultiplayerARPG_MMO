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

        public abstract string ValidateUserLogin(string username, string password);
        public abstract bool ValidateAccessToken(string userId, string accessToken);
        public abstract int GetCash(string userId);
        public abstract int IncreaseCash(string userId, int amount);
        public abstract int DecreaseCash(string userId, int amount);
        public abstract void UpdateAccessToken(string userId, string accessToken);
        public abstract void CreateUserLogin(string username, string password);
        public abstract long FindUsername(string username);
        public abstract string FacebookLogin(string fbId, string accessToken);
        public abstract string GooglePlayLogin(string idToken);

        public abstract void CreateCharacter(string userId, IPlayerCharacterData characterData);
        public abstract PlayerCharacterData ReadCharacter(string userId, 
            string id,
            bool withEquipWeapons = true, 
            bool withAttributes = true, 
            bool withSkills = true, 
            bool withBuffs = true, 
            bool withEquipItems = true, 
            bool withNonEquipItems = true, 
            bool withHotkeys = true, 
            bool withQuests = true);
        public abstract List<PlayerCharacterData> ReadCharacters(string userId);
        public abstract void UpdateCharacter(IPlayerCharacterData character);
        public abstract void DeleteCharacter(string userId, string id);
        public abstract long FindCharacterName(string characterName);

        public abstract void CreateBuilding(string mapName, IBuildingSaveData saveData);
        public abstract List<BuildingSaveData> ReadBuildings(string mapName);
        public abstract void UpdateBuilding(string mapName, IBuildingSaveData building);
        public abstract void DeleteBuilding(string mapName, string id);

        public abstract int CreateParty(bool shareExp, bool shareItem, string leaderId);
        public abstract PartyData ReadParty(int id);
        public abstract void UpdateParty(int id, bool shareExp, bool shareItem);
        public abstract void DeleteParty(int id);
        public abstract void SetCharacterParty(string characterId, int partyId);

        public abstract int CreateGuild(string guildName, string leaderId, string leaderName);
        public abstract GuildData ReadGuild(int id);
        public abstract void UpdateGuildMessage(int id, string message);
        public abstract void DeleteGuild(int id);
        public abstract void SetCharacterGuild(string characterId, int guild);
    }
}
