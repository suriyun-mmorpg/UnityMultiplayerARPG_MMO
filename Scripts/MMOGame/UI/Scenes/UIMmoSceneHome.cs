using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

namespace Insthync.MMOG
{
    public class UIMmoSceneHome : UIHistory
    {
        public UIMmoServerList uiServerList;
        public UIBase uiConnectedToServer;
        public UIMmoCharacterCreate uiCharacterCreate;
        public UIMmoCharacterList uiCharacterList;

        private void OnEnable()
        {
            MMOClientInstance.Singleton.onClientConnected += OnCentralServerConnected;
            MMOClientInstance.Singleton.onClientDisconnected += OnCentralServerDisconnected;
        }

        private void OnDisable()
        {
            MMOClientInstance.Singleton.onClientConnected -= OnCentralServerConnected;
            MMOClientInstance.Singleton.onClientDisconnected -= OnCentralServerDisconnected;
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            Next(uiConnectedToServer);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            ClearHistory();
        }

        public void OnClickExit()
        {
            Application.Quit();
        }
    }
}
