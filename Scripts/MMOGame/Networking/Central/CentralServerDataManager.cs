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

        public bool CanCreateCharacter(int dataId, int entityId, int factionId)
        {
            return GameInstance.PlayerCharacters.ContainsKey(dataId) && GameInstance.PlayerCharacterEntities.ContainsKey(entityId) && (GameInstance.Factions.Count <= 0 || (GameInstance.Factions.ContainsKey(factionId) && !GameInstance.Factions[factionId].IsLocked));
        }

        public void SetNewPlayerCharacterData(PlayerCharacterData playerCharacterData, string characterName, int dataId, int entityId, int factionId)
        {
            playerCharacterData.SetNewPlayerCharacterData(characterName, dataId, entityId, factionId);
        }
    }
}
