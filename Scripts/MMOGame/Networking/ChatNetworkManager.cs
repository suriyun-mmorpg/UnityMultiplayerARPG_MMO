using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using System;
using LiteNetLib;

namespace Insthync.MMOG
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
        private readonly Dictionary<long, HashSet<string>> mapCharacterNames = new Dictionary<long, HashSet<string>>();
        private readonly Dictionary<string, NetPeer> peersByCharacterName = new Dictionary<string, NetPeer>();

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            RegisterClientMessage(MMOMessageTypes.Chat, HandleChatAtClient);
        }

        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            RegisterServerMessage(MMOMessageTypes.Chat, HandleChatAtServer);
            RegisterServerMessage(MMOMessageTypes.UpdateMapUser, HandleUpdateMapUser);
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

        protected override void OnDestroy()
        {
            CentralAppServerRegister.Stop();
            base.OnDestroy();
        }

        public override void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnPeerDisconnected(peer, disconnectInfo);
            mapCharacterNames.Remove(peer.ConnectId);
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
            // When receive chat message from chat server at map server
            if (mapNetworkManager != null)
                mapNetworkManager.OnChatMessageReceive(message);
        }

        private void HandleChatAtServer(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<ChatMessage>();
            switch (message.channel)
            {
                case ChatChannel.Global:
                case ChatChannel.Party:
                case ChatChannel.Guild:
                    // Send message to all map servers, let's map servers filter messages
                    SendPacketToAllPeers(SendOptions.ReliableOrdered, MMOMessageTypes.Chat, message);
                    break;
                case ChatChannel.Whisper:
                    NetPeer receiverPeer;
                    // Send message to map server which have the character
                    if (!string.IsNullOrEmpty(message.receiver) &&
                        peersByCharacterName.TryGetValue(message.receiver, out receiverPeer))
                        LiteNetLibPacketSender.SendPacket(SendOptions.ReliableOrdered, receiverPeer, MMOMessageTypes.Chat, message);
                    break;
            }
        }

        private void HandleUpdateMapUser(LiteNetLibMessageHandler messageHandler)
        {
            var peer = messageHandler.peer;
            var message = messageHandler.ReadMessage<UpdateMapUserMessage>();
            if (mapCharacterNames.ContainsKey(peer.ConnectId))
            {
                switch (message.type)
                {
                    case UpdateMapUserMessage.UpdateType.Add:
                        if (!mapCharacterNames[peer.ConnectId].Contains(message.userData.characterName))
                        {
                            mapCharacterNames[peer.ConnectId].Add(message.userData.characterName);
                            peersByCharacterName[message.userData.characterName] = peer;
                            Debug.Log("[Chat] Add map user: " + message.userData.userId + " by " + peer.ConnectId);
                        }
                        break;
                    case UpdateMapUserMessage.UpdateType.Remove:
                        mapCharacterNames[peer.ConnectId].Remove(message.userData.characterName);
                        peersByCharacterName.Remove(message.userData.characterName);
                        Debug.Log("[Chat] Remove map user: " + message.userData.userId + " by " + peer.ConnectId);
                        break;
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
