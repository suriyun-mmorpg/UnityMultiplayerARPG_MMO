using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        private readonly Dictionary<StorageId, List<CharacterItem>> storageItems = new Dictionary<StorageId, List<CharacterItem>>();
        private readonly Dictionary<StorageId, HashSet<long>> usingStorageCharacters = new Dictionary<StorageId, HashSet<long>>();
#endif

        public async UniTaskVoid OpenStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!CanAccessStorage(playerCharacterEntity.CurrentStorageId, playerCharacterEntity))
            {
                SendServerGameMessage(playerCharacterEntity.ConnectionId, GameMessage.Type.CannotAccessStorage);
                return;
            }
            if (!usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId] = new HashSet<long>();
            usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Add(playerCharacterEntity.ConnectionId);
            ReadStorageItemsReq req = new ReadStorageItemsReq();
            req.StorageType = (EStorageType)playerCharacterEntity.CurrentStorageId.storageType;
            req.StorageOwnerId = playerCharacterEntity.CurrentStorageId.storageOwnerId;
            ReadStorageItemsResp resp = await DbServiceClient.ReadStorageItemsAsync(req);
            List<CharacterItem> storageItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            SetStorageItems(playerCharacterEntity.CurrentStorageId, storageItems);
            NotifyStorageItemsToCharacters(new HashSet<long>()
            {
                playerCharacterEntity.ConnectionId
            });
#endif
        }

        public async UniTaskVoid CloseStorage(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (usingStorageCharacters.ContainsKey(playerCharacterEntity.CurrentStorageId))
                usingStorageCharacters[playerCharacterEntity.CurrentStorageId].Remove(playerCharacterEntity.ConnectionId);
#endif
            await UniTask.Yield();
        }

        public async UniTask<bool> IncreaseStorageItems(StorageId storageId, CharacterItem addingItem)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IncreaseStorageItemsReq req = new IncreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.MapName = CurrentMapInfo.Id;
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
            storageItems[storageId] = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            NotifyStorageItemsToCharacters(usingStorageCharacters[storageId]);
            return true;
#else
            return false;
#endif
        }

        public async UniTask<DecreaseStorageItemsResult> DecreaseStorageItems(StorageId storageId, int dataId, short amount)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            DecreaseStorageItemsReq req = new DecreaseStorageItemsReq();
            req.StorageType = (EStorageType)storageId.storageType;
            req.StorageOwnerId = storageId.storageOwnerId;
            req.MapName = CurrentMapInfo.Id;
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
            storageItems[storageId] = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems);
            NotifyStorageItemsToCharacters(usingStorageCharacters[storageId]);
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
                storageItems[storageId] = new List<CharacterItem>();
            return storageItems[storageId];
#else
            return new List<CharacterItem>();
#endif
        }

        public void SetStorageItems(StorageId storageId, List<CharacterItem> items)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (!storageItems.ContainsKey(storageId))
                storageItems[storageId] = new List<CharacterItem>();
            storageItems[storageId] = items;
#endif
        }

        public Storage GetStorage(StorageId storageId)
        {
            Storage storage = default(Storage);
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    storage = CurrentGameInstance.playerStorage;
                    break;
                case StorageType.Guild:
                    storage = CurrentGameInstance.guildStorage;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity))
                        storage = buildingEntity.storage;
                    break;
            }
            return storage;
        }

        public bool CanAccessStorage(StorageId storageId, IPlayerCharacterData playerCharacter)
        {
            switch (storageId.storageType)
            {
                case StorageType.Player:
                    if (!playerCharacter.UserId.Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Guild:
                    if (!Guilds.ContainsKey(playerCharacter.GuildId) ||
                        !playerCharacter.GuildId.ToString().Equals(storageId.storageOwnerId))
                        return false;
                    break;
                case StorageType.Building:
                    StorageEntity buildingEntity;
                    if (!TryGetBuildingEntity(storageId.storageOwnerId, out buildingEntity) ||
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
            return usingStorageCharacters.ContainsKey(id) &&
                usingStorageCharacters[id].Count > 0;
#else
            return false;
#endif
        }

        public List<CharacterItem> GetStorageEntityItems(StorageEntity storageEntity)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            if (storageEntity == null)
                return new List<CharacterItem>();
            StorageId id = new StorageId(StorageType.Building, storageEntity.Id);
            if (!storageItems.ContainsKey(id))
                storageItems[id] = new List<CharacterItem>();
            return storageItems[id];
#else
            return new List<CharacterItem>();
#endif
        }

        public void ClearStorage()
        {
            storageItems.Clear();
            usingStorageCharacters.Clear();
        }

        private void NotifyStorageItemsToCharacters(HashSet<long> connectionIds)
        {
            foreach (long connectionId in connectionIds)
            {
                if (Players.ContainsKey(connectionId))
                {

                }
            }
        }
    }
}
