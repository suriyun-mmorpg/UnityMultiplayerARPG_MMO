using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UIMmoCharacterList : UICharacterList
    {
        protected override void LoadCharacters()
        {
            MMOClientInstance.Singleton.RequestCharacters(OnRequestedCharacters);
        }

        private void OnRequestedCharacters(AckResponseCode responseCode, BaseAckMessage message)
        {
            ResponseCharactersMessage castedMessage = (ResponseCharactersMessage)message;
            SelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();

            List<PlayerCharacterData> selectableCharacters = new List<PlayerCharacterData>();

            switch (responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseCharactersMessage.Error.NotLoggedin:
                            errorMessage = "User not logged in";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Load Characters", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Load Characters", "Connection timeout");
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
            CacheList.Generate(selectableCharacters, (index, character, ui) =>
            {
                UICharacter uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = character;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                BaseCharacterModel characterModel = character.InstantiateModel(characterModelContainer);
                CharacterModels[character.Id] = characterModel;
                characterModel.gameObject.SetActive(false);
                characterModel.SetEquipWeapons(character.EquipWeapons);
                characterModel.SetEquipItems(character.EquipItems);
                SelectionManager.Add(uiCharacter);
            });
        }

        protected override void OnClickStart()
        {
            UICharacter selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot start game", "Please choose character to start game");
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
                            errorMessage = "User not logged in";
                            break;
                        case ResponseSelectCharacterMessage.Error.AlreadySelectCharacter:
                            errorMessage = "Already select character";
                            break;
                        case ResponseSelectCharacterMessage.Error.InvalidCharacterData:
                            errorMessage = "Invalid character data";
                            break;
                        case ResponseSelectCharacterMessage.Error.MapNotReady:
                            errorMessage = "Map server is not ready";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Select Character", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Select Character", "Connection timeout");
                    break;
                default:
                    MMOClientInstance.Singleton.StartMapClient(castedMessage.sceneName, castedMessage.networkAddress, castedMessage.networkPort, castedMessage.connectKey);
                    break;
            }
        }

        protected override void OnClickDelete()
        {
            UICharacter selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot delete character", "Please choose character to delete");
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
                            errorMessage = "User not logged in";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Delete Character", errorMessage);
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Delete Character", "Connection timeout");
                    break;
                default:
                    // Reload characters
                    LoadCharacters();
                    break;
            }
        }
    }
}
