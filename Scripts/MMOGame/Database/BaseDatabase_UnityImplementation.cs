using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class BaseDatabase : MonoBehaviour
    {
        private void Awake()
        {
            UserLoginManager = GetComponentInChildren<IDatabaseUserLogin>();
            if (UserLoginManager == null)
            {
                Debug.Log("`UserLoginManager` not setup yet, Use default one...");
                // TODO: Setup by files/environment settings
                UserLoginManager = new DefaultDatabaseUserLogin(new DefaultDatabaseUserLoginConfig()
                {
                    PasswordSaltPrefix = string.Empty,
                    PasswordSaltPostfix = string.Empty,
                });
            }
        }

        public void LogInformation(string tag, string msg)
        {
            Logging.Log(tag, msg);
        }

        public void LogWarning(string tag, string msg)
        {
            Logging.LogWarning(tag, msg);
        }

        public void LogError(string tag, string msg)
        {
            Logging.LogError(tag, msg);
        }

        public void LogException(string tag, System.Exception ex)
        {
            Logging.LogException(tag, ex);
        }
    }
}
