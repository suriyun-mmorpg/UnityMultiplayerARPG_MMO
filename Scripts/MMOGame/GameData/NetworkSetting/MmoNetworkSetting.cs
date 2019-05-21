using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = "Mmo Network Setting", menuName = "Create NetworkSetting/Mmo Network Setting", order = -3998)]
    public class MmoNetworkSetting : ScriptableObject
    {
        public string title = "Local Server";
        public string networkAddress = "127.0.0.1";
        public int networkPort = 6000;
    }

    [System.Serializable]
    public class MmoNetworkSettingEvent : UnityEvent<MmoNetworkSetting> { }
}
