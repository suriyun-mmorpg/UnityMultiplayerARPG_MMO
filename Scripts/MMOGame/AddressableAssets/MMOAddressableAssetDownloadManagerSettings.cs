using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MultiplayerARPG.MMO
{
    [CreateAssetMenu(fileName = "MMO Addressable Asset Download Manager Settings", menuName = "Addressables/MMO Addressable Asset Download Manager Settings")]
    public class MMOAddressableAssetDownloadManagerSettings : AddressableAssetDownloadManagerSettings
    {
        public AssetReferenceMapNetworkManager mapNetworkManager;
        public AssetReferenceMMOClientInstance mmoClientInstance;
        public AssetReferenceMMOServerInstance mmoServerInstance;
        public AssetReferenceGameInstance gameInstance;

        [System.NonSerialized]
        private List<AssetReference> _filledInitialObjects = null;
        public override List<AssetReference> InitialObjects
        {
            get
            {
                if (_filledInitialObjects == null)
                {
                    _filledInitialObjects = new List<AssetReference>();
                    _filledInitialObjects.AddRange(initialObjects);
                    _filledInitialObjects.Add(mapNetworkManager);
                    _filledInitialObjects.Add(mmoClientInstance);
                    _filledInitialObjects.Add(mmoServerInstance);
                    _filledInitialObjects.Add(gameInstance);
                }
                return _filledInitialObjects;
            }
        }
    }
}
