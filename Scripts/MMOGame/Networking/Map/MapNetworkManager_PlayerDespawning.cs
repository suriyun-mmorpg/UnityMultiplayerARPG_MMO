using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _despawningPlayerCharacterCancellations = new ConcurrentDictionary<string, CancellationTokenSource>();
        private readonly ConcurrentDictionary<string, BasePlayerCharacterEntity> _despawningPlayerCharacterEntities = new ConcurrentDictionary<string, BasePlayerCharacterEntity>();
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
                playerCharacterEntity.SetOwnerClient(-1);
                playerCharacterEntity.StopMove();
                MovementState movementState = playerCharacterEntity.MovementState;
                movementState &= ~MovementState.Forward;
                movementState &= ~MovementState.Backward;
                movementState &= ~MovementState.Right;
                movementState &= ~MovementState.Left;
                playerCharacterEntity.KeyMovement(Vector3.zero, movementState);
                string userId = playerCharacterEntity.UserId;
                // Store despawnin user id, it will be used later if player not connect and continue playing the character
                RemoveDespawningCancellation(userId);
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                _despawningPlayerCharacterCancellations.TryAdd(userId, cancellationTokenSource);
                _despawningPlayerCharacterEntities.TryAdd(userId, playerCharacterEntity);

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
                    _despawningPlayerCharacterEntities.TryRemove(userId, out _);

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
                    _despawningPlayerCharacterEntities.TryRemove(userId, out _);
                }
                catch (System.OperationCanceledException)
                {
                    // Catch the cancellation
                }
                catch (System.Exception ex)
                {
                    // Other errors
                    Logging.LogException(LogTag, ex);
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
                    Logging.LogException(LogTag, ex);
                }
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public async UniTask SaveAndDespawnPendingPlayerCharacter(string userId)
        {
            // Find despawning character
            if (!_despawningPlayerCharacterCancellations.TryGetValue(userId, out CancellationTokenSource cancellationTokenSource) ||
                !_despawningPlayerCharacterEntities.TryGetValue(userId, out BasePlayerCharacterEntity playerCharacterEntity) ||
                cancellationTokenSource.IsCancellationRequested)
            {
                // No despawning character
                return;
            }

            // Cancel character despawning to despawning immediately
            _despawningPlayerCharacterCancellations.TryRemove(userId, out _);
            _despawningPlayerCharacterEntities.TryRemove(userId, out _);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            // Save character before despawned
            if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                Destroy(updater);
            DataUpdater.PlayerCharacterDataSaved(playerCharacterEntity.Id);
            await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()));
            playerCharacterEntity.NetworkDestroy();
        }
#endif
    }
}
