using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterList : UICharacterList
    {
        protected override void LoadCharacters()
        {
            MMOClientInstance.Singleton.RequestCharacters(OnRequestedCharacters);
        }

        private void OnRequestedCharacters(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseCharactersMessage castedMessage = (ResponseCharactersMessage)message;
            CacheCharacterSelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModelById.Clear();
            // Remove all cached data
            PlayerCharacterDataById.Clear();

            List<PlayerCharacterData> selectableCharacters = new List<PlayerCharacterData>();

            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseCharactersMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_NOT_LOGGED_IN.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    selectableCharacters = castedMessage.characters;
                    break;
            }

            // Show list of created characters
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null || !GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                    selectableCharacters.RemoveAt(i);
            }
            selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
            CacheCharacterList.Generate(selectableCharacters, (index, characterData, ui) =>
            {
                // Cache player character to dictionary, we will use it later
                PlayerCharacterDataById[characterData.Id] = characterData;
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = characterData;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                if (characterModel != null)
                {
                    CharacterModelById[characterData.Id] = characterModel;
                    characterModel.gameObject.SetActive(false);
                    characterModel.SetEquipWeapons(characterData.EquipWeapons);
                    characterModel.SetEquipItems(characterData.EquipItems);
                    CacheCharacterSelectionManager.Add(uiCharacter);
                }
            });
        }

        protected override void OnClickStart()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_NO_CHOSEN_CHARACTER_TO_START.ToString()));
                Debug.LogWarning("Cannot start game, No chosen character");
                return;
            }
            // Load gameplay scene, we're going to manage maps in gameplay scene later
            // So we can add gameplay UI just once in gameplay scene
            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestSelectCharacter(playerCharacter.Id, OnRequestedSelectCharacter);
        }

        private void OnRequestedSelectCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseSelectCharacterMessage castedMessage = (ResponseSelectCharacterMessage)message;
            
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseSelectCharacterMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_NOT_LOGGED_IN.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.AlreadySelectCharacter:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_ALREADY_SELECT_CHARACTER.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.InvalidCharacterData:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_INVALID_CHARACTER_DATA.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.MapNotReady:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_MAP_SERVER_NOT_READY.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    MMOClientInstance.Singleton.StartMapClient(castedMessage.sceneName, castedMessage.networkAddress, castedMessage.networkPort, castedMessage.connectKey);
                    break;
            }
        }

        protected override void OnClickDelete()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_NO_CHOSEN_CHARACTER_TO_DELETE.ToString()));
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestDeleteCharacter(playerCharacter.Id, OnRequestedDeleteCharacter);
        }

        private void OnRequestedDeleteCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseDeleteCharacterMessage castedMessage = (ResponseDeleteCharacterMessage)message;
            
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseDeleteCharacterMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UILocaleKeys.UI_NOT_LOGGED_IN.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UILocaleKeys.UI_ERROR.ToString()), LanguageManager.GetText(UILocaleKeys.UI_CONNECTION_TIMEOUT.ToString()));
                    break;
                default:
                    // Reload characters
                    LoadCharacters();
                    break;
            }
        }
    }
}
