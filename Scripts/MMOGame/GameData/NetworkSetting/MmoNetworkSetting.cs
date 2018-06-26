using UnityEngine;

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = "MmoNetworkSetting", menuName = "Create NetworkSetting/MmoNetworkSetting")]
    public class MmoNetworkSetting : ScriptableObject
    {
        public string title = "Local Server";
        public string networkAddress = "127.0.0.1";
        public int networkPort = 6000;
    }
}
