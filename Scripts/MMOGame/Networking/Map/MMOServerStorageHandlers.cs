using ConcurrentCollections;
using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerStorageHandlers : MonoBehaviour, IServerStorageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private readonly ConcurrentDictionary<StorageId, List<CharacterItem>> storageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();
        private readonly ConcurrentDictionary<StorageId, HashSet<long>> usingStorageClients = new ConcurrentDictionary<StorageId, HashSet<long>>();
        private readonly ConcurrentHashSet<StorageId> convertingStorages = new ConcurrentHashSet<StorageId>();
        private readonly ConcurrentDictionary<long, List<UserUsingStorageData>> userUsingStorages = new ConcurrentDictionary<long, List<UserUsingStorageData>>();
        private float _lastUpdateTime = 0f;
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public IDatabaseClient DatabaseClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }

        public MapNetworkManager MapNetworkManager
        {
            get { return BaseGameNetworkManager.Singleton as MapNetworkManager; }
        }
#endif

        public void OpenStorage(long connectionId, IPlayerCharacterData playerCharacter, IActivatableEntity storageEntity, StorageId storageId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!CanAccessStorage(playerCharacter, storageEntity, storageId, out UITextKeys errorMessage))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(connectionId, errorMessage);
                return;
            }
            // Store storage usage states
            if (!usingStorageClients.ContainsKey(storageId))
                usingStorageClients.TryAdd(storageId, new HashSet<long>());
            usingStorageClients[storageId].Add(connectionId);
            // Using storage data
            if (!userUsingStorages.TryGetValue(connectionId, out List<UserUsingStorageData> oneUserUsingStorages))
            {
                oneUserUsingStorages = new List<UserUsingStorageData>();
                userUsingStorages.TryAdd(connectionId, oneUserUsingStorages);
            }
            UserUsingStorageData usingStorage = new UserUsingStorageData()
            {
                Id = storageId,
                RequireEntity = !storageEntity.IsNull(),
                Entity = storageEntity,
            };
            if (!oneUserUsingStorages.Contains(usingStorage))
                oneUserUsingStorages.Add(usingStorage);
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            // Notify storage items to client
            Storage storage = GetStorage(storageId, out uint storageObjectId);
            GameInstance.ServerGameMessageHandlers.NotifyStorageOpened(connectionId, storageId.storageType, storageId.storageOwnerId, storageObjectId, storage.weightLimit, storage.slotLimit);
            storageItems.FillEmptySlots(storage.slotLimit > 0, storage.slotLimit);
            GameInstance.ServerGameMessageHandlers.NotifyStorageItems(connectionId, storageId.storageType, storageId.storageOwnerId, storageItems);
#endif
        }

        public void CloseStorage(long connectionId, StorageId storageId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!usingStorageClients.ContainsKey(storageId))
                return;
            usingStorageClients[storageId].Remove(connectionId);
            if (userUsingStorages.TryGetValue(connectionId, out List<UserUsingStorageData> oneUserUsingStorages))
            {
                for (int i = oneUserUsingStorages.Count - 1; i >= 0; --i)
                {
                    if (oneUserUsingStorages[i].Id.Equals(storageId))
                    {
                        oneUserUsingStorages.RemoveAt(i);
                        break;
                    }
                }
            }
            GameInstance.ServerGameMessageHandlers.NotifyStorageClosed(connectionId, storageId.storageType, storageId.storageOwnerId);
#endif
        }

        public void CloseAllStorages(long connectionId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!userUsingStorages.TryGetValue(connectionId, out List<UserUsingStorageData> oneUserUsingStorages))
                return;
            for (int i = oneUserUsingStorages.Count - 1; i >= 0; --i)
            {
                CloseStorage(connectionId, oneUserUsingStorages[i].Id);
            }
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private void Update()
        {
            float time = Time.unscaledTime;
            // Update every seconds
            if (time - _lastUpdateTime < 1f)
                return;
            _lastUpdateTime = time;
            List<long> connectionIds = new List<long>(userUsingStorages.Keys);
            foreach (long connectionId in connectionIds)
            {
                if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(connectionId, out IPlayerCharacterData playerCharacter))
                {
                    CloseAllStorages(connectionId);
                    continue;
                }
                if (userUsingStorages.TryGetValue(connectionId, out List<UserUsingStorageData> oneUserUsingStorages))
                {
                    // Looking for far entities and close the storage
                    for (int i = oneUserUsingStorages.Count - 1; i >= 0; --i)
                    {
                        UserUsingStorageData oneUserUsingStorage = oneUserUsingStorages[i];
                        if (!oneUserUsingStorage.RequireEntity)
                            continue;
                        if (oneUserUsingStorage.Entity.IsNull() || Vector3.Distance(playerCharacter.CurrentPosition, oneUserUsingStorage.Entity.EntityTransform.position) > oneUserUsingStorage.Entity.GetActivatableDistance())
                            CloseStorage(connectionId, oneUserUsingStorage.Id);
                    }
                }
            }
        }
