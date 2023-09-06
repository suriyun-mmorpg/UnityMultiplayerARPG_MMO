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

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = Data.Title;

            if (uiTextDescription != null)
                uiTextDescription.text = Data.Description;

            if (imageIcon != null)
            {
                Sprite iconSprite = Data == null ? null : Data.Icon;
                imageIcon.gameObject.SetActive(iconSprite != null);
                imageIcon.sprite = iconSprite;
                imageIcon.preserveAspect = true;
            }
        }
    }

    [System.Serializable]
    public class UIMmoServerEntryEvent : UnityEvent<UIMmoServerEntry> { }
}
