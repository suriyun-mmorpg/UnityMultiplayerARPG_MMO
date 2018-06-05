using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Insthync.MMOG
{
    public class MMOClientInstance : MonoBehaviour
    {
        public static MMOClientInstance Singleton { get; protected set; }

        #region Client components
        [Header("Client Components")]
        public CentralNetworkManager centralNetworkManager;
        public MapSpawnNetworkManager mapSpawnNetworkManager;
        public MapNetworkManager mapNetworkManager;
        #endregion

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Singleton = this;
        }

        #region Client functions
        public void StartCentralClient()
        {
            centralNetworkManager.StartClient();
        }

        public void StartMapSpawnClient()
        {
            mapSpawnNetworkManager.StartClient();
        }

        public void StartMapClient()
        {
            mapNetworkManager.StartClient();
        }
        #endregion
    }
}
