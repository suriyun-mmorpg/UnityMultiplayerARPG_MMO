using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        private void Awake()
        {
            DataManager = GetComponentInChildren<ICentralServerDataManager>();
            if (DataManager == null)
            {
                Debug.Log("`DataManager` not setup yet, Use default one...");
                DataManager = new DefaultCentralServerDataManager();
            }
        }
    }
}
