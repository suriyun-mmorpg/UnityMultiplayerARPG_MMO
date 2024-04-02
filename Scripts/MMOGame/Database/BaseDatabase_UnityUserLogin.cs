using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(DatabaseUserLoginSetup))]
    public partial class BaseDatabase
    {
#if NET || NETCOREAPP || ((UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        internal IDatabaseUserLogin UserLoginManager
        {
            get { return _userLoginManager; }
            set { _userLoginManager = value; }
        }
#endif
    }
}