#endif

        public async UniTask<bool> ConvertStorageItems(StorageId storageId, List<StorageConvertItemsEntry> convertItems, List<CharacterItem> droppingItems)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (convertingStorages.Contains(storageId))
            {
                return false;
            }
            convertingStorages.Add(storageId);
            // Prepare storage data
            StorageType storageType = storageId.storageType;
            string storageOwnerId = storageId.storageOwnerId;
            Storage storage = GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            int weightLimit = storage.weightLimit;
            int slotLimit = storage.slotLimit;
            List<CharacterItem> storageItems = GetStorageItems(storageId);
            for (int i = 0; i < convertItems.Count; ++i)
            {
                int dataId = convertItems[i].dataId;
                int amount = convertItems[i].amount;
                int convertedDataId = convertItems[i].convertedDataId;
                int convertedAmount = convertItems[i].convertedAmount;
                // Decrease item from storage
                if (!storageItems.DecreaseItems(dataId, amount, isLimitSlot, out _))
                    continue;
                // Increase item to storage
                if (GameInstance.Items.ContainsKey(convertedDataId) && convertedAmount > 0)
                {
                    // Increase item to storage
                    CharacterItem droppingItem = CharacterItem.Create(convertedDataId, 1, convertedAmount);
                    if (!storageItems.IncreasingItemsWillOverwhelming(convertedDataId, convertedAmount, isLimitWeight, weightLimit, storageItems.GetTotalItemWeight(), isLimitSlot, slotLimit))
                    {
                        storageItems.IncreaseItems(droppingItem);
                    }
                    else
                    {
                        droppingItems.Add(droppingItem);
                    }
                }
            }
            // Update storage items
            DatabaseApiResult updateResponse = await DatabaseClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = storageId.storageType,
                StorageOwnerId = storageId.storageOwnerId,
                StorageItems = storageItems,
            });
            convertingStorages.TryRemove(storageId);
            if (updateResponse.IsError)
            {
                return false;
            }
            // Update slots
            storageItems.FillEmptySlots(isLimitSlot, slotLimit);
            SetStorageItems(storageId, storageItems);
            NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
            return true;
#else
            return false;
#endif
        }

        public List<CharacterItem> GetStorageItems(StorageId storageId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            return storageItems[storageId];
#else
            return new List<CharacterItem>();
#endif
        }

        public void SetStorageItems(StorageId storageId, List<CharacterItem> items)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            storageItems[storageId] = items;
#endif
        }

        public Storage GetStorage(StorageId storageId, out uint objectId)
        {
            objectId = 0;
            Storage storage = default;
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    if (GameInstance.ServerBuildingHandlers.TryGetBuilding(storageId.storageOwnerId, out StorageEntity buildingEntity))
                    {
                        objectId = buildingEntity.ObjectId;
                        storage = buildingEntity.Storage;
                    }
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(IPlayerCharacterData playerCharacter, IActivatableEntity storageEntity, StorageId storageId, out UITextKeys uiTextKeys)
        {
            if (!playerCharacter.CanAccessStorage(storageId))
            {
                uiTextKeys = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE;
                return false;
            }

            if (storageEntity.IsNull())
            {
                // TODO: May add an options or rules to allow to open storage without storage entity needed
                uiTextKeys = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                return false;
            }

            if (Vector3.Distance(playerCharacter.CurrentPosition, storageEntity.EntityTransform.position) > storageEntity.GetActivatableDistance())
            {
                uiTextKeys = UITextKeys.UI_ERROR_CHARACTER_IS_TOO_FAR;
                return false;
            }

            uiTextKeys = UITextKeys.NONE;
            return true;
        }

        public bool IsStorageEntityOpen(StorageEntity storageEntity)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (storageEntity == null)
                return false;
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            return usingStorageClients.ContainsKey(id) && usingStorageClients[id].Count > 0;
#else
            return false;
#endif
        }

        public List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (storageEntity == null)
                return new List<CharacterItem>();
            return GetStorageItems(new StorageId(StorageType.Building, storageEntity.Id));
#else
            return new List<CharacterItem>();
#endif
        }

        public void ClearStorage()
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            foreach (var collection in storageItems.Values)
            {
                collection.Clear();
            }
            storageItems.Clear();

            foreach (var collection in usingStorageClients.Values)
            {
                collection.Clear();
            }
            usingStorageClients.Clear();

            foreach (var collection in userUsingStorages.Values)
            {
                collection.Clear();
            }
            userUsingStorages.Clear();
#endif
        }

        public void NotifyStorageItemsUpdated(StorageType storageType, string storageOwnerId)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            StorageId storageId = new StorageId(storageType, storageOwnerId);
            GameInstance.ServerGameMessageHandlers.NotifyStorageItemsToClients(usingStorageClients[storageId], storageId.storageType, storageId.storageOwnerId, GetStorageItems(storageId));
#endif
        }

        public IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems()
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            return storageItems;
#else
            return new ConcurrentDictionary<StorageId, List<CharacterItem>>();
#endif
        }
    }
}
