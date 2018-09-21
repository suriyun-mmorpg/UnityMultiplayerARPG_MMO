using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using System;
using LiteNetLib;

namespace MultiplayerARPG.MMO
{
    public class ChatNetworkManager : LiteNetLibManager.LiteNetLibManager, IAppServer
    {
        [Header("Central Network Connection")]
        public string centralConnectKey = "SampleConnectKey";
        public string centralNetworkAddress = "127.0.0.1";
        public int centralNetworkPort = 6000;
        public string machineAddress = "127.0.0.1";

        private CentralAppServerRegister cacheCentralAppServerRegister;
        public CentralAppServerRegister CentralAppServerRegister
        {
            get
            {
                if (cacheCentralAppServerRegister == null)
                    cacheCentralAppServerRegister = new CentralAppServerRegister(this);
                return cacheCentralAppServerRegister;
            }
        }

        public string CentralNetworkAddress { get { return centralNetworkAddress; } }
        public int CentralNetworkPort { get { return centralNetworkPort; } }
        public string CentralConnectKey { get { return centralConnectKey; } }
        public string AppAddress { get { return machineAddress; } }
        public int AppPort { get { return networkPort; } }
        public string AppConnectKey { get { return connectKey; } }
        public string AppExtra { get { return string.Empty; } }
        public CentralServerPeerType PeerType { get { return CentralServerPeerType.Chat; } }
        private MapNetworkManager mapNetworkManager;
        private readonly Dictionary<long, NetPeer> mapServerPeers = new Dictionary<long, NetPeer>();
        private readonly Dictionary<string, NetPeer> peersByCharacterId = new Dictionary<string, NetPeer>();
        private readonly Dictionary<string, NetPeer> peersByCharacterName = new Dictionary<string, NetPeer>();

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(MMOMessageTypes.Chat, HandleChatAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtClient);
            RegisterClientMessage(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtClient);
        }

        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(MMOMessageTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
            RegisterServerMessage(MMOMessageTypes.UpdatePartyMember, HandleUpdatePartyMemberAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateParty, HandleUpdatePartyAtServer);
        }

        public override void OnStartServer()
        {
            CentralAppServerRegister.OnStartServer();
            base.OnStartServer();
        }

        public override void OnStopServer()
        {
            CentralAppServerRegister.OnStopServer();
            base.OnStopServer();
        }

        protected override void Update()
        {
            base.Update();
            if (IsServer)
                CentralAppServerRegister.PollEvents();
        }

        protected override void OnDestroy()
        {
            CentralAppServerRegister.Stop();
            base.OnDestroy();
        }

        public override void OnPeerConnected(NetPeer peer)
        {
            base.OnPeerConnected(peer);
            mapServerPeers[peer.ConnectId] = peer;
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            mapServerPeers.Remove(peer.ConnectId);
        }

        public override void OnClientConnected(NetPeer peer)
        {
            base.OnClientConnected(peer);
            // Send map users to chat server from map server
            if (mapNetworkManager != null)
                mapNetworkManager.OnChatServerConnected();
        }

