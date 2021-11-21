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
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            StorageId storageId;
            if (!playerCharacter.GetStorageId(request.storageType, 0, out storageId))
            {
                result.Invoke(AckResponseCode.Error, new ResponseOpenStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_STORAGE_NOT_FOUND,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.OpenStorage(requestHandler.ConnectionId, playerCharacter, storageId);
            result.Invoke(AckResponseCode.Success, new ResponseOpenStorageMessage());
            await UniTask.Yield();
#endif
        }

        public async UniTaskVoid HandleRequestCloseStorage(RequestHandlerData requestHandler, EmptyMessage request, RequestProceedResultDelegate<ResponseCloseStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseCloseStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            storageUsers.Remove(playerCharacter.Id);
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
            result.Invoke(AckResponseCode.Success, new ResponseCloseStorageMessage());
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
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            MoveItemFromStorageReq req = new MoveItemFromStorageReq();
            req.StorageType = request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.StorageItemIndex = request.storageItemIndex;
            req.StorageItemAmount = request.storageItemAmount;
            req.InventoryItemIndex = request.inventoryItemIndex;
            req.Inventory = new List<CharacterItem>(playerCharacter.NonEquipItems);
            MoveItemFromStorageResp resp = await DbServiceClient.MoveItemFromStorageAsync(req);
            UITextKeys message = resp.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = resp.InventoryItemItems;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemFromStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestMoveItemToStorage(RequestHandlerData requestHandler, RequestMoveItemToStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemToStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            MoveItemToStorageReq req = new MoveItemToStorageReq();
            req.StorageType = request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.InventoryItemIndex = request.inventoryItemIndex;
            req.InventoryItemAmount = request.inventoryItemAmount;
            req.StorageItemIndex = request.storageItemIndex;
            req.Inventory = new List<CharacterItem>(playerCharacter.NonEquipItems);
            MoveItemToStorageResp resp = await DbServiceClient.MoveItemToStorageAsync(req);
            UITextKeys message = resp.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = resp.InventoryItemItems;
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.Invoke(AckResponseCode.Success, new ResponseMoveItemToStorageMessage());
#endif
        }

        public async UniTaskVoid HandleRequestSwapOrMergeStorageItem(RequestHandlerData requestHandler, RequestSwapOrMergeStorageItemMessage request, RequestProceedResultDelegate<ResponseSwapOrMergeStorageItemMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_NOT_LOGGED_IN,
                });
                return;
            }
            if (!storageUsers.Add(playerCharacter.Id))
            {
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CONTENT_NOT_AVAILABLE,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            SwapOrMergeStorageItemReq req = new SwapOrMergeStorageItemReq();
            req.StorageType = request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.FromIndex = request.fromIndex;
            req.ToIndex = request.toIndex;
            SwapOrMergeStorageItemResp resp = await DbServiceClient.SwapOrMergeStorageItemAsync(req);
            UITextKeys message = resp.Error;
            if (message != UITextKeys.NONE)
            {
                storageUsers.Remove(playerCharacter.Id);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = message,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, resp.StorageCharacterItems);
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            storageUsers.Remove(playerCharacter.Id);
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeStorageItemMessage());
#endif
        }
    }
}
