using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    public class UIMmoRegister : UIBase
    {
        public InputField textUsername;
        public InputField textPassword;
        public InputField textConfirmPassword;

        public string Username { get { return textUsername == null ? string.Empty : textUsername.text; } }
        public string Password { get { return textPassword == null ? string.Empty : textPassword.text; } }
        public string ConfirmPassword { get { return textConfirmPassword == null ? string.Empty : textConfirmPassword.text; } }

        public bool ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
                return false;
            if (textConfirmPassword != null && !Password.Equals(ConfirmPassword))
                return false;
            return true;
        }

        public void OnClickRegister()
        {
            var uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(Username))
            {
                uiSceneGlobal.ShowMessageDialog("Cannot register", "Username is empty");
                return;
            }

            if (string.IsNullOrEmpty(Password))
            {
                uiSceneGlobal.ShowMessageDialog("Cannot register", "Password is empty");
                return;
            }

            if (!ValidatePassword())
            {
                uiSceneGlobal.ShowMessageDialog("Cannot register", "Invalid confirm password");
                return;
            }

            MMOClientInstance.Singleton.centralNetworkManager.RequestUserRegister(Username, Password, OnRegister);
        }

        public void OnRegister(AckResponseCode responseCode, BaseAckMessage message)
        {

        }
    }
}
