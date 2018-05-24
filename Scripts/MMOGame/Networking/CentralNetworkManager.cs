using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLibManager;
using LiteNetLib;

namespace Insthync.MMOG
{
    public class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
        public System.Action<NetPeer> onClientConnected;
        public System.Action<NetPeer, DisconnectInfo> onClientDisconnected;
        // This server will collect servers data
        // All Map servers addresses, Login server address, Chat server address, Database server configs
        protected override void RegisterServerMessages()
        {
            base.RegisterServerMessages();
            // Receiving:
            // - Login Server Request To Store Information (Machine Address/Port)
            // - Chat Server Request To Store Information (Machine Address/Port)
            // - Map Servers Request To Store Information (Machine Address/Port)
            // - Login Server Request For Map Server Information
            // - Map Server Request For Other Map Server Information
            // - Map Server Request For Chat Server Information
        }

        protected override void RegisterClientMessages()
        {
            base.RegisterClientMessages();
            // Receiving:
            // - Chat Server Information
            // - Map Server Information
        }

        public override void OnClientConnected(NetPeer peer)
        {
            base.OnClientConnected(peer);
            if (onClientConnected != null)
                onClientConnected(peer);
        }

        public override void OnClientDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            base.OnClientDisconnected(peer, disconnectInfo);
            if (onClientDisconnected != null)
                onClientDisconnected(peer, disconnectInfo);
        }
    }
}
