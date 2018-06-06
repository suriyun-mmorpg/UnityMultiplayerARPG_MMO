using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class UIMmoSceneHome : UIHistory
    {
        public UIMmoLogin uiLogin;
        public UIMmoCharacterList uiCharacterList;
        public UIMmoCharacterCreate uiCharacterCreate;

        private void OnEnable()
        {
            MMOClientInstance.Singleton.onClientConnected += OnCentralServerConnected;
            MMOClientInstance.Singleton.onClientDisconnected += OnCentralServerDisconnected;
            if (MMOClientInstance.Singleton.IsConnectedToCentralServer())
            {
                ClearHistory();
                Next(uiLogin);
            }
        }

        private void OnDisable()
        {
            MMOClientInstance.Singleton.onClientConnected -= OnCentralServerConnected;
            MMOClientInstance.Singleton.onClientDisconnected -= OnCentralServerDisconnected;
        }

        public void OnCentralServerConnected(NetPeer netPeer)
        {
            ClearHistory();
            Next(uiLogin);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            ClearHistory();
        }

        public void OnUserLogout(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            ClearHistory();
            Next(uiLogin);
        }

        public void OnClickLogout()
        {
            MMOClientInstance.Singleton.RequestUserLogout(OnUserLogout);
        }

        public void OnClickExit()
        {
            Application.Quit();
        }
    }
}
