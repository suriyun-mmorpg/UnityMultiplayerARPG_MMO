using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace MultiplayerARPG.MMO
{
    public class UIMmoServerEntry : UISelectionEntry<MmoNetworkSetting>
    {
        public TextWrapper uiTextTitle;
        public TextWrapper uiTextDescription;
        public Image imageIcon;

        protected override void OnDestroy()
        {
            base.OnDestroy();
            uiTextTitle = null;
            uiTextDescription = null;
            imageIcon = null;
            _data = null;
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = Data.Title;

            if (uiTextDescription != null)
                uiTextDescription.text = Data.Description;

            imageIcon.SetImageGameDataIcon(Data);
        }
    }

    [System.Serializable]
    public class UIMmoServerEntryEvent : UnityEvent<UIMmoServerEntry> { }
}
