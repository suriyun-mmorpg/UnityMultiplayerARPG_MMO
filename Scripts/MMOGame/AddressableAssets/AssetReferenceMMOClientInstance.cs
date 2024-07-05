using Insthync.AddressableAssetTools;

namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public class AssetReferenceMMOClientInstance : AssetReferenceComponent<MMOClientInstance>
    {
        public AssetReferenceMMOClientInstance(string guid) : base(guid)
        {
        }
    }
}