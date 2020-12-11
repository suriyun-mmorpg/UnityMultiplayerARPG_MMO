using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using LiteNetLib.Utils;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public class UIMmoRegister : UIBase
    {
        public InputField textUsername;
        public InputField textPassword;
        public InputField textConfirmPassword;
        public UnityEvent onRegisterSuccess;
        public UnityEvent onRegisterFail;

        private bool registering;
        public bool Registering
        {
            get { return registering; }
            set
            {
                registering = value;
                if (textUsername != null)
                    textUsername.interactable = !registering;
                if (textPassword != null)
                    textPassword.interactable = !registering;
                if (textConfirmPassword != null)
                    textConfirmPassword.interactable = !registering;
            }
        }

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
            // Don't allow to spam register button
            if (Registering)
                return;

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

            if (!ValidatePassword())
            {
                uiSceneGlobal.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_CONFIRM_PASSWORD.ToString()));
                return;
            }

            Registering = true;
            MMOClientInstance.Singleton.RequestUserRegister(Username, Password, OnRegister);
        }

        public async UniTaskVoid OnRegister(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserRegisterMessage response)
        {
            await UniTask.Yield();
            Registering = false;
            if (responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                if (onRegisterFail != null)
                    onRegisterFail.Invoke();
                return;
            }
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (response.error)
                    {
                        case ResponseUserRegisterMessage.Error.TooShortUsername:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_TOO_SHORT.ToString());
                            break;
                        case ResponseUserRegisterMessage.Error.TooLongUsername:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_TOO_LONG.ToString());
                            break;
                        case ResponseUserRegisterMessage.Error.TooShortPassword:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_PASSWORD_TOO_SHORT.ToString());
                            break;
                        case ResponseUserRegisterMessage.Error.UsernameAlreadyExisted:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_USERNAME_EXISTED.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    if (onRegisterFail != null)
                        onRegisterFail.Invoke();
                    break;
                default:
                    if (onRegisterSuccess != null)
                        onRegisterSuccess.Invoke();
                    break;
            }
        }
    }
}