        private void HandleChatAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var message = messageHandler.ReadMessage<ChatMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnChatMessageReceive(message);
        }

        private void HandleUpdatePartyMemberAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdatePartyMemberMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdatePartyMember(message);
        }

        private void HandleUpdatePartyAtClient(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (mapNetworkManager != null)
                mapNetworkManager.OnUpdateParty(message);
        }

        private void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ChatMessage>();
            Debug.Log("Handle chat: " + message.channel + " sender: " + message.sender + " receiver: " + message.receiver + " message: " + message.message);
            switch (message.channel)
            {
                case ChatChannel.Global:
                case ChatChannel.Party:
                case ChatChannel.Guild:
                    // Send message to all map servers, let's map servers filter messages
                    SendPacketToAllPeers(SendOptions.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
                case ChatChannel.Whisper:
                    NetPeer senderPeer = null;
                    NetPeer receiverPeer = null;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.sender) &&
                        peersByCharacterName.TryGetValue(message.sender, out senderPeer))
                        LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, senderPeer, MMOMessageTypes.Chat, message);
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        peersByCharacterName.TryGetValue(message.receiver, out receiverPeer) &&
                        (senderPeer == null || receiverPeer != senderPeer))
                        LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, receiverPeer, MMOMessageTypes.Chat, message);
                    break;
            }
        }

        private void HandleUpdateMapUser(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdateMapUserMessage>();
            if (mapServerPeers.ContainsKey(peer.ConnectId))
            {
                switch (message.type)
                {
                    case UpdateMapUserMessage.UpdateType.Add:
                            peersByCharacterId[message.userData.characterId] = peer;
                            peersByCharacterName[message.userData.characterName] = peer;
                            Debug.Log("[Chat] Add map user: " + message.userData.userId + " by " + peer.ConnectId);
                        break;
                    case UpdateMapUserMessage.UpdateType.Remove:
                            peersByCharacterId.Remove(message.userData.characterId);
                            peersByCharacterName.Remove(message.userData.characterName);
                            Debug.Log("[Chat] Remove map user: " + message.userData.userId + " by " + peer.ConnectId);
                        break;
                }
            }
        }

        private void HandleUpdatePartyMemberAtServer(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdatePartyMemberMessage>();
            if (mapServerPeers.ContainsKey(peer.ConnectId))
            {
                foreach (var mapServerPeer in mapServerPeers.Values)
                {
                    LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, mapServerPeer, MMOMessageTypes.UpdatePartyMember, message);
                }
            }
        }

        private void HandleUpdatePartyAtServer(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdatePartyMessage>();
            if (mapServerPeers.ContainsKey(peer.ConnectId))
            {
                foreach (var mapServerPeer in mapServerPeers.Values)
                {
                    LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, mapServerPeer, MMOMessageTypes.UpdateParty, message);
                }
            }
        }

        public void EnterChat(ChatChannel channel, string message, string senderName, string receiverName)
        {
            if (!IsClientConnected)
                return;
            // Send chat message to server
            var chatMessage = new ChatMessage();
            chatMessage.channel = channel;
            chatMessage.message = message;
            chatMessage.sender = senderName;
            chatMessage.receiver = receiverName;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.Chat, chatMessage);
        }

        public void UpdatePartyMemberAdd(int id, string characterId, string characterName, int dataId, int level)
        {
            var message = new UpdatePartyMemberMessage();
            message.type = UpdatePartyMemberMessage.UpdateType.Add;
            message.id = id;
            message.characterId = characterId;
            message.characterName = characterName;
            message.dataId = dataId;
            message.level = level;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.UpdatePartyMember, message);
        }

        public void UpdatePartyMemberRemove(int id, string characterId)
        {
            var message = new UpdatePartyMemberMessage();
            message.type = UpdatePartyMemberMessage.UpdateType.Remove;
            message.id = id;
            message.characterId = characterId;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.UpdatePartyMember, message);
        }

        public void UpdatePartyMemberOnline(int id, string characterId, string characterName, int dataId, int level, int currentHp, int maxHp, int currentMp, int maxMp)
        {
            var message = new UpdatePartyMemberMessage();
            message.type = UpdatePartyMemberMessage.UpdateType.Online;
            message.id = id;
            message.characterId = characterId;
            message.characterName = characterName;
            message.dataId = dataId;
            message.level = level;
            message.currentHp = currentHp;
            message.maxHp = maxHp;
            message.currentMp = currentMp;
            message.maxMp = maxMp;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.UpdatePartyMember, message);
        }

        public void UpdatePartySetting(int id, bool shareExp, bool shareItem)
        {
            var message = new UpdatePartyMessage();
            message.type = UpdatePartyMessage.UpdateType.Setting;
            message.id = id;
            message.shareExp = shareExp;
            message.shareItem = shareItem;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.UpdateParty, message);
        }

        public void UpdatePartyTerminate(int id)
        {
            var message = new UpdatePartyMessage();
            message.type = UpdatePartyMessage.UpdateType.Terminate;
            message.id = id;
            LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, Client.Peer, MMOMessageTypes.UpdateParty, message);
        }

        public void StartClient(MapNetworkManager mapNetworkManager, string networkAddress, int networkPort, string connectKey)
        {
            // Start client as map server
            this.mapNetworkManager = mapNetworkManager;
            base.StartClient(networkAddress, networkPort, connectKey);
        }

        public override void OnStopClient()
        {
            base.OnStopClient();
            mapNetworkManager = null;
        }
    }
}
