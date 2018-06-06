using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class UIMmoLogin : UIBase
    {
        public InputField textUsername;
        public InputField textPassword;

        public string Username { get { return textUsername == null ? string.Empty : textUsername.text; } }
        public string Password { get { return textPassword == null ? string.Empty : textPassword.text; } }

        public void OnClickLogin()
        {
            var uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(Username))
            {
                uiSceneGlobal.ShowMessageDialog("Cannot login", "Username is empty");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                uiSceneGlobal.ShowMessageDialog("Cannot login", "Password is empty");
                return;
            }

            MMOClientInstance.Singleton.RequestUserLogin(Username, Password, OnLogin);
        }

        public void OnLogin(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseUserLoginMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseUserLoginMessage.Error.AlreadyLogin:
                            errorMessage = "User already loggedin";
                            break;
                        case ResponseUserLoginMessage.Error.InvalidUsernameOrPassword:
                            errorMessage = "Invalid username or password";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Login", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Login", "Connection timeout");
                    break;
                default:
                    Hide();
                    break;
            }
        }
    }
}
