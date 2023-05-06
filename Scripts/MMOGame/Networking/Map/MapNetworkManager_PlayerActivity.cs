using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MapNetworkManager
    {
        public override string GetCurrentMapId(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!IsInstanceMap())
                return base.GetCurrentMapId(playerCharacterEntity);
            return _locationsBeforeEnterInstance[playerCharacterEntity.Id].Key;
#else
            return string.Empty;
#endif
        }

        public override Vector3 GetCurrentPosition(BasePlayerCharacterEntity playerCharacterEntity)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!IsInstanceMap())
                return base.GetCurrentPosition(playerCharacterEntity);
            return _locationsBeforeEnterInstance[playerCharacterEntity.Id].Value;
#else
            return Vector3.zero;
#endif
        }

        protected override bool IsInstanceMap()
        {
            return !string.IsNullOrEmpty(MapInstanceId);
        }

        public override void WarpCharacter(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!CanWarpCharacter(playerCharacterEntity))
                return;

            // If map name is empty, just teleport character to target position
            if (string.IsNullOrEmpty(mapName) || (mapName.Equals(CurrentMapInfo.Id) && !IsInstanceMap()))
            {
                if (overrideRotation)
                    playerCharacterEntity.CurrentRotation = rotation;
                playerCharacterEntity.Teleport(position, Quaternion.Euler(playerCharacterEntity.CurrentRotation));
                return;
            }

            WarpCharacterRoutine(playerCharacterEntity, mapName, position, overrideRotation, rotation).Forget();
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
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
            // If warping to different map
            long connectionId = playerCharacterEntity.ConnectionId;
            CentralServerPeerInfo peerInfo;
            BaseMapInfo mapInfo;
            if (!string.IsNullOrEmpty(mapName) &&
                ServerUserHandlers.TryGetPlayerCharacter(connectionId, out _) &&
                _mapServerConnectionIdsBySceneName.TryGetValue(mapName, out peerInfo) &&
                GameInstance.MapInfos.TryGetValue(mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Add this character to warping list
                playerCharacterEntity.IsWarping = true;
                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                // Clone character data to save
                while (savingCharacters.Contains(playerCharacterEntity.Id))
                {
                    await UniTask.Yield();
                }
                await SaveCharacter(playerCharacterEntity, true, mapName, position, overrideRotation, rotation);
                // Remove this character from warping list
                playerCharacterEntity.IsWarping = false;
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Unregister character to tell central server that the user is disconnecting from this server
                UnregisterPlayerCharacter(connectionId);
                // Send message to client to warp
                MMOWarpMessage message = new MMOWarpMessage();
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Warp, message);
            }
        }
