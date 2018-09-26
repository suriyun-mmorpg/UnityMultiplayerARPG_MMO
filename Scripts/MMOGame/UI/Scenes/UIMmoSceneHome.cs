using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoSceneHome : UIHistory
    {
        public UIMmoLogin uiLogin;
        public UIMmoCharacterList uiCharacterList;
        public UIMmoCharacterCreate uiCharacterCreate;
        public UnityEvent onValidateAccessTokenSuccess;

        private void OnEnable()
        {
            MMOClientInstance.Singleton.onCentralClientConnected += OnCentralServerConnected;
            MMOClientInstance.Singleton.onCentralClientDisconnected += OnCentralServerDisconnected;
            if (MMOClientInstance.Singleton.IsConnectedToCentralServer())
                OnCentralServerConnected();
            else if (!string.IsNullOrEmpty(MMOClientInstance.SelectedCentralAddress) && MMOClientInstance.SelectedCentralPort > 0)
                MMOClientInstance.Singleton.StartCentralClient(MMOClientInstance.SelectedCentralAddress, MMOClientInstance.SelectedCentralPort);
        }

        private void OnDisable()
        {
            MMOClientInstance.Singleton.onCentralClientConnected -= OnCentralServerConnected;
            MMOClientInstance.Singleton.onCentralClientDisconnected -= OnCentralServerDisconnected;
        }

        public void OnCentralServerConnected()
        {
            ClearHistory();
            Next(uiLogin);
            if (!string.IsNullOrEmpty(MMOClientInstance.UserId) && !string.IsNullOrEmpty(MMOClientInstance.AccessToken))
                MMOClientInstance.Singleton.RequestValidateAccessToken(MMOClientInstance.UserId, MMOClientInstance.AccessToken, OnValidateAccessToken);
        }

        public void OnCentralServerDisconnected(DisconnectInfo disconnectInfo)
        {
            UISceneGlobal.Singleton.ShowDisconnectDialog(disconnectInfo);
            ClearHistory();
        }

        public void OnClickLogout()
        {
            MMOClientInstance.Singleton.RequestUserLogout(OnUserLogout);
        }

        public void OnClickDisconnect()
        {
            MMOClientInstance.Singleton.ClearClientData();
            MMOClientInstance.Singleton.StopCentralClient();
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
