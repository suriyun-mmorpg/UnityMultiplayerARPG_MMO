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

        public async UniTaskVoid HandleRequestMoveItemFromStorage(RequestHandlerData requestHandler, RequestMoveItemFromStorageMessage request, RequestProceedResultDelegate<ResponseMoveItemFromStorageMessage> result)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            StorageId storageId = new StorageId(request.storageType, request.storageOwnerId);
            IPlayerCharacterData playerCharacter;
            if (!GameInstance.ServerUserHandlers.TryGetPlayerCharacter(requestHandler.ConnectionId, out playerCharacter))
            {
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = ResponseMoveItemFromStorageMessage.Error.NotAllowed,
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
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                ResponseMoveItemFromStorageMessage.Error error = ResponseMoveItemFromStorageMessage.Error.None;
                switch (resp.Error)
                {
                    case EStorageError.StorageErrorInvalidInventoryIndex:
                    case EStorageError.StorageErrorInvalidStorageIndex:
                        BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                        error = ResponseMoveItemFromStorageMessage.Error.InvalidItemIndex;
                        break;
                    case EStorageError.StorageErrorInventoryWillOverwhelming:
                    case EStorageError.StorageErrorStorageWillOverwhelming:
                        BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                        error = ResponseMoveItemFromStorageMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemFromStorageMessage()
                {
                    error = error,
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
                    error = ResponseMoveItemToStorageMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = ResponseMoveItemToStorageMessage.Error.NotAllowed,
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
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                ResponseMoveItemToStorageMessage.Error error = ResponseMoveItemToStorageMessage.Error.None;
                switch (resp.Error)
                {
                    case EStorageError.StorageErrorInvalidInventoryIndex:
                    case EStorageError.StorageErrorInvalidStorageIndex:
                        BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                        error = ResponseMoveItemToStorageMessage.Error.InvalidItemIndex;
                        break;
                    case EStorageError.StorageErrorInventoryWillOverwhelming:
                    case EStorageError.StorageErrorStorageWillOverwhelming:
                        BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotCarryAnymore);
                        error = ResponseMoveItemToStorageMessage.Error.CannotCarryAllItems;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseMoveItemToStorageMessage()
                {
                    error = error,
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
                    error = ResponseSwapOrMergeStorageItemMessage.Error.NotLoggedIn,
                });
                return;
            }
            if (!GameInstance.ServerStorageHandlers.CanAccessStorage(playerCharacter, storageId))
            {
                BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.CannotAccessStorage);
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = ResponseSwapOrMergeStorageItemMessage.Error.NotAllowed,
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
            if (resp.Error != EStorageError.StorageErrorNone)
            {
                ResponseSwapOrMergeStorageItemMessage.Error error = ResponseSwapOrMergeStorageItemMessage.Error.None;
                switch (resp.Error)
                {
                    case EStorageError.StorageErrorInvalidInventoryIndex:
                    case EStorageError.StorageErrorInvalidStorageIndex:
                        BaseGameNetworkManager.Singleton.SendServerGameMessage(requestHandler.ConnectionId, GameMessage.Type.InvalidItemData);
                        error = ResponseSwapOrMergeStorageItemMessage.Error.InvalidItemIndex;
                        break;
                }
                result.Invoke(AckResponseCode.Error, new ResponseSwapOrMergeStorageItemMessage()
                {
                    error = error,
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
