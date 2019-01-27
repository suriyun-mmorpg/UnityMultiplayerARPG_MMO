using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterCreate : UICharacterCreate
    {
        protected override void OnClickCreate()
        {
            string characterName = inputCharacterName.text.Trim();
            MMOClientInstance.Singleton.RequestCreateCharacter(characterName, CreatingPlayerCharacterData.DataId, CreatingPlayerCharacterData.EntityId, OnRequestedCreateCharacter);
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
