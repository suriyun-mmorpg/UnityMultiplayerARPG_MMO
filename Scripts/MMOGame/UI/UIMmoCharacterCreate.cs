using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UIMmoCharacterCreate : UICharacterCreate
    {
        protected override void OnClickCreate()
        {
            GameInstance gameInstance = GameInstance.Singleton;
            UICharacter selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
                Debug.LogWarning("Cannot create character, did not selected character class");
                return;
            }
            string characterName = inputCharacterName.text.Trim();
            MMOClientInstance.Singleton.RequestCreateCharacter(characterName, selectedUI.Data.DataId, selectedUI.Data.EntityId, OnRequestedCreateCharacter);
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
                            errorMessage = "User not logged in";
                            break;
                        case ResponseCreateCharacterMessage.Error.InvalidData:
                            errorMessage = "Invalid data";
                            break;
                        case ResponseCreateCharacterMessage.Error.TooShortCharacterName:
                            errorMessage = "Character name is too short";
                            break;
                        case ResponseCreateCharacterMessage.Error.TooLongCharacterName:
                            errorMessage = "Character name is too long";
                            break;
                        case ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted:
                            errorMessage = "Character name is already existed";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Create Characters", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Create Characters", "Connection timeout");
                    break;
                default:
                    if (eventOnCreateCharacter != null)
                        eventOnCreateCharacter.Invoke();
                    break;
            }
        }
    }
}
