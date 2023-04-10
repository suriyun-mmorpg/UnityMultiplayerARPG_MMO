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

        public async UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
            await UniTask.Yield();
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (request.storageType == StorageType.None ||
                request.storageType == StorageType.Building)
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (request.storageType == StorageType.Guild)
            {
                if (!GameInstance.ServerGuildHandlers.TryGetGuild(playerCharacter.GuildId, out GuildData guildData) || !guildData.CanUseStorage(playerCharacter.Id))
                {
                    result.InvokeError(new ResponseOpenStorageMessage()
                    {
                        message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                    });
                    return;
                }
            }
            if (!playerCharacter.GetStorageId(request.storageType, 0, out StorageId storageId))
            {
                result.InvokeError(new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, storageId);
            result.InvokeSuccess(new ResponseOpenStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
            await UniTask.Yield();
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
            {
                result.InvokeError(new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            result.InvokeSuccess(new ResponseCloseStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            if (GameInstance.ServerStorageHandlers.IsStorageBusy(storageId))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage());
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
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
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem() && !CharacterIsSaving(playerCharacterEntity))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage());
                return;
            }
            SetStorageBusy(storageId, playerCharacterEntity, true);
            // Refresh storage item from database
            DatabaseApiResult<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            List<CharacterItem> storageItems = readResp.Response.StorageCharacterItems;
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            int slotLimit = storage.slotLimit;
            UITextKeys gameMessage;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            try
            {
                if (!applyingPlayerCharacter.MoveItemFromStorage(isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.storageItemAmount, request.inventoryType, request.inventoryItemIndex, request.equipSlotIndexOrWeaponSet, out gameMessage))
                {
                    SetStorageBusy(storageId, playerCharacterEntity, false);
                    result.InvokeError(new ResponseMoveItemFromStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return;
                }
            }
            catch (System.Exception ex)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                Logging.LogError("Unable to move item from storage: " + ex.StackTrace);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update storage items to database
            DatabaseApiResult updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
                CharacterData = applyingPlayerCharacter,
            });
            if (!updateResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                Logging.LogError("Unable to update storage items when move item from storage");
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
            SetStorageBusy(storageId, playerCharacterEntity, false);
            result.InvokeSuccess(new ResponseMoveItemFromStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            if (GameInstance.ServerStorageHandlers.IsStorageBusy(storageId))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage());
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
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
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem() && !CharacterIsSaving(playerCharacterEntity))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage());
                return;
            }
            SetStorageBusy(storageId, playerCharacterEntity, true);
            // Refresh storage item from database
            DatabaseApiResult<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            List<CharacterItem> storageItems = readResp.Response.StorageCharacterItems;
            // Prepare storage data
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitWeight = storage.weightLimit > 0;
            bool isLimitSlot = storage.slotLimit > 0;
            int weightLimit = storage.weightLimit;
            int slotLimit = storage.slotLimit;
            UITextKeys gameMessage;
            // Don't apply data to player character immediately, it should be saved properly before apply the data
            PlayerCharacterData applyingPlayerCharacter = new PlayerCharacterData();
            applyingPlayerCharacter = playerCharacter.CloneTo(applyingPlayerCharacter);
            try
            {
                if (!applyingPlayerCharacter.MoveItemToStorage(isLimitWeight, weightLimit, isLimitSlot, slotLimit, storageItems, request.storageItemIndex, request.inventoryType, request.inventoryItemIndex, request.inventoryItemAmount, request.equipSlotIndexOrWeaponSet, out gameMessage))
                {
                    SetStorageBusy(storageId, playerCharacterEntity, false);
                    result.InvokeError(new ResponseMoveItemToStorageMessage()
                    {
                        message = gameMessage,
                    });
                    return;
                }
            }
            catch (System.Exception ex)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                Logging.LogError("Unable to move item to storage: " + ex.StackTrace);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            // Update storage items to database
            DatabaseApiResult updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
                CharacterData = applyingPlayerCharacter,
            });
            if (!updateResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                Logging.LogError("Unable to update storage items when move item to storage");
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
            SetStorageBusy(storageId, playerCharacterEntity, false);
            result.InvokeSuccess(new ResponseMoveItemToStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            // Validate user and storage accessibility
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            if (GameInstance.ServerStorageHandlers.IsStorageBusy(storageId))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage());
                return;
            }
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out IPlayerCharacterData playerCharacter))
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
            // Check that the character can move items or not
            BasePlayerCharacterEntity playerCharacterEntity = playerCharacter as BasePlayerCharacterEntity;
            if (playerCharacterEntity != null && !playerCharacterEntity.CanMoveItem() && !CharacterIsSaving(playerCharacterEntity))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage());
                return;
            }
            // Mark as busy to not allow storage to be changed
            SetStorageBusy(storageId, playerCharacterEntity, true);
            // Refresh storage item from database
            DatabaseApiResult<ReadStorageItemsResp> readResp = await DbServiceClient.ReadStorageItemsAsync(new ReadStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                ReadForUpdate = true,
            });
            if (!readResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
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
                SetStorageBusy(storageId, playerCharacterEntity, false);
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INVALID_ITEM_INDEX,
                });
                return;
            }
            // Perform swap or merge items
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            bool isLimitSlot = storage.slotLimit > 0;
            int slotLimit = storage.slotLimit;
            // Prepare item data
            CharacterItem fromItem = storageItems[request.fromIndex].Clone(true);
            CharacterItem toItem = storageItems[request.toIndex].Clone(true);
            if (fromItem.dataId.Equals(toItem.dataId) && !fromItem.IsFull() && !toItem.IsFull())
            {
                // Merge if same id and not full
                int maxStack = toItem.GetMaxStack();
                if (toItem.amount + fromItem.amount <= maxStack)
                {
                    toItem.amount += fromItem.amount;
                    storageItems[request.fromIndex] = CharacterItem.Empty;
                    storageItems[request.toIndex] = toItem;
                }
                else
                {
                    int remains = toItem.amount + fromItem.amount - maxStack;
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
            DatabaseApiResult updateResp = await DbServiceClient.UpdateStorageItemsAsync(new UpdateStorageItemsReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                StorageItems = storageItems,
            });
            if (!updateResp.IsSuccess)
            {
                SetStorageBusy(storageId, playerCharacterEntity, false);
                Logging.LogError("Unable to update storage items when swap or merge storage item");
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, storageItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            SetStorageBusy(storageId, playerCharacterEntity, false);
            result.InvokeSuccess(new ResponseSwapOrMergeStorageItemMessage());
#endif
        }

        private void SetStorageBusy(StorageId storageId, BasePlayerCharacterEntity playerCharacterEntity, bool isBusy)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (playerCharacterEntity != null)
            {
                playerCharacterEntity.IsUpdatingStorage = isBusy;
                // Don't allow to save character while working with storage
                if (isBusy)
                    MMOServerInstance.Singleton.MapNetworkManager.savingCharacters.Add(playerCharacterEntity.Id);
                else
                    MMOServerInstance.Singleton.MapNetworkManager.savingCharacters.Remove(playerCharacterEntity.Id);
            }
            GameInstance.ServerStorageHandlers.SetStorageBusy(storageId, isBusy);
#endif
        }

        private bool CharacterIsSaving(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            return playerCharacterEntity != null && MMOServerInstance.Singleton.MapNetworkManager.savingCharacters.Contains(playerCharacterEntity.Id);
#else
            return false;
#endif
        }
    }
}
