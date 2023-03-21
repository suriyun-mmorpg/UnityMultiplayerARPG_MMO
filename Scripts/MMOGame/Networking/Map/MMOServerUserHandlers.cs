using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerUserHandlers : DefaultServerUserHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void MuteCharacterByName(string characterName, int minutes)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * minutes);
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = time;
            DbServiceClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = time,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void UnmuteCharacterByName(string characterName)
        {
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = 0;
            DbServiceClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = 0,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void BanUserByCharacterName(string characterName, int days)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 60 * 24 * days);
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter) && TryGetConnectionId(playerCharacter.Id, out long connectionId))
                Manager.ServerTransport.ServerDisconnect(connectionId);
            DbServiceClient.SetUserUnbanTimeByCharacterNameAsync(new SetUserUnbanTimeByCharacterNameReq()
            {
                CharacterName = characterName,
                UnbanTime = time,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public override void UnbanUserByCharacterName(string characterName)
        {
            DbServiceClient.SetUserUnbanTimeByCharacterNameAsync(new SetUserUnbanTimeByCharacterNameReq()
            {
                CharacterName = characterName,
                UnbanTime = 0,
            }).Forget();
        }
#endif
    }
}
