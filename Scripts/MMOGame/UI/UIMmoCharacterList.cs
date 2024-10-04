using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public class UIMmoCharacterList : UICharacterList
    {
        protected override void LoadCharacters()
        {
            if (buttonStart != null)
                buttonStart.gameObject.SetActive(false);
            if (buttonDelete != null)
                buttonDelete.gameObject.SetActive(false);
            eventOnNotAbleToCreateCharacter.Invoke();
            MMOClientInstance.Singleton.RequestCharacters(OnRequestedCharacters);
        }

        private void OnRequestedCharacters(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseCharactersMessage response)
        {
            // Clear character list
            CharacterSelectionManager.Clear();
            CharacterList.HideAll();
            // Remove all models
            characterModelContainer.RemoveChildren();
            _characterModelById.Clear();
            // Remove all cached data
            _playerCharacterDataById.Clear();
            // Proceed response
            List<PlayerCharacterData> selectableCharacters = new List<PlayerCharacterData>();
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
                return;
            // Success, so set selectable characters by response's data
            selectableCharacters = response.characters;
            // Show list of created characters
            for (int i = selectableCharacters.Count - 1; i >= 0; --i)
            {
                PlayerCharacterData selectableCharacter = selectableCharacters[i];
                if (selectableCharacter == null)
                {
                    // Null data
                    selectableCharacters.RemoveAt(i);
                    continue;
                }
                if (
#if !EXCLUDE_PREFAB_REFS
                    !GameInstance.PlayerCharacterEntities.ContainsKey(selectableCharacter.EntityId) && 
#endif
                    !GameInstance.AddressablePlayerCharacterEntities.ContainsKey(selectableCharacter.EntityId) &&
                    !GameInstance.PlayerCharacterEntityMetaDataList.ContainsKey(selectableCharacter.EntityId))
                {
                    // Invalid entity data
                    selectableCharacters.RemoveAt(i);
                    continue;
                }
                if (!GameInstance.PlayerCharacters.ContainsKey(selectableCharacter.DataId))
                {
                    // Invalid character data
                    selectableCharacters.RemoveAt(i);
                    continue;
                }
            }

            if (GameInstance.Singleton.maxCharacterSaves > 0 &&
                selectableCharacters.Count >= GameInstance.Singleton.maxCharacterSaves)
                eventOnNotAbleToCreateCharacter.Invoke();
            else
                eventOnAbleToCreateCharacter.Invoke();

            // Clear selected character data, will select first in list if available
            _selectedPlayerCharacterData = null;

            // Generate list entry by saved characters
            if (selectableCharacters.Count > 0)
            {
                selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
                CharacterList.Generate(selectableCharacters, (index, characterData, ui) =>
                {
                    // Cache player character to dictionary, we will use it later
                    _playerCharacterDataById[characterData.Id] = characterData;
                    // Setup UIs
                    UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                    uiCharacter.Data = characterData;
                    // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                    BaseCharacterModel characterModel = characterData.InstantiateModel(characterModelContainer);
                    if (characterModel != null)
                    {
                        _characterModelById[characterData.Id] = characterModel;
                        characterModel.SetupModelBodyParts(characterData);
                        characterModel.SetEquipItems(characterData.EquipItems, characterData.SelectableWeaponSets, characterData.EquipWeaponSet, false);
                        characterModel.gameObject.SetActive(false);
                        CharacterSelectionManager.Add(uiCharacter);
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
            // Do nothing
        }

        public override void OnClickStart()
        {
            UICharacter selectedUI = CharacterSelectionManager.SelectedUI;
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

        private void OnRequestedSelectCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseSelectCharacterMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            if (!GameInstance.MapInfos.TryGetValue(response.mapName, out BaseMapInfo mapInfo))
            {
                responseCode.ShowUnhandledResponseMessageDialog(UITextKeys.UI_ERROR_INVALID_DATA);
                return;
            }
            MMOClientInstance.Singleton.StartMapClient(mapInfo, response.networkAddress, response.networkPort);
        }

        public override void OnClickDelete()
        {
            UICharacter selectedUI = CharacterSelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_NO_CHOSEN_CHARACTER_TO_DELETE.ToString()));
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            IPlayerCharacterData playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestDeleteCharacter(playerCharacter.Id, OnRequestedDeleteCharacter);
        }

        private void OnRequestedDeleteCharacter(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseDeleteCharacterMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message)) return;
            // Reload characters
            LoadCharacters();
        }
    }
}
