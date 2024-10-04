using System.Collections.Generic;
using System.Text;

namespace MultiplayerARPG.MMO
{
    public partial class DefaultCentralServerDataManager : ICentralServerDataManager
    {
        public string GenerateCharacterId()
        {
            return GenericUtils.GetUniqueId();
        }

        public string GenerateMapSpawnInstanceId()
        {
            return GenericUtils.GetUniqueId();
        }

        public bool CanCreateCharacter(ref int dataId, ref int entityId, ref int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats, out UITextKeys errorMessage)
        {
            errorMessage = UITextKeys.NONE;

            if (!GameInstance.PlayerCharacters.ContainsKey(dataId))
            {
                // No player character data
                errorMessage = UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA;
                return false;
            }

            if (
#if !EXCLUDE_PREFAB_REFS
                !GameInstance.PlayerCharacterEntities.ContainsKey(entityId) &&
#endif
                !GameInstance.AddressablePlayerCharacterEntities.ContainsKey(entityId) &&
                !GameInstance.PlayerCharacterEntityMetaDataList.ContainsKey(entityId))
            {
                // No player character entity
                errorMessage = UITextKeys.UI_ERROR_INVALID_CHARACTER_ENTITY;
                return false;
            }

            if (GameInstance.Factions.Count <= 0)
            {
                // No factions to select
                factionId = 0;
                return true;
            }

            if (GameInstance.Factions.ContainsKey(factionId) && !GameInstance.Factions[factionId].IsLocked)
            {
                // Can select the faction
                return true;
            }

            List<Faction> notLockedFactions = new List<Faction>();
            foreach (Faction faction in GameInstance.Factions.Values)
            {
                if (faction == null)
                    continue;
                if (!faction.IsLocked)
                    notLockedFactions.Add(faction);
            }

            // Random faction, if player doesn't select it properly
            if (notLockedFactions.Count > 0)
                factionId = notLockedFactions[GenericUtils.RandomInt(0, notLockedFactions.Count)].DataId;
            else
                factionId = 0;

            return true;
        }

        public void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            playerCharacterData.SetNewPlayerCharacterData(characterName, dataId, entityId, factionId);
#if !DISABLE_CUSTOM_CHARACTER_DATA
            playerCharacterData.PublicBools = publicBools;
            playerCharacterData.PublicInts = publicInts;
            playerCharacterData.PublicFloats = publicFloats;
#endif
        }

        public string GenerateAccessToken(string userId)
        {
            string str = $"{userId}_{System.DateTime.Now.ToLongDateString()}";
            return System.Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        public string GetUserIdFromAccessToken(string accessToken)
        {
            string str = Encoding.ASCII.GetString(System.Convert.FromBase64String(accessToken));
            string[] splitedStr = str.Split('_');
            if (splitedStr.Length > 0)
                return splitedStr[0];
            return string.Empty;
        }
    }
}
