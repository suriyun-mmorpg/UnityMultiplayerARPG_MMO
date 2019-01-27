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
            RequestCharactersMessage message = new RequestCharactersMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestCharacters, message, callback);
        }

        public uint RequestCreateCharacter(string characterName, int dataId, int entityId, byte[] extra, AckMessageCallback callback)
        {
            RequestCreateCharacterMessage message = new RequestCreateCharacterMessage();
            message.characterName = characterName;
            message.dataId = dataId;
            message.entityId = entityId;
            message.extra = extra;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestCreateCharacter, message, callback);
        }

        public uint RequestDeleteCharacter(string characterId, AckMessageCallback callback)
        {
            RequestDeleteCharacterMessage message = new RequestDeleteCharacterMessage();
            message.characterId = characterId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestDeleteCharacter, message, callback);
        }

        public uint RequestSelectCharacter(string characterId, AckMessageCallback callback)
        {
            RequestSelectCharacterMessage message = new RequestSelectCharacterMessage();
            message.characterId = characterId;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestSelectCharacter, message, callback);
        }

        protected void HandleRequestCharacters(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestCharactersRoutine(messageHandler));
        }

        private IEnumerator HandleRequestCharactersRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestCharactersMessage message = messageHandler.ReadMessage<RequestCharactersMessage>();
            ResponseCharactersMessage.Error error = ResponseCharactersMessage.Error.None;
            List<PlayerCharacterData> characters = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseCharactersMessage.Error.NotLoggedin;
            else
            {
                ReadCharactersJob job = new ReadCharactersJob(Database, userPeerInfo.userId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                characters = job.result;
            }
            ResponseCharactersMessage responseMessage = new ResponseCharactersMessage();
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
            long connectionId = messageHandler.connectionId;
            RequestCreateCharacterMessage message = messageHandler.ReadMessage<RequestCreateCharacterMessage>();
            ResponseCreateCharacterMessage.Error error = ResponseCreateCharacterMessage.Error.None;
            string characterName = message.characterName;
            int dataId = message.dataId;
            int entityId = message.entityId;
            byte[] extra = message.extra;
            CentralUserPeerInfo userPeerInfo;
            FindCharacterNameJob findCharacterNameJob = new FindCharacterNameJob(Database, characterName);
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
                string characterId = GenericUtils.GetUniqueId();
                PlayerCharacterData characterData = new PlayerCharacterData();
                characterData.Id = characterId;
                characterData.SetNewPlayerCharacterData(characterName, dataId, entityId);
                ApplyCreateCharacterExtra(characterData, extra);
                CreateCharacterJob createCharacterJob = new CreateCharacterJob(Database, userPeerInfo.userId, characterData);
                createCharacterJob.Start();
                yield return StartCoroutine(createCharacterJob.WaitFor());
            }
            ResponseCreateCharacterMessage responseMessage = new ResponseCreateCharacterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseCreateCharacterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseCreateCharacter, responseMessage);
        }

        private void ApplyCreateCharacterExtra(PlayerCharacterData characterData, byte[] extra)
        {
            this.InvokeInstanceDevExtMethods("ApplyCreateCharacterExtra", characterData, extra);
        }

        protected void HandleRequestDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestDeleteCharacterRoutine(messageHandler));
        }

        private IEnumerator HandleRequestDeleteCharacterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestDeleteCharacterMessage message = messageHandler.ReadMessage<RequestDeleteCharacterMessage>();
            ResponseDeleteCharacterMessage.Error error = ResponseDeleteCharacterMessage.Error.None;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseDeleteCharacterMessage.Error.NotLoggedin;
            else
            {
                DeleteCharactersJob job = new DeleteCharactersJob(Database, userPeerInfo.userId, message.characterId);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
            }
            ResponseDeleteCharacterMessage responseMessage = new ResponseDeleteCharacterMessage();
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
            long connectionId = messageHandler.connectionId;
            RequestSelectCharacterMessage message = messageHandler.ReadMessage<RequestSelectCharacterMessage>();
            ResponseSelectCharacterMessage.Error error = ResponseSelectCharacterMessage.Error.None;
            CentralServerPeerInfo mapServerPeerInfo = null;
            CentralUserPeerInfo userPeerInfo;
            if (!userPeers.TryGetValue(connectionId, out userPeerInfo))
                error = ResponseSelectCharacterMessage.Error.NotLoggedin;
            else
            {
                ReadCharacterJob job = new ReadCharacterJob(Database, userPeerInfo.userId, message.characterId, false, false, false, false, false, false, false, false, false);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                PlayerCharacterData character = job.result;
                if (character == null)
                    error = ResponseSelectCharacterMessage.Error.InvalidCharacterData;
                else if (!mapServerPeersBySceneName.TryGetValue(character.CurrentMapName, out mapServerPeerInfo))
                    error = ResponseSelectCharacterMessage.Error.MapNotReady;
            }
            ResponseSelectCharacterMessage responseMessage = new ResponseSelectCharacterMessage();
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
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCharactersMessage message = messageHandler.ReadMessage<ResponseCharactersMessage>();
            uint ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseCreateCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseCreateCharacterMessage message = messageHandler.ReadMessage<ResponseCreateCharacterMessage>();
            uint ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseDeleteCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseDeleteCharacterMessage message = messageHandler.ReadMessage<ResponseDeleteCharacterMessage>();
            uint ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseSelectCharacter(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseSelectCharacterMessage message = messageHandler.ReadMessage<ResponseSelectCharacterMessage>();
            uint ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }
    }
}
