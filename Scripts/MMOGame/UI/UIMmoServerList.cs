using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [RequireComponent(typeof(UIMmoServerEntrySelectionManager))]
    public class UIMmoServerList : UIBase
    {
        public UIMmoServerEntry uiServerEntryPrefab;
        public Transform uiServerEntryContainer;

        private UIList cacheList;
        public UIList CacheList
        {
            get
            {
                if (cacheList == null)
                {
                    cacheList = gameObject.AddComponent<UIList>();
                    cacheList.uiPrefab = uiServerEntryPrefab.gameObject;
                    cacheList.uiContainer = uiServerEntryContainer;
                }
                return cacheList;
            }
        }

        private UIMmoServerEntrySelectionManager selectionManager;
        public UIMmoServerEntrySelectionManager SelectionManager
        {
            get
            {
                if (selectionManager == null)
                    selectionManager = GetComponent<UIMmoServerEntrySelectionManager>();
                selectionManager.selectionMode = UISelectionMode.Toggle;
                return selectionManager;
            }
        }

        public void OnClickConnect()
        {
            MmoNetworkSetting data = SelectionManager.SelectedUI.Data;
            MMOClientInstance.Singleton.StartCentralClient(data.networkAddress, data.networkPort);
        }

        protected virtual void OnEnable()
        {
            SelectionManager.DeselectSelectedUI();
            SelectionManager.Clear();

            MmoNetworkSetting[] networkSettings = MMOClientInstance.Singleton.NetworkSettings;
            CacheList.Generate(networkSettings, (index, networkSetting, ui) =>
            {
                UIMmoServerEntry uiServerEntry = ui.GetComponent<UIMmoServerEntry>();
                uiServerEntry.Data = networkSetting;
                uiServerEntry.Show();
                SelectionManager.Add(uiServerEntry);
                if (index == 0)
                    uiServerEntry.OnClickSelect();
            });
        }
    }
}
