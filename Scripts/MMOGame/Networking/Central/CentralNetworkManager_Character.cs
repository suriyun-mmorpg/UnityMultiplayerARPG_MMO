using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestCharacters(AckMessageCallback callback)
        {
            var message = new RequestCharactersMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestCharacters, message, callback);
        }

        public uint RequestCreateCharacter(string characterName, int dataId, int entityId, AckMessageCallback callback)
        {
            var message = new RequestCreateCharacterMessage();
            message.characterName = characterName;
            message.dataId = dataId;
            message.entityId = entityId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestCreateCharacter, message, callback);
        }

        public uint RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            var message = new RequestDeleteCharacterMessage();
            message.characterId = characterId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestDeleteCharacter, message, callback);
        }

        public uint RequestSelectCharacter(string characterId, AckMessageCallback callback)
        {
            var message = new RequestSelectCharacterMessage();
            message.characterId = characterId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestSelectCharacter, message, callback);
        }

        protected void HandleRequestCharacters(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCharactersRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCharactersRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCharactersMessage>();
            var error = ResponseCharactersMessage.Error.None;
            List<PlayerCharacterData> characters = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseCharactersMessage.Error.NotLoggedin;
            else
            {
                var job = new ReadCharactersJob(Database, userPeerInfo.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                characters = job.result;
            }
            var responseMessage = new ResponseCharactersMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCharactersMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.characters = characters;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseCharacters, responseMessage);
        }

        protected void HandleRequestCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCreateCharacterRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCreateCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestCreateCharacterMessage>();
            var error = ResponseCreateCharacterMessage.Error.None;
            var characterName = message.characterName;
            var dataId = message.dataId;
            var entityId = message.entityId;
            CentralUserPeerInfo userPeerInfo;
            var findCharacterNameJob = new FindCharacterNameJob(Database, characterName);
            findCharacterNameJob.Start();
            yield return StartCoroutine(findCharacterNameJob.WaitFor());
            if (findCharacterNameJob.result > 0)
                error = ResponseCreateCharacterMessage.Error.CharacterNameAlreadyExisted;
            else if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseCreateCharacterMessage.Error.NotLoggedin;
            else if (string.IsNullOrEmpty(characterName) || characterName.Length < minCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooShortCharacterName;
            else if (characterName.Length > maxCharacterNameLength)
                error = ResponseCreateCharacterMessage.Error.TooLongCharacterName;
            else if (!GameInstance.PlayerCharacters.ContainsKey(dataId) ||
                !GameInstance.PlayerCharacterEntities.ContainsKey(entityId))
                error = ResponseCreateCharacterMessage.Error.InvalidData;
            else
            {
                var characterId = GenericUtils.GetUniqueId();
                var characterData = new PlayerCharacterData();
                characterData.Id = characterId;
                characterData.SetNewPlayerCharacterData(characterName, dataId, entityId);
                var createCharacterJob = new CreateCharacterJob(Database, userPeerInfo.userId, characterData);
                createCharacterJob.Start();
                yield return StartCoroutine(createCharacterJob.WaitFor());
            }
            var responseMessage = new ResponseCreateCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseCreateCharacter, responseMessage);
        }

        protected void HandleRequestDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestDeleteCharacterRoutine(messageHandler));
        }

        private IEnumerator HandleRequestDeleteCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestDeleteCharacterMessage>();
            var error = ResponseDeleteCharacterMessage.Error.None;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseDeleteCharacterMessage.Error.NotLoggedin;
            else
            {
                var job = new DeleteCharactersJob(Database, userPeerInfo.userId, message.characterId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
            }
            var responseMessage = new ResponseDeleteCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseDeleteCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseDeleteCharacter, responseMessage);
        }

        protected void HandleRequestSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestSelectCharacterRoutine(messageHandler));
        }

        private IEnumerator HandleRequestSelectCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestSelectCharacterMessage>();
            var error = ResponseSelectCharacterMessage.Error.None;
            CentralServerPeerInfo mapServerPeerInfo = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseSelectCharacterMessage.Error.NotLoggedin;
            else
            {
                var job = new ReadCharacterJob(Database, userPeerInfo.userId, message.characterId, false, false, false, false, false, false, false, false);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                var character = job.result;
                if (character == null)
                    error = ResponseSelectCharacterMessage.Error.InvalidCharacterData;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = ResponseSelectCharacterMessage.Error.MapNotReady;
            }
            var responseMessage = new ResponseSelectCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseSelectCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            if (mapServerPeerInfo != null)
            {
                responseMessage.sceneName = mapServerPeerInfo.extra;
                responseMessage.networkAddress = mapServerPeerInfo.networkAddress;
                responseMessage.networkPort = mapServerPeerInfo.networkPort;
                responseMessage.connectKey = mapServerPeerInfo.connectKey;
            }
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseSelectCharacter, responseMessage);
        }

        protected void HandleResponseCharacters(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCharactersMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseCreateCharacterMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseDeleteCharacterMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseSelectCharacterMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }
    }
}
