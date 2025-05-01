using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UIMmoServerEntrySelectionManager))]
    public class UIMmoServerList : UIBase
    {
        public UIMmoServerEntry uiServerEntryPrefab;
        public Transform uiServerEntryContainer;

        private UIList _list;
        public UIList List
        {
            get
            {
                if (_list == null)
                {
                    _list = gameObject.AddComponent<UIList>();
                    _list.uiPrefab = uiServerEntryPrefab.gameObject;
                    _list.uiContainer = uiServerEntryContainer;
                }
                return _list;
            }
        }

        private UIMmoServerEntrySelectionManager _selectionManager;
        public UIMmoServerEntrySelectionManager SelectionManager
        {
            get
            {
                if (_selectionManager == null)
                    _selectionManager = GetComponent<UIMmoServerEntrySelectionManager>();
                _selectionManager.selectionMode = UISelectionMode.Toggle;
                return _selectionManager;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiServerEntryPrefab = null;
            uiServerEntryContainer = null;
            _list = null;
            _selectionManager = null;
        }

        public void OnClickConnect()
        {
            MmoNetworkSetting data = SelectionManager.SelectedUI.Data;
            MMOClientInstance.Singleton.WebSocketSecure = data.webSocketSecure;
            MMOClientInstance.Singleton.StartCentralClient(data.networkAddress, data.networkPort);
        }

        protected virtual void OnEnable()
        {
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();
            RefreshServerList();
        }

        public async void RefreshServerList()
        {
            MmoNetworkSetting[] networkSettings = MMOClientInstance.Singleton.NetworkSettings;
            List<MmoNetworkSetting> servers = await ConfigManager.ReadServerList();
            if (servers != null && servers.Count > 0)
                networkSettings = servers.ToArray();
            List.Generate(networkSettings, (index, networkSetting, ui) =>
            {
                UIMmoServerEntry uiServerEntry = ui.GetComponent<UIMmoServerEntry>();
                uiServerEntry.Data = networkSetting;
                uiServerEntry.Show();
                SelectionManager.Add(uiServerEntry);
                if (index == 0)
                    uiServerEntry.SelectByManager();
            });
        }
    }
}
