using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Insthync.MMOG
{
    public class UIMmoServerEntry : UISelectionEntry<MmoNetworkSetting>
    {
        public Text textTitle;

        protected override void UpdateData()
        {
            if (textTitle != null)
                textTitle.text = Data.title;
        }
    }

    [System.Serializable]
    public class UIMmoServerEntryEvent : UnityEvent<UIMmoServerEntry> { }
}
