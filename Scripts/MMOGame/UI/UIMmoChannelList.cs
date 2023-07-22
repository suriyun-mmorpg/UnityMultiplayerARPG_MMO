using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UIMmoChannelEntrySelectionManager))]
    public class UIMmoChannelList : UIBase
    {
        public UIMmoChannelEntry uiChannelEntryPrefab;
        public Transform uiChannelEntryContainer;

        private UIList _list;
        public UIList List
        {
            get
            {
                if (_list == null)
                {
                    _list = gameObject.AddComponent<UIList>();
                    _list.uiPrefab = uiChannelEntryPrefab.gameObject;
                    _list.uiContainer = uiChannelEntryContainer;
                }
                return _list;
            }
        }

        private UIMmoChannelEntrySelectionManager _selectionManager;
        public UIMmoChannelEntrySelectionManager SelectionManager
        {
            get
            {
                if (_selectionManager == null)
                    _selectionManager = GetComponent<UIMmoChannelEntrySelectionManager>();
                _selectionManager.selectionMode = UISelectionMode.Toggle;
                return _selectionManager;
            }
        }

        private void OnEnable()
        {
            LoadChannels();
        }

        public void LoadChannels()
        {
            MMOClientInstance.Singleton.RequestChannels(OnRequestedChannels);
        }

        private void OnRequestedChannels(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseChannelsMessage response)
        {
            // Clear channel list
            SelectionManager.Clear();
            List.HideAll();
            // Put channels
            List.Generate(response.channels, (index, data, ui) =>
            {
                // Setup UIs
                UIMmoChannelEntry uiChannelEntry = ui.GetComponent<UIMmoChannelEntry>();
                uiChannelEntry.Data = data;
                SelectionManager.Add(uiChannelEntry);
            });
        }

        public void OnClickSelectChannel()
        {
            ChannelEntry data = SelectionManager.SelectedUI.Data;
            MMOClientInstance.Singleton.SelectedChannelId = data.id;
        }
    }
}