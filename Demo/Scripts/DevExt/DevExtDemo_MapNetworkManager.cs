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
        protected void DevExtDemo_RegisterClientMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterClientMessages()");
        }

        [DevExtMethods("RegisterServerMessages")]
        protected void DevExtDemo_RegisterServerMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterServerMessages()");
        }

        [DevExtMethods("Init")]
        protected void DevExtDemo_Init()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.Init()");
        }

        [DevExtMethods("OnClientOnlineSceneLoaded")]
        protected void DevExtDemo_OnClientOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnClientOnlineSceneLoaded()");
        }

        [DevExtMethods("OnServerOnlineSceneLoaded")]
        protected void DevExtDemo_OnServerOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnServerOnlineSceneLoaded()");
        }
    }
}
