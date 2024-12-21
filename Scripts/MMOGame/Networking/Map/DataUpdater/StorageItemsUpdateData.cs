using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public class StorageItemsUpdateData
    {
        private StorageId _storageId;
        private List<CharacterItem> _storageItems;

        public void Update(StorageId storageId, IList<CharacterItem> storageItems)
        {
            _storageId = storageId;
            _storageItems = null;
            if (_storageItems == null)
                return;
            _storageItems = new List<CharacterItem>(storageItems);
        }

        public async UniTask<bool> ProceedSave(MapNetworkManagerDataUpdater updater)
        {
            if (_storageItems == null)
            {
                updater.StorageItemsDateSaved(_storageId);
                return true;
            }
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (await updater.Manager.SaveStorage(_storageId, _storageItems, false))
            {
                updater.StorageItemsDateSaved(_storageId);
                _storageItems = null;
                return true;
            }
#endif
            return false;
        }
    }
}