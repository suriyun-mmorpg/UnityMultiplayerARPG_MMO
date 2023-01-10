using UnityEngine;
using UnityEngine.Events;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Net.Sockets;

namespace MultiplayerARPG.MMO
{
    public class UIMmoSceneHome : UIHistory
    {
        public UIMmoLogin uiLogin;
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
            if (!string.IsNullOrEmpty(GameInstance.UserId) && !string.IsNullOrEmpty(GameInstance.UserToken))
                MMOClientInstance.Singleton.RequestValidateAccessToken(GameInstance.UserId, GameInstance.UserToken, OnValidateAccessToken);
        }

        public void OnCentralServerDisconnected(DisconnectReason reason, SocketError socketError, UITextKeys message)
        {
            UISceneGlobal.Singleton.ShowDisconnectDialog(reason, socketError, message);
            ClearHistory();
        }

        public void OnClickLogout()
        {
            MMOClientInstance.Singleton.RequestUserLogout(OnUserLogout);
        }

        public void OnClickDisconnect()
        {
            MMOClientInstance.Singleton.StopCentralClient();
        }

        public void OnClickExit()
        {
            Application.Quit();
        }

        private void OnUserLogout(ResponseHandlerData responseHandler, AckResponseCode responseCode, INetSerializable response)
        {
            if (responseCode == AckResponseCode.Success)
            {
                ClearHistory();
                Next(uiLogin);
            }
        }

        private void OnValidateAccessToken(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseValidateAccessTokenMessage response)
        {
            if (responseCode == AckResponseCode.Success)
            {
                if (onValidateAccessTokenSuccess != null)
                    onValidateAccessTokenSuccess.Invoke();
            }
        }
    }
}
