using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE)
        private void Awake()
        {
            DataManager = GetComponentInChildren<ICentralServerDataManager>();
            if (DataManager == null)
            {
                Debug.Log("`DataManager` not setup yet, Use default one...");
                DataManager = new DefaultCentralServerDataManager();
            }
        }
#endif
    }
}
