namespace MultiplayerARPG.MMO
{
    public struct SimpleUserCharacterData
    {
        public string userId;
        public string characterId;
        public string characterName;
        public SimpleUserCharacterData(string userId, string characterId, string characterName)
        {
            this.userId = userId;
            this.characterId = characterId;
            this.characterName = characterName;
        }
    }
}