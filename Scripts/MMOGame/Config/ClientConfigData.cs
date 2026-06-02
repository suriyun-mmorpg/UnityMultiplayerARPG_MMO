using Insthync.UnityEditorUtils;
using Newtonsoft.Json;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = "ClientConfigData", menuName = "Create Client Config Data")]
    public partial class ClientConfigData : ScriptableObject
    {
        public ClientConfig config;

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