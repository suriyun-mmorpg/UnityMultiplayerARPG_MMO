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

        public bool CanCreateCharacter(int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            if (!GameInstance.PlayerCharacters.ContainsKey(dataId))
            {
                // No player character data
                return false;
            }

            if (!GameInstance.PlayerCharacterEntities.ContainsKey(entityId))
            {
                // No player character entity
                return false;
            }

            if (GameInstance.Factions.Count <= 0)
            {
                // No factions to select
                return true;
            }

            if (GameInstance.Factions.ContainsKey(factionId) && !GameInstance.Factions[factionId].IsLocked)
            {
                // Can select the faction
                return true;
            }

            foreach (Faction faction in GameInstance.Factions.Values)
            {
                if (!faction.IsLocked)
                {
                    // Found a faction which selectable (not locked), but the player does not select it, so determine that the player is selecting wrong faction
                    return false;
                }
            }

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
