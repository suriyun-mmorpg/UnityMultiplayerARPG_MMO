using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

            MMOClientInstance.Singleton.loginClientNetworkManager.Login(Username, Password);
        }
    }
}
