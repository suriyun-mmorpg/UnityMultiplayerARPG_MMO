using ConcurrentCollections;
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
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

        public UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (request.storageType == StorageType.None)
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return default;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (request.storageType == StorageType.Guild)
            {
                if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guildData) || !guildData.CanUseStorage(playerCharacter.Id))
                {
                    result.InvokeError(new ResponseOpenStorageMessage()
                    {
                        message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                    });
                    return default;
                }
            }
            if (!playerCharacter.GetStorageId(request.storageType, 0, out StorageId storageId))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND,
                });
                return default;
            }
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, null, storageId);
            result.InvokeSuccess(new ResponseOpenStorageMessage());
#endif
            return default;
        }

        public UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            GameInstance.ServerStorageHandlers.CloseAllStorages(requestHandler.ConnectionId);
            result.InvokeSuccess(new ResponseCloseStorageMessage());
#endif
            return default;
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            string userId = playerCharacter.UserId;
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);

            if (!playerCharacter.CanAccessStorage(storageId))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }

            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }

            if (HasStorageUser(userId, storageId))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            AddStorageUser(userId, storageId);

            // Get items from storage
            if (storageId.storageType == StorageType.Guild)
            {
                await MapNetworkManager.LoadStorageRoutine(storageId);
            }
            List<CharacterItem> storageItems = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);

            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            int weightLimit = storage.weightLimit;
            int slotLimit = storage.slotLimit;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(
                applyingPlayerCharacter, true, false, false,
                false, false, true, true, false, false, false,
                false, false, false, false, false);
            try
            {
                if (!applyingPlayerCharacter.MoveItemFromStorage(storageId, isLimitWeight, weightLimit, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.storageItemAmount, request.inventoryType, request.inventoryItemIndex, request.equipSlotIndexOrWeaponSet, out UITextKeys gameMessage))
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseMoveItemFromStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return;
                }
                DatabaseApiResult updateResponse = await DatabaseClient.UpdateStorageAndCharacterItemsAsync(new UpdateStorageAndCharacterItemsReq()
                {
                    StorageType = storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId,
                    StorageItems = storageItems,
                    CharacterId = applyingPlayerCharacter.Id,
                    SelectableWeaponSets = new List<EquipWeapons>(applyingPlayerCharacter.SelectableWeaponSets),
                    EquipItems = new List<CharacterItem>(applyingPlayerCharacter.EquipItems),
                    NonEquipItems = new List<CharacterItem>(applyingPlayerCharacter.NonEquipItems),
                });
                if (updateResponse.IsError)
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseMoveItemFromStorageMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Unable to move item from storage");
                Debug.LogException(ex);
                RemoveStorageUser(userId, storageId);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            RemoveStorageUser(userId, storageId);
            result.InvokeSuccess(new ResponseMoveItemFromStorageMessage());
#endif
            return;
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            string userId = playerCharacter.UserId;
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);

            if (!playerCharacter.CanAccessStorage(storageId))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }

            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }

            if (HasStorageUser(userId, storageId))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            AddStorageUser(userId, storageId);

            // Get items from storage
            if (storageId.storageType == StorageType.Guild)
            {
                await MapNetworkManager.LoadStorageRoutine(storageId);
            }
            List<CharacterItem> storageItems = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);

            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            int weightLimit = storage.weightLimit;
            int slotLimit = storage.slotLimit;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(
                applyingPlayerCharacter, true, false, false,
                false, false, true, true, false, false, false,
                false, false, false, false, false);
            try
            {
                if (!applyingPlayerCharacter.MoveItemToStorage(storageId, isLimitWeight, weightLimit, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.inventoryType, request.inventoryItemIndex, request.inventoryItemAmount, request.equipSlotIndexOrWeaponSet, out UITextKeys gameMessage))
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseMoveItemToStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return;
                }
                DatabaseApiResult updateResponse = await DatabaseClient.UpdateStorageAndCharacterItemsAsync(new UpdateStorageAndCharacterItemsReq()
                {
                    StorageType = storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId,
                    StorageItems = storageItems,
                    CharacterId = applyingPlayerCharacter.Id,
                    SelectableWeaponSets = new List<EquipWeapons>(applyingPlayerCharacter.SelectableWeaponSets),
                    EquipItems = new List<CharacterItem>(applyingPlayerCharacter.EquipItems),
                    NonEquipItems = new List<CharacterItem>(applyingPlayerCharacter.NonEquipItems),
                });
                if (updateResponse.IsError)
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseMoveItemToStorageMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Unable to move item to storage");
                Debug.LogException(ex);
                RemoveStorageUser(userId, storageId);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            RemoveStorageUser(userId, storageId);
            result.InvokeSuccess(new ResponseMoveItemToStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            int fromIndex = request.fromIndex;
            int toIndex = request.toIndex;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }

            string userId = playerCharacter.UserId;
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);

            if (!playerCharacter.CanAccessStorage(storageId))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }

            if (HasStorageUser(userId, storageId))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            AddStorageUser(userId, storageId);

            // Get items from storage
            if (storageId.storageType == StorageType.Guild)
            {
                await MapNetworkManager.LoadStorageRoutine(storageId);
            }
            List<CharacterItem> storageItems = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);

            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            int slotLimit = storage.slotLimit;
            try
            {
                if (!playerCharacter.SwapOrMergeStorageItem(storageId, isLimitSlot, slotLimit, storageItems, fromIndex, toIndex, out UITextKeys gameMessage))
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                    {
                        message = gameMessage,
                    });
                    return;
                }
                DatabaseApiResult updateResponse = await DatabaseClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
                {
                    StorageType = storageId.storageType,
                    StorageOwnerId = storageId.storageOwnerId,
                    StorageItems = storageItems,
                });
                if (updateResponse.IsError)
                {
                    RemoveStorageUser(userId, storageId);
                    result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                    {
                        message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                    });
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Unable to move item to storage");
                Debug.LogException(ex);
                RemoveStorageUser(userId, storageId);
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            RemoveStorageUser(userId, storageId);
            result.InvokeSuccess(new ResponseSwapOrMergeStorageItemMessage());
#endif
            return;
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private bool AddStorageUser(string userId, StorageId storageId)
        {
            if (!MapNetworkManager.storageUsers.TryGetValue(userId, out ConcurrentHashSet<StorageId> storageIds))
            {
                storageIds = new ConcurrentHashSet<StorageId>();
                MapNetworkManager.storageUsers.TryAdd(userId, storageIds);
            }
            return storageIds.Add(storageId);
        }

        private bool RemoveStorageUser(string userId, StorageId storageId)
        {
            if (!MapNetworkManager.storageUsers.TryGetValue(userId, out ConcurrentHashSet<StorageId> storageIds))
            {
                return false;
            }
            return storageIds.TryRemove(storageId);
        }

        private bool HasStorageUser(string userId, StorageId storageId)
        {
            return MapNetworkManager.storageUsers.TryGetValue(userId, out ConcurrentHashSet<StorageId> storageIds) && storageIds.Contains(storageId);
        }
#endif
    }
}
