using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public class CentralServerDataManager : ICentralServerDataManager
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

            if (!GameInstance.PlayerCharacterEntities.ContainsKey(entityId) && !GameInstance.AddressablePlayerCharacterEntities.ContainsKey(entityId))
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
            playerCharacterData.PublicBools = publicBools;
            playerCharacterData.PublicInts = publicInts;
            playerCharacterData.PublicFloats = publicFloats;
        }
    }
}
