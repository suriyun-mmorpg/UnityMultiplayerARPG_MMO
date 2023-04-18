using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [DisallowMultipleComponent]
    public class DatabaseUserLoginSetup : MonoBehaviour
    {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public static readonly string LogTag = nameof(DatabaseUserLoginSetup);

        private class OldDatabaseUserLogin : IDatabaseUserLogin
        {
            public string GenerateNewId()
            {
                return GenericUtils.GetUniqueId();
            }

            public string GetHashedPassword(string password)
            {
                return password.PasswordHash();
            }

            public bool VerifyPassword(string password, string hashedPassword)
            {
                return password.PasswordVerify(hashedPassword);
            }
        }

        void Start()
        {
            BaseDatabase database = GetComponent<BaseDatabase>();
            if (database.UserLoginManager == null)
            {
                database.LogInformation(LogTag, "`UserLoginManager` not setup yet, Get it in children to setup...");
                database.UserLoginManager = GetComponentInChildren<IDatabaseUserLogin>();
            }
            if (database.UserLoginManager == null)
            {
                database.LogInformation(LogTag, "`UserLoginManager` not setup yet, Use default one...");
                database.UserLoginManager = new OldDatabaseUserLogin();
            }
        }
#endif
    }
}
