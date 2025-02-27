using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerUserHandlers : DefaultServerUserHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }

        public CentralNetworkManager CentralNetworkManager
        {
            get { return MMOServerInstance.Singleton.CentralNetworkManager; }
        }

        public MapNetworkManager MapNetworkManager
        {
            get { return BaseGameNetworkManager.Singleton as MapNetworkManager; }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override void MuteCharacterByName(string characterName, int minutes)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * minutes);
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = time;
            DatabaseClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = time,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override void UnmuteCharacterByName(string characterName)
        {
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter))
                playerCharacter.UnmuteTime = 0;
            DatabaseClient.SetCharacterUnmuteTimeByNameAsync(new SetCharacterUnmuteTimeByNameReq()
            {
                CharacterName = characterName,
                UnmuteTime = 0,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override void BanUserByCharacterName(string characterName, int days)
        {
            long time = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (60 * 60 * 24 * days);
            if (TryGetPlayerCharacterByName(characterName, out IPlayerCharacterData playerCharacter) && TryGetConnectionId(playerCharacter.Id, out long connectionId))
                MapNetworkManager.ServerTransport.ServerDisconnect(connectionId);
            DatabaseClient.SetUserUnbanTimeByCharacterNameAsync(new SetUserUnbanTimeByCharacterNameReq()
            {
                CharacterName = characterName,
                UnbanTime = time,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override void UnbanUserByCharacterName(string characterName)
        {
            DatabaseClient.SetUserUnbanTimeByCharacterNameAsync(new SetUserUnbanTimeByCharacterNameReq()
            {
                CharacterName = characterName,
                UnbanTime = 0,
            }).Forget();
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override async void ChangeUserGold(string userId, int gold)
        {
            if (!TryGetPlayerCharacterByUserId(userId, out IPlayerCharacterData playerCharacter))
                return;
            DatabaseApiResult<GoldResp> resp = await DatabaseClient.ChangeGoldAsync(new ChangeGoldReq()
            {
                UserId = userId,
                ChangeAmount = gold,
            });
            if (resp.IsError)
                return;
            playerCharacter.UserGold = playerCharacter.UserGold.Increase(gold);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override async void ChangeUserCash(string userId, int cash)
        {
            if (!TryGetPlayerCharacterByUserId(userId, out IPlayerCharacterData playerCharacter))
                return;
            DatabaseApiResult<CashResp> resp = await DatabaseClient.ChangeCashAsync(new ChangeCashReq()
            {
                UserId = userId,
                ChangeAmount = cash,
            });
            if (resp.IsError)
                return;
            playerCharacter.UserCash = playerCharacter.UserCash.Increase(cash);
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public override async UniTask<UITextKeys> ValidateCharacterName(string characterName)
        {
            ProfanityDetectResult profanityDetectResult = await MapNetworkManager.ChatProfanityDetector.Proceed(characterName);
            if (profanityDetectResult.shouldMutePlayer || profanityDetectResult.shouldKickPlayer || !string.Equals(profanityDetectResult.message, characterName))
            {
                return UITextKeys.UI_ERROR_INVALID_CHARACTER_NAME;
            }
            if (!NameExtensions.IsValidCharacterName(characterName))
            {
                return UITextKeys.UI_ERROR_INVALID_CHARACTER_NAME;
            }
            if (characterName.Length < CentralNetworkManager.minCharacterNameLength)
            {
                return UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_SHORT;
            }
            if (characterName.Length > CentralNetworkManager.maxCharacterNameLength)
            {
                return UITextKeys.UI_ERROR_CHARACTER_NAME_TOO_LONG;
            }
            DatabaseApiResult<FindCharacterNameResp> findCharacterNameResp = await DatabaseClient.FindCharacterNameAsync(new FindCharacterNameReq()
            {
                CharacterName = characterName
            });
            if (!findCharacterNameResp.IsSuccess)
            {
                return UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR;
            }
            if (findCharacterNameResp.Response.FoundAmount > 0)
            {
                return UITextKeys.UI_ERROR_CHARACTER_NAME_EXISTED;
            }
            return UITextKeys.NONE;
        }
#endif
    }
}
