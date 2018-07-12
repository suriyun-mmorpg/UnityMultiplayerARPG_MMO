using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCentralAckLoading : MonoBehaviour
    {
        public static UIMmoCentralAckLoading Singleton { get; private set; }
        public GameObject rootObject;

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            Singleton = this;

            if (rootObject != null)
                rootObject.SetActive(false);

            InvokeRepeating("UpdateUI", 0.5f, 0.5f);
        }

        void UpdateUI()
        {
            if (rootObject != null)
                rootObject.SetActive(MMOClientInstance.Singleton.CentralNetworkManager.IsClientConnected && MMOClientInstance.Singleton.CentralNetworkManager.Client.AckCallbacksCount > 0);
        }
    }
}
