﻿using UnityEngine;
using UnityEngine.Events;

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = GameDataMenuConsts.MMO_NETWORK_SETTING_FILE, menuName = GameDataMenuConsts.MMO_NETWORK_SETTING_MENU, order = GameDataMenuConsts.MMO_NETWORK_SETTING_ORDER)]
    public class MmoNetworkSetting : BaseGameData
    {
        public string networkAddress = "127.0.0.1";
        public int networkPort = 6000;
        public bool webSocketSecure = false;
    }

    [System.Serializable]
    public class MmoNetworkSettingEvent : UnityEvent<MmoNetworkSetting> { }
}
