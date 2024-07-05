using Insthync.AddressableAssetTools;

namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public class AssetReferenceMapNetworkManager : AssetReferenceComponent<MapNetworkManager>
    {
        public AssetReferenceMapNetworkManager(string guid) : base(guid)
        {
        }
    }
}