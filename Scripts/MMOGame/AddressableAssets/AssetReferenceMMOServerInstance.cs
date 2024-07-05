using Insthync.AddressableAssetTools;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    [System.Serializable]
    public class AssetReferenceMMOServerInstance : AssetReferenceComponent<MMOServerInstance>
    {
        public AssetReferenceMMOServerInstance(string guid) : base(guid)
        {
        }
    }
}