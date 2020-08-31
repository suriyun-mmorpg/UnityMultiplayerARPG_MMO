using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class UIMmoLogin : UIBase
    {
        public InputField textUsername;
        public InputField textPassword;
        [Tooltip("If this is turned on, it will store both username to player prefs")]
        public Toggle toggleRememberUsername;
        [Tooltip("If this is turned on, it will store both username and password to player prefs. And login automatically when start game")]
        public Toggle toggleAutoLogin;
        public string keyUsername = "_USERNAME_";
        public string keyPassword = "_PASSWORD_";
        public UnityEvent onLoginSuccess;
        public UnityEvent onLoginFail;

        private bool logginIn;
        public bool LoggingIn
        {
            get { return logginIn; }
            set
            {
                logginIn = value;
                if (textUsername != null)
                    textUsername.interactable = !logginIn;
                if (textPassword != null)
                    textPassword.interactable = !logginIn;
            }
        }

        public string Username { 
            get { return textUsername == null ? string.Empty : textUsername.text; } 
            set { if (textUsername != null) textUsername.text = value; }
        }
        public string Password { 
            get { return textPassword == null ? string.Empty : textPassword.text; }
            set { if (textPassword != null) textPassword.text = value; }
        }

        private void Start()
        {
            string username = PlayerPrefs.GetString(keyUsername, string.Empty);
            string password = PlayerPrefs.GetString(keyPassword, string.Empty);
            if (!string.IsNullOrEmpty(username))
                Username = username;
            if (!string.IsNullOrEmpty(password))
                Password = password;
            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                OnClickLogin();
        }

        public void OnClickLogin()
        {
            // Don't allow to spam login button
            if (LoggingIn)
                return;

            // Clear stored username and password
            PlayerPrefs.SetString(keyUsername, string.Empty);
            PlayerPrefs.SetString(keyPassword, string.Empty);
            PlayerPrefs.Save();

            UISceneGlobal uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(Username))
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_IS_EMPTY.ToString()));
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_PASSWORD_IS_EMPTY.ToString()));
                return;
            }

            if ((toggleRememberUsername != null && toggleRememberUsername.isOn) ||
                (toggleAutoLogin != null && toggleAutoLogin.isOn))
            {
                // Remember username
                PlayerPrefs.SetString(keyUsername, Username);
                PlayerPrefs.Save();
            }

            LoggingIn = true;
            MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLogin);
        }

        public void OnLogin(AckResponseCode responseCode, BaseAckMessage message)
        {
            LoggingIn = false;
            string storingUsername = string.Empty;
            string storingPassword = string.Empty;
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            ResponseUserLoginMessage castedMessage = message as ResponseUserLoginMessage;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseUserLoginMessage.Error.AlreadyLogin:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_ALREADY_LOGGED_IN.ToString());
                            break;
                        case ResponseUserLoginMessage.Error.InvalidUsernameOrPassword:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    if (onLoginFail != null)
                        onLoginFail.Invoke();
                    break;
                default:
                    if (toggleAutoLogin != null && toggleAutoLogin.isOn)
                    {
                        // Store password
                        PlayerPrefs.SetString(keyPassword, storingPassword);
                        PlayerPrefs.Save();
                    }
                    if (onLoginSuccess != null)
                        onLoginSuccess.Invoke();
                    break;
            }
        }
    }
}
