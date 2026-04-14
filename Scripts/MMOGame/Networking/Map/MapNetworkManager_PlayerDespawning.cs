using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Concurrent;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private readonly ConcurrentDictionary<string, GameEntityCancellationTokenSource<BasePlayerCharacterEntity>> _despawningPlayerCharacterCancellations = new ConcurrentDictionary<string, GameEntityCancellationTokenSource<BasePlayerCharacterEntity>>();
#endif


#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public async UniTask SaveAndDespawnPlayerCharacter(long connectionId, bool despawnImmediately)
        {
            // Save player character data
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out BasePlayerCharacterEntity playerCharacterEntity))
            {
                cancellingReserveStorageCharacterIds.Add(playerCharacterEntity.Id);

                // Clear character states
                if (playerCharacterEntity.DealingComponent != null)
                    playerCharacterEntity.DealingComponent.StopDealing();
                if (playerCharacterEntity.VendingComponent != null)
                    playerCharacterEntity.VendingComponent.StopVending();
                ExtraMovementState extraMovementState = playerCharacterEntity.ExtraMovementState;
                playerCharacterEntity.SetOwnerClient(-1);
                playerCharacterEntity.StopMove();
                MovementState movementState = playerCharacterEntity.MovementState;
                movementState &= ~MovementState.Forward;
                movementState &= ~MovementState.Backward;
                movementState &= ~MovementState.Right;
                movementState &= ~MovementState.Left;
                movementState &= ~MovementState.Up;
                movementState &= ~MovementState.Down;
                playerCharacterEntity.KeyMovement(Vector3.zero, movementState);
                playerCharacterEntity.SetExtraMovementState(extraMovementState);
                string userId = playerCharacterEntity.UserId;
                // Store despawnin user id, it will be used later if player not connect and continue playing the character
                RemoveDespawningCancellation(userId);
                var cancellationTokenSource = new GameEntityCancellationTokenSource<BasePlayerCharacterEntity>(playerCharacterEntity);
                _despawningPlayerCharacterCancellations.TryAdd(userId, cancellationTokenSource);

                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserIdAndAccessToken(connectionId);

                // Save character immediately when player disconnect
                await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()));

                if (IsInstanceMap() || despawnImmediately)
                {
                    // Destroy character from server
                    if (playerCharacterEntity != null)
                    {
                        if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                            Destroy(updater);
                        DataUpdater.PlayerCharacterDataSaved(playerCharacterEntity.Id);
                        playerCharacterEntity.NetworkDestroy();
                    }

                    RemoveDespawningCancellation(userId);
                    return;
                }

                // Delay and kick later
                try
                {
                    await UniTask.Delay(playerCharacterDespawnMillisecondsDelay, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);

                    // Save the characer
                    if (playerCharacterEntity != null)
                    {
                        if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                            Destroy(updater);
                        DataUpdater.PlayerCharacterDataSaved(playerCharacterEntity.Id);
                        await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()), cancellationTokenSource.Token);
                        playerCharacterEntity.NetworkDestroy();
                    }
                }
                catch (System.OperationCanceledException)
                {
                    // Catch the cancellation
                }
                catch (System.Exception ex)
                {
                    // Other errors
                    Logging.LogError(LogTag, $"Error occuring while save and despawn player character entity\n{ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    RemoveDespawningCancellation(userId);
                }
            }
            else
            {
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserIdAndAccessToken(connectionId);
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private void RemoveDespawningCancellation(string userId)
        {
            if (_despawningPlayerCharacterCancellations.TryRemove(userId, out var cancellationTokenSource))
            {
                try
                {
                    cancellationTokenSource.Dispose();
                }
                catch (System.ObjectDisposedException)
                {
                    // Already disposed
                }
                catch (System.Exception ex)
                {
                    // Other errors
                    Logging.LogError(LogTag, $"Error occuring while remove despawning cancellation\n{ex.Message}\n{ex.StackTrace}");
                }
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public async UniTask SaveAndDespawnPendingPlayerCharacter(string userId)
        {
            // Find despawning character
            if (!_despawningPlayerCharacterCancellations.TryRemove(userId, out GameEntityCancellationTokenSource<BasePlayerCharacterEntity> cancellationTokenSource))
                return;

            if (cancellationTokenSource.IsCancellationRequested)
                return;

            // Cancel character despawning to despawning immediately
            BasePlayerCharacterEntity entity = cancellationTokenSource.Entity;
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            // Save character before despawned
            if (entity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                Destroy(updater);
            DataUpdater.PlayerCharacterDataSaved(entity.Id);
            await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, entity.CloneTo(new PlayerCharacterData()));
            entity.NetworkDestroy();
        }
#endif
    }
}
