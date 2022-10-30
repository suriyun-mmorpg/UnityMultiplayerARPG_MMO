using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
#if UNITY_EDITOR || UNITY_SERVER
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            if (request.storageType != StorageType.Player &&
                request.storageType != StorageType.Guild)
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            StorageId storageId;
            if (!playerCharacter.GetStorageId(request.storageType, 0, out storageId))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, storageId);
            result.InvokeSuccess(new ResponseOpenStorageMessage());
            await UniTask.Yield();
#endif
        }

        public async UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            result.InvokeSuccess(new ResponseCloseStorageMessage());
            await UniTask.Yield();
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            // Refresh storage item from database
            AsyncResponseData<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            List<CharacterItem> storageItems = readResp.Response.StorageCharacterItems;
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem)
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage());
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = true;
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            UITextKeys gameMessage;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            if (!applyingPlayerCharacter.MoveItemFromStorage(isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.storageItemAmount, request.inventoryType, request.inventoryItemIndex, request.equipSlotIndexOrWeaponSet, out gameMessage))
            {
                if (playerCharacterEntity != null)
                    playerCharacterEntity.IsUpdatingStorage = false;
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            // Update storage items to database
            AsyncResponseData<EmptyMessage> updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
                UpdateCharacterData = true,
                CharacterData = applyingPlayerCharacter,
            });
            if (!updateResp.IsSuccess)
            {
                if (playerCharacterEntity != null)
                    playerCharacterEntity.IsUpdatingStorage = false;
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = false;
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;

            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemFromStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            // Refresh storage item from database
            AsyncResponseData<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            List<CharacterItem> storageItems = readResp.Response.StorageCharacterItems;
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem)
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage());
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = true;
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            short weightLimit = storage.weightLimit;
            short slotLimit = storage.slotLimit;
            UITextKeys gameMessage;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            if (!applyingPlayerCharacter.MoveItemToStorage(isLimitWeight, weightLimit, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.inventoryType, request.inventoryItemIndex, request.inventoryItemAmount, request.equipSlotIndexOrWeaponSet, out gameMessage))
            {
                if (playerCharacterEntity != null)
                    playerCharacterEntity.IsUpdatingStorage = false;
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = gameMessage,
                });
                return;
            }
            // Update storage items to database
            AsyncResponseData<EmptyMessage> updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
                UpdateCharacterData = true,
                CharacterData = applyingPlayerCharacter,
            });
            if (!updateResp.IsSuccess)
            {
                if (playerCharacterEntity != null)
                    playerCharacterEntity.IsUpdatingStorage = false;
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = false;
            // Apply updated data
            playerCharacter.NonEquipItems = applyingPlayerCharacter.NonEquipItems;
            playerCharacter.EquipItems = applyingPlayerCharacter.EquipItems;
            playerCharacter.SelectableWeaponSets = applyingPlayerCharacter.SelectableWeaponSets;

            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemToStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if UNITY_EDITOR || UNITY_SERVER
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            // Refresh storage item from database
            AsyncResponseData<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            List<CharacterItem> storageItems = readResp.Response.StorageCharacterItems;
            // Validate swap or merge indexes
            if (request.fromIndex >= storageItems.Count ||
                request.toIndex >= storageItems.Count)
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX,
                });
                return;
            }
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem)
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage());
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = true;
            // Perform swap or merge items
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            short slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem fromItem = storageItems[request.fromIndex].Clone(true);
            CharacterItem toItem = storageItems[request.toIndex].Clone(true);
            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                short maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    storageItems[request.fromIndex] = CharacterItem.Empty;
                    storageItems[request.toIndex] = toItem;
                }
                else
                {
                    short remains = (short)(toItem.amount + fromItem.amount - maxStack);
                    toItem.amount = maxStack;
                    fromItem.amount = remains;
                    storageItems[request.fromIndex] = fromItem;
                    storageItems[request.toIndex] = toItem;
                }
            }
            else
            {
                // Swap
                storageItems[request.fromIndex] = toItem;
                storageItems[request.toIndex] = fromItem;
            }
            storageItems.FillEmptySlots(isLimitSlot, slotLimit);
            // Update storage items to database
            AsyncResponseData<EmptyMessage> updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
                UpdateCharacterData = false,
            });
            if (!updateResp.IsSuccess)
            {
                if (playerCharacterEntity != null)
                    playerCharacterEntity.IsUpdatingStorage = false;
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            if (playerCharacterEntity != null)
                playerCharacterEntity.IsUpdatingStorage = false;

            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeStorageItemMessage());
#endif
        }
    }
}
