using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterCreate : UICharacterCreate
    {
        protected override void OnClickCreate()
        {
            PlayerCharacterData characterData = new PlayerCharacterData();
            characterData.Id = GenericUtils.GetUniqueId();
            characterData.SetNewPlayerCharacterData(inputCharacterName.text.Trim(), SelectedDataId, SelectedEntityId);
            MMOClientInstance.Singleton.RequestCreateCharacter(characterData, OnRequestedCreateCharacter);
        }

        private void OnRequestedCreateCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseCreateCharacterMessage castedMessage = (ResponseCreateCharacterMessage)message;

            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseCreateCharacterMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_NOT_LOGGED_IN.ToString());
                            break;
                        case ResponseCreateCharacterMessage.Error.InvalidData:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_INVALID_DATA.ToString());
                            break;
                        case ResponseCreateCharacterMessage.Error.TooShortCharacterName:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_CHARACTER_NAME_TOO_SHORT.ToString());
                            break;
                        case ResponseCreateCharacterMessage.Error.TooLongCharacterName:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_CHARACTER_NAME_TOO_LONG.ToString());
                            break;
                        case ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_CHARACTER_NAME_EXISTED.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    if (eventOnCreateCharacter != null)
                        eventOnCreateCharacter.Invoke();
                    break;
            }
        }
    }
}
