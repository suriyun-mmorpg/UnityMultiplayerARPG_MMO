using Cysharp.Threading.Tasks;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerStorageHandlers : MonoBehaviour, IServerStorageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        private readonly ConcurrentDictionary<StorageId, List<CharacterItem>> storageItems = new ConcurrentDictionary<StorageId, List<CharacterItem>>();
        private readonly ConcurrentDictionary<StorageId, HashSet<long>> usingStorageCharacters = new ConcurrentDictionary<StorageId, HashSet<long>>();
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
        }
#endif

        public async UniTaskVoid OpenStorage(BasePlayerCharacterEntity playerCharacter)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacter, playerCharacter.CurrentStorageId))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(playerCharacter.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!usingStorageCharacters.ContainsKey(playerCharacter.CurrentStorageId))
                usingStorageCharacters.TryAdd(playerCharacter.CurrentStorageId, new HashSet<long>());
            usingStorageCharacters[playerCharacter.CurrentStorageId].Add(playerCharacter.ConnectionId);
            // Load storage items from database
            ReadStorageItemsReq req = new ReadStorageItemsReq();
            req.StorageType = (EStorageType)playerCharacter.CurrentStorageId.storageType;
            req.StorageOwnerId = playerCharacter.CurrentStorageId.storageOwnerId;
            ReadStorageItemsResp resp = await DbServiceClient.ReadStorageItemsAsync(req);
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            SetStorageItems(playerCharacter.CurrentStorageId, storageItems);
            // Notify storage items to client
            uint storageObjectId;
            Storage storage = GetStorage(playerCharacter.CurrentStorageId, out storageObjectId);
            BaseGameNetworkManager.Singleton.SendNotifyStorageOpenedToClient(playerCharacter.ConnectionId, playerCharacter.CurrentStorageId.storageType, playerCharacter.CurrentStorageId.storageOwnerId, storageObjectId, storage.weightLimit, storage.slotLimit);
            BaseGameNetworkManager.Singleton.SendNotifyStorageItemsUpdatedToClient(playerCharacter.ConnectionId, GetStorageItems(playerCharacter.CurrentStorageId));
#endif
        }

        public async UniTaskVoid CloseStorage(BasePlayerCharacterEntity playerCharacter)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (usingStorageCharacters.ContainsKey(playerCharacter.CurrentStorageId))
                usingStorageCharacters[playerCharacter.CurrentStorageId].Remove(playerCharacter.ConnectionId);
#endif
            await UniTask.Yield();
        }

        public async UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Storage storge = GetStorage(storageId, out _);
            IncreaseStorageItemsReq req = new IncreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.WeightLimit = storge.weightLimit;
            req.SlotLimit = storge.slotLimit;
            req.Item = DatabaseServiceUtils.ToByteString(addingItem);
            IncreaseStorageItemsResp resp = await DbServiceClient.IncreaseStorageItemsAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                if (resp.Error == EStorageError.StorageErrorStorageWillOverwhelming)
                {
                    // TODO: May push error message
                }
                return false;
            }
            SetStorageItems(storageId, DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems));
            NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
            return true;
#else
            return false;
#endif
        }

        public async UniTask<DecreaseStorageItemsResult> DecreaseStorageItems(StorageId storageId, int dataId, short amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            Storage storge = GetStorage(storageId, out _);
            DecreaseStorageItemsReq req = new DecreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.WeightLimit = storge.weightLimit;
            req.SlotLimit = storge.slotLimit;
            req.DataId = dataId;
            req.Amount = amount;
            DecreaseStorageItemsResp resp = await DbServiceClient.DecreaseStorageItemsAsync(req);
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                if (resp.Error == EStorageError.StorageErrorStorageWillOverwhelming)
                {
                    // TODO: May push error message
                }
                return new DecreaseStorageItemsResult();
            }
            SetStorageItems(storageId, DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems));
            NotifyStorageItemsUpdated(storageId.storageType, storageId.storageOwnerId);
            Dictionary<int, short> decreasedItems = new Dictionary<int, short>();
            foreach (ItemIndexAmountMap entry in resp.DecreasedItems)
            {
                decreasedItems.Add(entry.Index, (short)entry.Amount);
            }
            return new DecreaseStorageItemsResult()
            {
                IsSuccess = true,
                DecreasedItems = decreasedItems,
            };
#else
            return new DecreaseStorageItemsResult();
#endif
        }

        public List<CharacterItem> GetStorageItems(StorageId storageId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            return storageItems[storageId];
#else
            return new List<CharacterItem>();
#endif
        }

        public void SetStorageItems(StorageId storageId, List<CharacterItem> items)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!storageItems.ContainsKey(storageId))
                storageItems.TryAdd(storageId, new List<CharacterItem>());
            storageItems[storageId] = items;
#endif
        }

        public Storage GetStorage(StorageId storageId, out uint objectId)
        {
            objectId = 0;
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = GameInstance.Singleton.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = GameInstance.Singleton.guildStorage;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (BaseGameNetworkManager.Singleton.TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity))
                    {
                        objectId = buildingEntity.ObjectId;
                        storage = buildingEntity.storage;
                    }
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(IPlayerCharacterData playerCharacter, StorageId storageId)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacter.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!GameInstance.ServerGuildHandlers.ContainsGuild(playerCharacter.GuildId) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!BaseGameNetworkManager.Singleton.TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity) ||
                        !(buildingEntity.IsCreator(playerCharacter.Id) || buildingEntity.canUseByEveryone))
                        return false;
                    break;
            }
            return true;
        }

        public bool IsStorageEntityOpen(StorageEntity storageEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (storageEntity == null)
                return false;
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            return usingStorageCharacters.ContainsKey(id) && usingStorageCharacters[id].Count > 0;
#else
            return false;
#endif
        }

        public List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (storageEntity == null)
                return new List<CharacterItem>();
            return GetStorageItems(new StorageId(StorageType.Building, storageEntity.Id));
#else
            return new List<CharacterItem>();
#endif
        }

        public void ClearStorage()
        {
            storageItems.Clear();
            usingStorageCharacters.Clear();
        }

        public void NotifyStorageItemsUpdated(StorageType storageType, string storageOwnerId)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            StorageId storageId = new StorageId(storageType, storageOwnerId);
            BaseGameNetworkManager.Singleton.SendNotifyStorageItemsUpdatedToClients(usingStorageCharacters[storageId], GetStorageItems(storageId));
#endif
        }

        public IDictionary<StorageId, List<CharacterItem>> GetAllStorageItems()
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            return storageItems;
#else
            return new ConcurrentDictionary<StorageId, List<CharacterItem>>();
#endif
        }
    }
}
