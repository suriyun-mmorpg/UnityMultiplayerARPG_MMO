using Insthync.UnityEditorUtils;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = "ServerConfigData", menuName = "Create Server Config Data")]
    public partial class ServerConfigData : ScriptableObject
    {
        public ServerConfig config;

#if UNITY_EDITOR
        [InspectorButton(nameof(CopyAsJson), "Copy As Json")]
        public bool btnCopyAsJson;

        public void CopyAsJson()
        {
            GUIUtility.systemCopyBuffer = JsonConvert.SerializeObject(config);
        }
#endif
    }
}