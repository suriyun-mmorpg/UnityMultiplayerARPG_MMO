using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
            SelectionManager.Clear();
            // Unenabled buttons
            buttonStart.gameObject.SetActive(false);
            buttonDelete.gameObject.SetActive(false);
            // Remove all models
            characterModelContainer.RemoveChildren();
            CharacterModels.Clear();
            // Show list of created characters
            var selectableCharacters = PlayerCharacterDataExtension.LoadAllPersistentCharacterData();
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
            SelectionManager.eventOnSelect.RemoveListener(OnSelectCharacter);
            SelectionManager.eventOnSelect.AddListener(OnSelectCharacter);
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
            var characterData = new PlayerCharacterData();
            var playerCharacter = selectedUI.Data as IPlayerCharacterData;
            playerCharacter.CloneTo(characterData);
            var gameInstance = GameInstance.Singleton;
            var networkManager = LanRpgNetworkManager.Singleton;
            networkManager.selectedCharacter = characterData;
            networkManager.Assets.offlineScene.SceneName = gameInstance.homeScene;
            networkManager.Assets.onlineScene.SceneName = characterData.CurrentMapName;
            networkManager.StartGame();
        }

        private void OnClickDelete()
        {
            if (SelectionManager.SelectedUI == null)
            {
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot delete character", "Please choose character to delete");
                Debug.LogWarning("Cannot delete character, No chosen character");
                return;
            }

            var playerCharacter = SelectionManager.SelectedUI.Data as IPlayerCharacterData;
            playerCharacter.DeletePersistentCharacterData();
            // Reload characters
            LoadCharacters();
        }
    }
}
