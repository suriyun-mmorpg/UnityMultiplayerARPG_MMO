using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
        private readonly HashSet<string> storageUsers = new HashSet<string>();

#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTaskVoid HandleRequestOpenStorage(RequestHandlerData requestHandler, RequestOpenStorageMessage request, RequestProceedResultDelegate<ResponseOpenStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.InvokeError(new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            storageUsers.Remove(playerCharacter.Id);
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            result.InvokeSuccess(new ResponseCloseStorageMessage());
            await UniTask.Yield();
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            AsyncResponseData<MoveItemFromStorageResp> resp = await DbServiceClient.MoveItemFromStorageAsync(new MoveItemFromStorageReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                CharacterId = playerCharacter.Id,
                WeightLimit = storage.weightLimit,
                SlotLimit = storage.slotLimit,
                StorageItemIndex = request.storageItemIndex,
                StorageItemAmount = request.storageItemAmount,
                InventoryItemIndex = request.inventoryItemIndex,
                Inventory = new List<CharacterItem>(playerCharacter.NonEquipItems),
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            UITextKeys message = resp.Response.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseMoveItemFromStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = resp.Response.InventoryItemItems;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.InvokeSuccess(new ResponseMoveItemFromStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            AsyncResponseData<MoveItemToStorageResp> resp = await DbServiceClient.MoveItemToStorageAsync(new MoveItemToStorageReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                CharacterId = playerCharacter.Id,
                WeightLimit = storage.weightLimit,
                SlotLimit = storage.slotLimit,
                InventoryItemIndex = request.inventoryItemIndex,
                InventoryItemAmount = request.inventoryItemAmount,
                StorageItemIndex = request.storageItemIndex,
                Inventory = new List<CharacterItem>(playerCharacter.NonEquipItems),
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            UITextKeys message = resp.Response.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseMoveItemToStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = resp.Response.InventoryItemItems;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.InvokeSuccess(new ResponseMoveItemToStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
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
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            AsyncResponseData<SwapOrMergeStorageItemResp> resp = await DbServiceClient.SwapOrMergeStorageItemAsync(new SwapOrMergeStorageItemReq()
            {
                StorageType = request.storageType,
                StorageOwnerId = request.storageOwnerId,
                CharacterId = playerCharacter.Id,
                WeightLimit = storage.weightLimit,
                SlotLimit = storage.slotLimit,
                FromIndex = request.fromIndex,
                ToIndex = request.toIndex,
            });
            if (!resp.IsSuccess)
            {
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_INTERNAL_SERVER_ERROR,
                });
                return;
            }
            UITextKeys message = resp.Response.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.InvokeError(new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = message,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.Response.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.InvokeSuccess(new ResponseSwapOrMergeStorageItemMessage());
#endif
        }
    }
}
