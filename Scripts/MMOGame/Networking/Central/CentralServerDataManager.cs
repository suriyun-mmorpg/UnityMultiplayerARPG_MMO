using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public class CentralServerDataManager : ICentralServerDataManager
    {
        public string GenerateCharacterId()
        {
            return GenericUtils.GetUniqueId();
        }

        public string GenerateMapSpawnRequestId()
        {
            return GenericUtils.GetUniqueId();
        }

        public bool CanCreateCharacter(int dataId, int entityId, int factionId, IList<CharacterDataBoolean> publicBools, IList<CharacterDataInt32> publicInts, IList<CharacterDataFloat32> publicFloats)
        {
            return GameInstance.PlayerCharacters.ContainsKey(dataId) && GameInstance.PlayerCharacterEntities.ContainsKey(entityId) && (GameInstance.Factions.Count <= 0 || (GameInstance.Factions.ContainsKey(factionId) && !GameInstance.Factions[factionId].IsLocked));
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
