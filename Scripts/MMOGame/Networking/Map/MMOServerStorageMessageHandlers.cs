using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        public UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (request.storageType == StorageType.None ||
                request.storageType == StorageType.Building)
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
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, storageId);
            result.InvokeSuccess(new ResponseOpenStorageMessage());
#endif
            return default;
        }

        public UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            result.InvokeSuccess(new ResponseCloseStorageMessage());
#endif
            return default;
        }

        public UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return default;
            }
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage());
                return default;
            }
            List<CharacterItem> storageItems = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            int slotLimit = storage.slotLimit;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            try
            {
                if (!applyingPlayerCharacter.MoveItemFromStorage(storageId, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.storageItemAmount, request.inventoryType, request.inventoryItemIndex, request.equipSlotIndexOrWeaponSet, out UITextKeys gameMessage))
                {
                    result.InvokeError(new ResponseMoveItemFromStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return default;
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogError("Unable to move item from storage: " + ex.StackTrace);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return default;
            }
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            SetStorageSavePending(storageId, true);
            result.InvokeSuccess(new ResponseMoveItemFromStorageMessage());
#endif
            return default;
        }

        public UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return default;
            }
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage());
                return default;
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
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            try
            {
                if (!applyingPlayerCharacter.MoveItemToStorage(storageId, isLimitWeight, weightLimit, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.inventoryType, request.inventoryItemIndex, request.inventoryItemAmount, request.equipSlotIndexOrWeaponSet, out UITextKeys gameMessage))
                {
                    result.InvokeError(new ResponseMoveItemToStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return default;
                }
            }
            catch (System.Exception ex)
            {
                Logging.LogError("Unable to move item to storage: " + ex.StackTrace);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return default;
            }
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            SetStorageSavePending(storageId, true);
            result.InvokeSuccess(new ResponseMoveItemToStorageMessage());
#endif
            return default;
        }

        public UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            int fromIndex = request.fromIndex;
            int toIndex = request.toIndex;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return default;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return default;
            }
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem())
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage());
                return default;
            }

            // Get items from storage
            List<CharacterItem> storageItems = GameInstance.ServerStorageHandlers.GetStorageItems(storageId);

            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            int slotLimit = storage.slotLimit;
            if (!playerCharacter.SwapOrMergeStorageItem(storageId, isLimitSlot, slotLimit, storageItems, fromIndex, toIndex, out UITextKeys gameMessage))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = gameMessage,
                });
                return default;
            }

            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            SetStorageSavePending(storageId, true);
            result.InvokeSuccess(new ResponseSwapOrMergeStorageItemMessage());
#endif
            return default;
        }

        private void SetStorageSavePending(StorageId storageId, bool isSavePending)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (storageId.storageType == StorageType.Guild)
                return;

            if (isSavePending)
                MMOServerInstance.Singleton.MapNetworkManager.pendingSaveStorageIds.Add(storageId);
            else
                MMOServerInstance.Singleton.MapNetworkManager.pendingSaveStorageIds.TryRemove(storageId);
#endif
        }
    }
}
