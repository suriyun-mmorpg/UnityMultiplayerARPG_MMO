using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    public partial class UIMmoLogin : UIBase
    {
        [System.Obsolete("Deprecated, use `uiTextUsername` instead.")]
        [HideInInspector]
        public InputField textUsername;
        [System.Obsolete("Deprecated, use `uiTextPassword` instead.")]
        [HideInInspector]
        public InputField textPassword;

        public InputFieldWrapper uiTextUsername;
        public InputFieldWrapper uiTextPassword;
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
                if (uiTextUsername != null)
                    uiTextUsername.interactable = !logginIn;
                if (uiTextPassword != null)
                    uiTextPassword.interactable = !logginIn;
            }
        }

        public string Username
        {
            get { return uiTextUsername == null ? string.Empty : uiTextUsername.text; }
            set { if (uiTextUsername != null) uiTextUsername.text = value; }
        }
        public string Password
        {
            get { return uiTextPassword == null ? string.Empty : uiTextPassword.text; }
            set { if (uiTextPassword != null) uiTextPassword.text = value; }
        }

        protected override void Awake()
        {
            base.Awake();
            MigrateInputComponent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (MigrateInputComponent())
                EditorUtility.SetDirty(this);
        }
#endif

        public bool MigrateInputComponent()
        {
            bool hasChanges = false;
            InputFieldWrapper wrapper;
#pragma warning disable CS0618 // Type or member is obsolete
            if (textUsername != null)
            {
                hasChanges = true;
                wrapper = textUsername.gameObject.GetOrAddComponent<InputFieldWrapper>();
                wrapper.unityInputField = textUsername;
                uiTextUsername = wrapper;
                textUsername = null;
            }
            if (textPassword != null)
            {
                hasChanges = true;
                wrapper = textPassword.gameObject.GetOrAddComponent<InputFieldWrapper>();
                wrapper.unityInputField = textPassword;
                uiTextPassword = wrapper;
                textPassword = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
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

        public void OnLogin(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserLoginMessage response)
        {
            LoggingIn = false;
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            if (toggleAutoLogin != null && toggleAutoLogin.isOn)
            {
                // Store password
                PlayerPrefs.SetString(keyUsername, Username);
                PlayerPrefs.SetString(keyPassword, Password);
                PlayerPrefs.Save();
            }
            if (onLoginSuccess != null)
                onLoginSuccess.Invoke();
        }
    }
}
