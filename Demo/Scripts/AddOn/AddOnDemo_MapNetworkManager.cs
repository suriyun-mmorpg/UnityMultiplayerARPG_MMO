using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        [Header("Demo Addon")]
        public bool writeAddonLog;
        protected void AddOnDemo_RegisterClientMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterClientMessages()");
        }

        protected void AddOnDemo_RegisterServerMessages()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.RegisterServerMessages()");
        }

        protected void AddOnDemo_Init()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.Init()");
        }

        protected void AddOnDemo_OnClientOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnClientOnlineSceneLoaded()");
        }

        protected void AddOnDemo_OnServerOnlineSceneLoaded()
        {
            if (writeAddonLog) Debug.Log("[" + name + "] LanRpgNetworkManager.OnServerOnlineSceneLoaded()");
        }
    }
}
