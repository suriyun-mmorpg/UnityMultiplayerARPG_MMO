using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerUserHandlers : DefaultServerUserHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void MuteCharacterByName(string characterName, int minutes)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * minutes);
            IPlayerCharacterData playerCharacter;
            if (TryGetPlayerCharacterByName(characterName, out playerCharacter))
                playerCharacter.UnmuteTime = time;
            DbServiceClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = time,
            }).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void UnmuteCharacterByName(string characterName)
        {
            IPlayerCharacterData playerCharacter;
            if (TryGetPlayerCharacterByName(characterName, out playerCharacter))
                playerCharacter.UnmuteTime = 0;
            DbServiceClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = 0,
            }).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public override void BanUserByCharacterName(string characterName, int days)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 60 * 24 * days);
            IPlayerCharacterData playerCharacter;
            long connectionId;
            if (TryGetPlayerCharacterByName(characterName, out playerCharacter) && TryGetConnectionId(playerCharacter.Id, out connectionId))
                Manager.ServerTransport.ServerDisconnect(connectionId);
            DbServiceClient.SetUserUnbanTimeByCharacterNameAsync(new SetUserUnbanTimeByCharacterNameReq()
            {
                CharacterName = characterName,
                UnbanTime = time,
            }).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
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
