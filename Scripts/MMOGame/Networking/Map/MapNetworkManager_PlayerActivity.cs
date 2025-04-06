using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override bool IsInstanceMap()
        {
            return !IsAllocate && !string.IsNullOrEmpty(MapInstanceId);
        }

        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName) || (mapName.Equals(CurrentMapInfo.Id) && !IsInstanceMap()))
            {
                if (overrideRotation)
                    playerCharacterEntity.CurrentRotation = rotation;
                playerCharacterEntity.Teleport(position, Quaternion.Euler(playerCharacterEntity.CurrentRotation), false);
                return;
            }

            WarpCharacterRoutine(playerCharacterEntity, mapName, position, overrideRotation, rotation).Forget();
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        /// <summary>
        /// Warp to different map.
        /// </summary>
        /// <param name="playerCharacterEntity"></param>
        /// <param name="mapName"></param>
        /// <param name="position"></param>
        /// <param name="overrideRotation"></param>
        /// <param name="rotation"></param>
        /// <returns></returns>
        private async UniTaskVoid WarpCharacterRoutine(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
            if (string.IsNullOrEmpty(mapName))
                return;

            if (!_mapServerConnectionIdsBySceneName.TryGetValue(PeerInfoExtensions.GetPeerInfoKey(ChannelId, mapName), out CentralServerPeerInfo peerInfo))
                return;

            if (!GameInstance.MapInfos.TryGetValue(mapName, out BaseMapInfo mapInfo) || (!mapInfo.IsAddressableSceneValid() && !mapInfo.IsSceneValid()))
                return;

            await SaveAndWarpCharacterByPeerInfo(playerCharacterEntity, peerInfo, true, mapName, position, overrideRotation, rotation);
        }
#endif

        public override async void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            // Prepare data for warp character later when instance map server registered to this map server
            List<BasePlayerCharacterEntity> instanceMapWarpingCharacters = new List<BasePlayerCharacterEntity>();
            if (ServerPartyHandlers.TryGetParty(playerCharacterEntity.PartyId, out PartyData party))
            {
                // If character is party leader, will bring party member to join instance
                if (party.IsLeader(playerCharacterEntity.Id))
                {
                    List<BasePlayerCharacterEntity> aliveAllies = playerCharacterEntity.FindAliveEntities<BasePlayerCharacterEntity>(CurrentGameInstance.joinInstanceMapDistance, true, false, false, CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.playingLayer.Mask);
                    foreach (BasePlayerCharacterEntity aliveAlly in aliveAllies)
                    {
                        if (!party.IsMember(aliveAlly.Id))
                            continue;
                        instanceMapWarpingCharacters.Add(aliveAlly);
                        aliveAlly.IsWarping = true;
                    }
                    instanceMapWarpingCharacters.Add(playerCharacterEntity);
                    playerCharacterEntity.IsWarping = true;
                }
                else
                {
                    ServerGameMessageHandlers.SendGameMessage(playerCharacterEntity.ConnectionId, UITextKeys.UI_ERROR_PARTY_MEMBER_CANNOT_ENTER_INSTANCE);
                    return;
                }
            }
            else
            {
                // If no party enter instance alone
                instanceMapWarpingCharacters.Add(playerCharacterEntity);
                playerCharacterEntity.IsWarping = true;
            }

            // Generate instance id
            AsyncResponseData<ResponseSpawnMapMessage> result = await ClusterClient.SendRequestAsync<RequestSpawnMapMessage, ResponseSpawnMapMessage>(MMORequestTypes.SpawnMap, new RequestSpawnMapMessage()
            {
                channelId = ChannelId,
                mapName = mapName,
                instanceId = "__GENERATING__",
                instanceWarpPosition = position,
                instanceWarpOverrideRotation = overrideRotation,
                instanceWarpRotation = rotation,
            }, mapSpawnMillisecondsTimeout);

            // Failed to start a new instance
            if (!result.IsSuccess)
            {
                // Reset teleporting state
                foreach (BasePlayerCharacterEntity instanceMapWarpingCharacter in instanceMapWarpingCharacters)
                {
                    if (instanceMapWarpingCharacter == null)
                        continue;
                    instanceMapWarpingCharacter.IsWarping = false;
                    ServerGameMessageHandlers.SendGameMessage(instanceMapWarpingCharacter.ConnectionId, UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE);
                }
                return;
            }

            // Move characters to the instance
            List<UniTask> saveAndWarpTasks = new List<UniTask>();
            foreach (BasePlayerCharacterEntity instanceMapWarpingCharacter in instanceMapWarpingCharacters)
            {
                saveAndWarpTasks.Add(SaveAndWarpCharacterByPeerInfo(instanceMapWarpingCharacter, result.Response.peerInfo));
            }
            await UniTask.WhenAll(saveAndWarpTasks);
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
        private async UniTask SaveAndWarpCharacterByPeerInfo(BasePlayerCharacterEntity playerCharacterEntity, CentralServerPeerInfo peerInfo,
            bool changeMap = false, string mapName = "",
            Vector3 position = default, bool overrideRotation = false, Vector3 rotation = default)
        {
            if (playerCharacterEntity == null)
                return;

            long connectionId = playerCharacterEntity.ConnectionId;
            // Player's character is already unregistered?
            if (!ServerUserHandlers.TryGetPlayerCharacter(connectionId, out _))
                return;

            // Tell player that the character is warping
            playerCharacterEntity.IsWarping = true;

            // Unregister player character
            UnregisterPlayerCharacter(connectionId);

            // Save the characer
            if (playerCharacterEntity.TryGetComponent(out PlayerCharacterDataUpdater updater))
                Destroy(updater);
            DataUpdater.PlayerCharacterDataSaved(playerCharacterEntity.Id);
            await WaitAndSaveCharacter(TransactionUpdateCharacterState.All, playerCharacterEntity.CloneTo(new PlayerCharacterData()), changeMap, mapName, position, overrideRotation, rotation);

            // Remove this character from warping list
            playerCharacterEntity.IsWarping = false;

            // Destroy character from server
            playerCharacterEntity.NetworkDestroy();

            // Send message to client to warp
            MMOWarpMessage message = new MMOWarpMessage();
            message.networkAddress = peerInfo.networkAddress;
            message.networkPort = peerInfo.networkPort;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Warp, message);
        }
#endif
    }
}
