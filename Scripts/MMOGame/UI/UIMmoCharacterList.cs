using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterList : UICharacterList
    {
        protected override void LoadCharacters()
        {
            eventOnNotAbleToCreateCharacter.Invoke();
            MMOClientInstance.Singleton.RequestCharacters(OnRequestedCharacters);
        }

        private void OnRequestedCharacters(ResponseCharactersMessage message)
        {
            // Clear character list
            CacheCharacterSelectionManager.Clear();
            CacheCharacterList.HideAll();
            // Unable buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModelById.Clear();
            // Remove all cached data
            PlayerCharacterDataById.Clear();
            // Don't make character list if timeout
            if (message.responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            // Prepare character list
            List<PlayerCharacterData> selectableCharacters = new List<PlayerCharacterData>();
            switch (message.responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (message.error)
                    {
                        case ResponseCharactersMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_NOT_LOGGED_IN.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    selectableCharacters = message.characters;
                    break;
            }

            // Show list of created characters
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null ||
                    !GameInstance.PlayerCharacterEntities.ContainsKey(selectableCharacter.EntityId) ||
                    !GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                {
                    // If invalid entity id or data id, remove from selectable character list
                    selectableCharacters.RemoveAt(i);
                }
            }

            if (GameInstance.Singleton.maxCharacterSaves > 0 &&
                selectableCharacters.Count >= GameInstance.Singleton.maxCharacterSaves)
                eventOnNotAbleToCreateCharacter.Invoke();
            else
                eventOnAbleToCreateCharacter.Invoke();

            // Clear selected character data, will select first in list if available
            selectedPlayerCharacterData = null;

            // Generate list entry by saved characters
            if (selectableCharacters.Count > 0)
            {
                selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
                CacheCharacterList.Generate(selectableCharacters, (index, characterData, ui) =>
                {
                    // Cache player character to dictionary, we will use it later
                    PlayerCharacterDataById[characterData.Id] = characterData;
                    // Setup UIs
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.Data = characterData;
                    // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                    BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                    if (characterModel != null)
                    {
                        CharacterModelById[characterData.Id] = characterModel;
                        characterModel.SetEquipWeapons(characterData.EquipWeapons);
                        characterModel.SetEquipItems(characterData.EquipItems);
                        characterModel.gameObject.SetActive(false);
                        CacheCharacterSelectionManager.Add(uiCharacter);
                    }
                });
            }
            else
            {
                eventOnNoCharacter.Invoke();
            }
        }

        protected override void OnSelectCharacter(IPlayerCharacterData playerCharacterData)
        {
            if (buttonStart)
                buttonStart.gameObject.SetActive(true);
            if (buttonDelete)
                buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            // Load selected character and also set selected player character data
            PlayerCharacterDataById.TryGetValue(playerCharacterData.Id, out selectedPlayerCharacterData);
            // Show selected character model
            CharacterModelById.TryGetValue(playerCharacterData.Id, out selectedModel);
            if (SelectedModel != null)
                SelectedModel.gameObject.SetActive(true);
        }

        public override void OnClickStart()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_START.ToString()));
                Debug.LogWarning("Cannot start game, No chosen character");
                return;
            }
            // Load gameplay scene, we're going to manage maps in gameplay scene later
            // So we can add gameplay UI just once in gameplay scene
            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestSelectCharacter(playerCharacter.Id, OnRequestedSelectCharacter);
        }

        private void OnRequestedSelectCharacter(ResponseSelectCharacterMessage message)
        {
            if (message.responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            switch (message.responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (message.error)
                    {
                        case ResponseSelectCharacterMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_NOT_LOGGED_IN.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.AlreadySelectCharacter:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_ALREADY_SELECT_CHARACTER.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.InvalidCharacterData:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_CHARACTER_DATA.ToString());
                            break;
                        case ResponseSelectCharacterMessage.Error.MapNotReady:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_MAP_SERVER_NOT_READY.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    MMOClientInstance.Singleton.StartMapClient(message.sceneName, message.networkAddress, message.networkPort);
                    break;
            }
        }

        public override void OnClickDelete()
        {
            UICharacter selectedUI = CacheCharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE.ToString()));
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestDeleteCharacter(playerCharacter.Id, OnRequestedDeleteCharacter);
        }

        private void OnRequestedDeleteCharacter(ResponseDeleteCharacterMessage message)
        {
            if (message.responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                return;
            }
            switch (message.responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (message.error)
                    {
                        case ResponseDeleteCharacterMessage.Error.NotLoggedin:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_NOT_LOGGED_IN.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
                    break;
                default:
                    // Reload characters
                    LoadCharacters();
                    break;
            }
        }
    }
}
