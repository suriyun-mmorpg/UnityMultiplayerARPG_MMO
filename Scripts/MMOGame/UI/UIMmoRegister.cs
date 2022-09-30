using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using LiteNetLibManager;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    public partial class UIMmoRegister : UIBase
    {
        [System.Obsolete("Deprecated, use `uiTextUsername` instead.")]
        [HideInInspector]
        public InputField textUsername;
        [System.Obsolete("Deprecated, use `uiTextPassword` instead.")]
        [HideInInspector]
        public InputField textPassword;
        [System.Obsolete("Deprecated, use `textConfirmPassword` instead.")]
        [HideInInspector]
        public InputField textConfirmPassword;

        public InputFieldWrapper uiTextUsername;
        public InputFieldWrapper uiTextPassword;
        public InputFieldWrapper uiTextConfirmPassword;
        public InputFieldWrapper uiTextEmail;

        public UnityEvent onRegisterSuccess;
        public UnityEvent onRegisterFail;

        private bool registering;
        public bool Registering
        {
            get { return registering; }
            set
            {
                registering = value;
                if (uiTextUsername != null)
                    uiTextUsername.interactable = !registering;
                if (uiTextPassword != null)
                    uiTextPassword.interactable = !registering;
                if (uiTextConfirmPassword != null)
                    uiTextConfirmPassword.interactable = !registering;
                if (uiTextEmail != null)
                    uiTextEmail.interactable = !registering;
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
        public string ConfirmPassword
        {
            get { return uiTextConfirmPassword == null ? string.Empty : uiTextConfirmPassword.text; }
            set { if (uiTextConfirmPassword != null) uiTextConfirmPassword.text = value; }
        }
        public string Email
        {
            get { return uiTextEmail == null ? string.Empty : uiTextEmail.text; }
            set { if (uiTextEmail != null) uiTextEmail.text = value; }
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
            if (textConfirmPassword != null)
            {
                hasChanges = true;
                wrapper = textConfirmPassword.gameObject.GetOrAddComponent<InputFieldWrapper>();
                wrapper.unityInputField = textConfirmPassword;
                uiTextConfirmPassword = wrapper;
                textConfirmPassword = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
        }

        public bool ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
                return false;
            if (uiTextConfirmPassword != null && !Password.Equals(ConfirmPassword))
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
            MMOClientInstance.Singleton.RequestUserRegister(Username, Password, Email, OnRegister);
        }

        public void OnRegister(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserRegisterMessage response)
        {
            Registering = false;
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                if (onRegisterFail != null)
                    onRegisterFail.Invoke();
                return;
            }
            if (onRegisterSuccess != null)
                onRegisterSuccess.Invoke();
        }
    }
}
