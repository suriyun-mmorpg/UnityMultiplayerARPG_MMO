using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        [Header("Demo Developer Extension")]
        public bool writeAddonLog;

        [DevExtMethods("RegisterClientMessages")]
        private void DevExtDemo_RegisterClientMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterClientMessages()");
        }

        [DevExtMethods("RegisterServerMessages")]
        private void DevExtDemo_RegisterServerMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterServerMessages()");
        }

        [DevExtMethods("Init")]
        private void DevExtDemo_Init()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.Init()");
        }

        [DevExtMethods("OnClientOnlineSceneLoaded")]
        private void DevExtDemo_OnClientOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnClientOnlineSceneLoaded()");
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        private void DevExtDemo_OnServerOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnServerOnlineSceneLoaded()");
        }
    }
}
