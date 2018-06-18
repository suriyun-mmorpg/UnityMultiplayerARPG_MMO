using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLibManager;

namespace Insthync.MMOG
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UIMmoCharacterList : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public Transform characterModelContainer;
        [Header("UI Elements")]
        public Button buttonStart;
        public Button buttonDelete;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiCharacterPrefab.gameObject;
                    cacheList.uiContainer = uiCharacterContainer;
                }
                return cacheList;
            }
        }

        private UICharacterSelectionManager selectionManager;
        public UICharacterSelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UICharacterSelectionManager>();
                selectionManager.selectionMode = UISelectionMode.Toggle;
                return selectionManager;
            }
        }

        private readonly Dictionary<string, CharacterModel> CharacterModels = new Dictionary<string, CharacterModel>();

        private void LoadCharacters()
        {
            MMOClientInstance.Singleton.RequestCharacters(OnLoadCharacters);
        }

        private void OnLoadCharacters(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCharactersMessage)message;
            SelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();

            var selectableCharacters = new List<PlayerCharacterData>();

            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
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
            selectableCharacters.Sort(new PlayerCharacterDataLastUpdateComparer().Desc());
            CacheList.Generate(selectableCharacters, (index, character, ui) =>
            {
                var uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = character;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                var characterModel = character.InstantiateModel(characterModelContainer);
                CharacterModels[character.Id] = characterModel;
                characterModel.gameObject.SetActive(false);
                characterModel.SetEquipWeapons(character.EquipWeapons);
                characterModel.SetEquipItems(character.EquipItems);
                SelectionManager.Add(uiCharacter);
            });
        }

        public override void Show()
        {
            buttonStart.onClick.RemoveListener(OnClickStart);
            buttonStart.onClick.AddListener(OnClickStart);
            buttonDelete.onClick.RemoveListener(OnClickDelete);
            buttonDelete.onClick.AddListener(OnClickDelete);
            // Clear selection
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            SelectionManager.Clear();
            CacheList.HideAll();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            LoadCharacters();
            base.Show();
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            base.Hide();
        }

        private void OnSelectCharacter(UICharacter ui)
        {
            buttonStart.gameObject.SetActive(true);
            buttonDelete.gameObject.SetActive(true);
            characterModelContainer.SetChildrenActive(false);
            var playerCharacter = ui.Data as IPlayerCharacterData;
            ShowCharacter(playerCharacter.Id);
        }

        private void ShowCharacter(string id)
        {
            CharacterModel characterModel;
            if (string.IsNullOrEmpty(id) || !CharacterModels.TryGetValue(id, out characterModel))
                return;
            characterModel.gameObject.SetActive(true);
        }

        private void OnClickStart()
        {
            var selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot start game", "Please choose character to start game");
                Debug.LogWarning("Cannot start game, No chosen character");
                return;
            }
            // Load gameplay scene, we're going to manage maps in gameplay scene later
            // So we can add gameplay UI just once in gameplay scene
            var playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestSelectCharacter(playerCharacter.Id, OnSelectCharacter);
        }

        private void OnSelectCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseSelectCharacterMessage)message;
            
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
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
                    // Disconnect from central server then connect to map server
                    MMOClientInstance.Singleton.StopCentralClient();
                    MMOClientInstance.Singleton.StartMapClient(castedMessage.sceneName, castedMessage.networkAddress, castedMessage.networkPort, castedMessage.connectKey);
                    break;
            }
        }

        private void OnClickDelete()
        {
            var selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot delete character", "Please choose character to delete");
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            var playerCharacter = selectedUI.Data as IPlayerCharacterData;
            MMOClientInstance.Singleton.RequestDeleteCharacter(playerCharacter.Id, OnDeleteCharacter);
        }

        private void OnDeleteCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseDeleteCharacterMessage)message;
            
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
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