#endif

        public override void WarpCharacterToInstance(BasePlayerCharacterEntity playerCharacterEntity, string mapName, Vector3 position, bool overrideRotation, Vector3 rotation)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            if (!CanWarpCharacter(playerCharacterEntity))
                return;
            // Generate instance id
            string instanceId = GenericUtils.GetUniqueId();
            // Prepare data for warp character later when instance map server registered to this map server
            HashSet<uint> instanceMapWarpingCharacters = new HashSet<uint>();
            PartyData party;
            if (ServerPartyHandlers.TryGetParty(playerCharacterEntity.PartyId, out party))
            {
                // If character is party leader, will bring party member to join instance
                if (party.IsLeader(playerCharacterEntity.Id))
                {
                    List<BasePlayerCharacterEntity> aliveAllies = playerCharacterEntity.FindAliveEntities<BasePlayerCharacterEntity>(CurrentGameInstance.joinInstanceMapDistance, true, false, false, CurrentGameInstance.playerLayer.Mask | CurrentGameInstance.playingLayer.Mask);
                    foreach (BasePlayerCharacterEntity aliveAlly in aliveAllies)
                    {
                        if (!party.IsMember(aliveAlly.Id))
                            continue;
                        instanceMapWarpingCharacters.Add(aliveAlly.ObjectId);
                        aliveAlly.IsWarping = true;
                    }
                    instanceMapWarpingCharacters.Add(playerCharacterEntity.ObjectId);
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
                instanceMapWarpingCharacters.Add(playerCharacterEntity.ObjectId);
                playerCharacterEntity.IsWarping = true;
            }
            _instanceMapWarpingCharactersByInstanceId.TryAdd(instanceId, instanceMapWarpingCharacters);
            _instanceMapWarpingLocations.TryAdd(instanceId, new InstanceMapWarpingLocation()
            {
                mapName = mapName,
                position = position,
                overrideRotation = overrideRotation,
                rotation = rotation,
            });
            ClusterClient.SendRequest(MMORequestTypes.RequestSpawnMap, new RequestSpawnMapMessage()
            {
                mapId = mapName,
                instanceId = instanceId,
                instanceWarpPosition = position,
                instanceWarpOverrideRotation = overrideRotation,
                instanceWarpRotation = rotation,
            }, responseDelegate: (responseHandler, responseCode, response) => OnRequestSpawnMap(responseHandler, responseCode, response, instanceId), millisecondsTimeout: mapSpawnMillisecondsTimeout);
#endif
        }

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private async UniTaskVoid WarpCharacterToInstanceRoutine(BasePlayerCharacterEntity playerCharacterEntity, string instanceId)
        {
            // If warping to different map
            long connectionId = playerCharacterEntity.ConnectionId;
            CentralServerPeerInfo peerInfo;
            InstanceMapWarpingLocation warpingLocation;
            BaseMapInfo mapInfo;
            if (ServerUserHandlers.TryGetPlayerCharacter(connectionId, out _) &&
                _instanceMapWarpingLocations.TryGetValue(instanceId, out warpingLocation) &&
                _instanceMapServerConnectionIdsByInstanceId.TryGetValue(instanceId, out peerInfo) &&
                GameInstance.MapInfos.TryGetValue(warpingLocation.mapName, out mapInfo) &&
                mapInfo.IsSceneSet())
            {
                // Add this character to warping list
                playerCharacterEntity.IsWarping = true;
                // Unregister player character
                UnregisterPlayerCharacter(connectionId);
                // Wait to save character before move to instance map
                while (savingCharacters.Contains(playerCharacterEntity.Id))
                {
                    await UniTask.Yield();
                }
                await SaveCharacter(playerCharacterEntity);
                // Remove this character from warping list
                playerCharacterEntity.IsWarping = false;
                // Destroy character from server
                playerCharacterEntity.NetworkDestroy();
                // Unregister character to tell central server that the user is disconnecting from this server
                UnregisterPlayerCharacter(connectionId);
                // Send message to client to warp
                MMOWarpMessage message = new MMOWarpMessage();
                message.networkAddress = peerInfo.networkAddress;
                message.networkPort = peerInfo.networkPort;
                ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.Warp, message);
            }
        }
#endif

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        private void OnRequestSpawnMap(ResponseHandlerData requestHandler, AckResponseCode responseCode, INetSerializable response, string instanceId)
        {
            if (responseCode == AckResponseCode.Error ||
                responseCode == AckResponseCode.Timeout)
            {
                // Remove warping characters who warping to instance map
                HashSet<uint> instanceMapWarpingCharacters;
                if (_instanceMapWarpingCharactersByInstanceId.TryGetValue(instanceId, out instanceMapWarpingCharacters))
                {
                    BasePlayerCharacterEntity playerCharacterEntity;
                    foreach (uint warpingCharacter in instanceMapWarpingCharacters)
                    {
                        if (Assets.TryGetSpawnedObject(warpingCharacter, out playerCharacterEntity))
                        {
                            playerCharacterEntity.IsWarping = false;
                            ServerGameMessageHandlers.SendGameMessage(playerCharacterEntity.ConnectionId, UITextKeys.UI_ERROR_SERVICE_NOT_AVAILABLE);
                        }
                    }
                    _instanceMapWarpingCharactersByInstanceId.TryRemove(instanceId, out _);
                }
                _instanceMapWarpingLocations.TryRemove(instanceId, out _);
            }
        }
#endif
    }
}
