using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(DatabaseUserLoginSetup))]
    public partial class BaseDatabase
    {
        internal IDatabaseUserLogin UserLoginManager
        {
            get { return _userLoginManager; }
            set { _userLoginManager = value; }
        }
    }
}
