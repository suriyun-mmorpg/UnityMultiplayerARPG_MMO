namespace MultiplayerARPG.MMO
{
    public struct SimpleUserCharacterData
    {
        public string userId;
        public string characterName;
        public SimpleUserCharacterData(string userId, string characterName)
        {
            this.userId = userId;
            this.characterName = characterName;
        }
    }
}