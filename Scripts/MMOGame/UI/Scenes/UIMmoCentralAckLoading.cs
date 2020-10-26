using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCentralAckLoading : MonoBehaviour
    {
        public static UIMmoCentralAckLoading Singleton { get; private set; }
        public GameObject rootObject;
        private float lastUpdateTime;

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
        }

        private void Update()
        {
            if (Time.unscaledTime - lastUpdateTime >= 0.5f)
            {
                UpdateUI();
                lastUpdateTime = Time.unscaledTime;
            }
        }

        private void UpdateUI()
        {
            if (rootObject != null)
                rootObject.SetActive(MMOClientInstance.Singleton.CentralNetworkManager.IsClientConnected && MMOClientInstance.Singleton.CentralNetworkManager.Client.RequestsCount > 0);
        }
    }
}
