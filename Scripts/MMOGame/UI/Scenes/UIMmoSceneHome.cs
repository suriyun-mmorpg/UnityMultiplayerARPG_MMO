using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLib;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class UIMmoSceneHome : UIHistory
    {
        public UIMmoLogin uiLogin;
        public UIMmoCharacterList uiCharacterList;
        public UIMmoCharacterCreate uiCharacterCreate;
        public UnityEvent onValidateAccessTokenSuccess;

        private void OnEnable()
        {
            MMOClientInstance.Singleton.onClientConnected += OnCentralServerConnected;
            MMOClientInstance.Singleton.onClientDisconnected += OnCentralServerDisconnected;
            if (MMOClientInstance.Singleton.IsConnectedToCentralServer())
                OnCentralServerConnected(MMOClientInstance.Singleton.GetCentralClientPeer());
            else if (!string.IsNullOrEmpty(MMOClientInstance.UserId) && !string.IsNullOrEmpty(MMOClientInstance.AccessToken))
                MMOClientInstance.Singleton.StartCentralClient();
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
            if (!string.IsNullOrEmpty(MMOClientInstance.UserId) && !string.IsNullOrEmpty(MMOClientInstance.AccessToken))
                MMOClientInstance.Singleton.RequestValidateAccessToken(MMOClientInstance.UserId, MMOClientInstance.AccessToken, OnValidateAccessToken);
        }

        public void OnCentralServerDisconnected(NetPeer netPeer, DisconnectInfo disconnectInfo)
        {
            ClearHistory();
        }

        public void OnClickLogout()
        {
            MMOClientInstance.Singleton.RequestUserLogout(OnUserLogout);
        }

        public void OnClickExit()
        {
            Application.Quit();
        }

        private void OnUserLogout(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            ClearHistory();
            Next(uiLogin);
        }

        private void OnValidateAccessToken(AckResponseCode responseCode, BaseAckMessage messageData)
        {
            if (responseCode == AckResponseCode.Success)
            {
                if (onValidateAccessTokenSuccess != null)
                    onValidateAccessTokenSuccess.Invoke();
            }
        }
    }
}
