using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    public class UIMmoServerEntry : UISelectionEntry<MmoNetworkSetting>
    {
        [System.Obsolete("Deprecated, use `uiTextUsername` instead.")]
        [HideInInspector]
        public Text textTitle;
        public TextWrapper uiTextTitle;

        protected override void Awake()
        {
            base.Awake();
            MigrateTextComponent();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (MigrateTextComponent())
                EditorUtility.SetDirty(this);
        }
#endif

        public bool MigrateTextComponent()
        {
            bool hasChanges = false;
            TextWrapper wrapper;
#pragma warning disable CS0618 // Type or member is obsolete
            if (textTitle != null)
            {
                hasChanges = true;
                wrapper = textTitle.gameObject.GetOrAddComponent<TextWrapper>();
                wrapper.unityText = textTitle;
                uiTextTitle = wrapper;
                textTitle = null;
            }
#pragma warning restore CS0618 // Type or member is obsolete
            return hasChanges;
        }

        protected override void UpdateData()
        {
            if (uiTextTitle != null)
                uiTextTitle.text = Data.title;
        }
    }

    [System.Serializable]
    public class UIMmoServerEntryEvent : UnityEvent<UIMmoServerEntry> { }
}
