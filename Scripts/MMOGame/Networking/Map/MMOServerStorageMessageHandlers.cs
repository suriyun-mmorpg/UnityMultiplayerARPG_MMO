using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public class MMOServerStorageMessageHandlers : MonoBehaviour, IServerStorageMessageHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseService.DatabaseServiceClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager.ServiceClient; }
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
            GameInstance.ServerStorageHandlers.CloseStorage(requestHandler.ConnectionId);
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
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            MoveItemFromStorageReq req = new MoveItemFromStorageReq();
            req.StorageType = (EStorageType)request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.StorageItemIndex = request.storageItemIndex;
            req.StorageItemAmount = request.storageItemAmount;
            req.InventoryItemIndex = request.inventoryItemIndex;
            MoveItemFromStorageResp resp = await DbServiceClient.MoveItemFromStorageAsync(req);
            UITextKeys message = (UITextKeys)resp.Error;
            if (message != UITextKeys.NONE)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, message);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.InventoryItemItems);
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems));
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
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
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            MoveItemToStorageReq req = new MoveItemToStorageReq();
            req.StorageType = (EStorageType)request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.InventoryItemIndex = request.inventoryItemIndex;
            req.InventoryItemAmount = request.inventoryItemAmount;
            req.StorageItemIndex = request.storageItemIndex;
            MoveItemToStorageResp resp = await DbServiceClient.MoveItemToStorageAsync(req);
            UITextKeys message = (UITextKeys)resp.Error;
            if (message != UITextKeys.NONE)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, message);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    message = message,
                });
                return;
            }
            playerCharacter.NonEquipItems = DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.InventoryItemItems);
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems));
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
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
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = UITextKeys.UI_ERROR_CANNOT_ACCESS_STORAGE,
                });
                return;
            }
            Storage storage = GameInstance.ServerStorageHandlers.GetStorage(storageId, out _);
            SwapOrMergeStorageItemReq req = new SwapOrMergeStorageItemReq();
            req.StorageType = (EStorageType)request.storageType;
            req.StorageOwnerId = request.storageOwnerId;
            req.CharacterId = playerCharacter.Id;
            req.WeightLimit = storage.weightLimit;
            req.SlotLimit = storage.slotLimit;
            req.FromIndex = request.fromIndex;
            req.ToIndex = request.toIndex;
            SwapOrMergeStorageItemResp resp = await DbServiceClient.SwapOrMergeStorageItemAsync(req);
            UITextKeys message = (UITextKeys)resp.Error;
            if (message != UITextKeys.NONE)
            {
                GameInstance.ServerGameMessageHandlers.SendGameMessage(requestHandler.ConnectionId, message);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    message = message,
                });
                return;
            }
            GameInstance.ServerStorageHandlers.SetStorageItems(storageId, DatabaseServiceUtils.MakeListFromRepeatedByteString<CharacterItem>(resp.StorageCharacterItems));
            GameInstance.ServerStorageHandlers.NotifyStorageItemsUpdated(request.storageType, request.storageOwnerId);
            // Success
            result.Invoke(AckResponseCode.Success, new ResponseSwapOrMergeStorageItemMessage());
#endif
        }
    }
}
