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
                playerCharacterEntity.Dealing.StopDealing();
                playerCharacterEntity.Vending.StopVending();
                playerCharacterEntity.SetOwnerClient(-1);
                playerCharacterEntity.StopMove();
                MovementState movementState = playerCharacterEntity.MovementState;
                movementState &= ~MovementState.Forward;
                movementState &= ~MovementState.Backward;
                movementState &= ~MovementState.Right;
                movementState &= ~MovementState.Left;
                playerCharacterEntity.KeyMovement(Vector3.zero, movementState);
                string id = playerCharacterEntity.Id;
                // Store despawning player character id, it will be used later if player not connect and continue playing the character
                CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
                _despawningPlayerCharacterCancellations.TryAdd(id, cancellationTokenSource);
                _despawningPlayerCharacterEntities.TryAdd(id, playerCharacterEntity);

                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserId(connectionId);

                try
                {
                    if (!IsInstanceMap() || !despawnImmediately)
                        await UniTask.Delay(playerCharacterDespawnMillisecondsDelay, true, PlayerLoopTiming.Update, cancellationTokenSource.Token);

                    // Save the characer
                    if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                        Destroy(updater);
                    await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()), cancellationTokenSource.Token);

                    // Destroy character from server
                    playerCharacterEntity.NetworkDestroy();
                    _despawningPlayerCharacterEntities.TryRemove(id, out _);
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
                    if (_despawningPlayerCharacterCancellations.TryRemove(id, out cancellationTokenSource))
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
            }
            else
            {
                UnregisterPlayerCharacter(connectionId);
                UnregisterUserId(connectionId);
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        public async UniTask SaveAndDespawnPendingPlayerCharacter(string characterId)
        {
            // Find despawning character
            if (!_despawningPlayerCharacterCancellations.TryGetValue(characterId, out CancellationTokenSource cancellationTokenSource) ||
                !_despawningPlayerCharacterEntities.TryGetValue(characterId, out BasePlayerCharacterEntity playerCharacterEntity) ||
                cancellationTokenSource.IsCancellationRequested)
            {
                // No despawning character
                return;
            }

            // Cancel character despawning to despawning immediately
            _despawningPlayerCharacterCancellations.TryRemove(characterId, out _);
            _despawningPlayerCharacterEntities.TryRemove(characterId, out _);
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();

            // Save character before despawned
            if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                Destroy(updater);
            await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()));

            // Despawn the character
            playerCharacterEntity.NetworkDestroy();
        }
#endif
    }
}
