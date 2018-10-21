using LiteNetLibManager;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UICharacterSelectionManager))]
    public class UIMmoCharacterCreate : UIBase
    {
        public UICharacter uiCharacterPrefab;
        public Transform uiCharacterContainer;
        public Transform characterModelContainer;
        [Header("UI Elements")]
        public InputField inputCharacterName;
        public Button buttonCreate;
        [Header("Event")]
        public UnityEvent eventOnCreateCharacter;

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

        private readonly Dictionary<int, BaseCharacterModel> CharacterModels = new Dictionary<int, BaseCharacterModel>();

        public override void Show()
        {
            buttonCreate.onClick.RemoveListener(OnClickCreate);
            buttonCreate.onClick.AddListener(OnClickCreate);
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
            LoadCharacters();
            base.Show();
        }

        private void LoadCharacters()
        {
            SelectionManager.Clear();
            // Show list of characters that can be create
            var selectableCharacters = GameInstance.PlayerCharacterEntities.Values.ToList();
            CacheList.Generate(selectableCharacters, (index, characterEntity, ui) =>
            {
                var character = characterEntity.database;
                var characterData = new PlayerCharacterData();
                characterData.DataId = characterEntity.DataId;
                characterData.EntityId = characterEntity.EntityId;
                characterData.SetNewPlayerCharacterData(character.title, characterEntity.DataId, characterEntity.EntityId);
                var uiCharacter = ui.GetComponent<UICharacter>();
                uiCharacter.Data = characterData;
                // Select trigger when add first entry so deactivate all models is okay beacause first model will active
                var characterModel = characterData.InstantiateModel(characterModelContainer);
                CharacterModels[characterData.EntityId] = characterModel;
                characterModel.gameObject.SetActive(false);
                SelectionManager.Add(uiCharacter);
            });
        }

        public override void Hide()
        {
            characterModelContainer.RemoveChildren();
            inputCharacterName.text = "";
            base.Hide();
        }

        private void OnSelectCharacter(UICharacter ui)
        {
            characterModelContainer.SetChildrenActive(false);
            ShowCharacter(ui.Data.EntityId);
        }

        private void ShowCharacter(int id)
        {
            BaseCharacterModel characterModel;
            if (!CharacterModels.TryGetValue(id, out characterModel))
                return;
            characterModel.gameObject.SetActive(true);
        }

        private void OnClickCreate()
        {
            var gameInstance = GameInstance.Singleton;
            var selectedUI = SelectionManager.SelectedUI;
            if (selectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot create character", "Please select character class");
                Debug.LogWarning("Cannot create character, did not selected character class");
                return;
            }
            var characterName = inputCharacterName.text.Trim();
            MMOClientInstance.Singleton.RequestCreateCharacter(characterName, selectedUI.Data.DataId, selectedUI.Data.EntityId, OnCreateCharacter);
        }

        private void OnCreateCharacter(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseCreateCharacterMessage)message;

            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
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
