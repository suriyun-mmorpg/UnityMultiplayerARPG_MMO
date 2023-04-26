using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;
using System.Net.Sockets;

namespace MultiplayerARPG.MMO
{
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
    [DefaultExecutionOrder(-897)]
#endif
    public partial class CentralNetworkManager : LiteNetLibManager.LiteNetLibManager
    {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        // User peers (Login / Register / Manager characters)
        protected readonly Dictionary<long, CentralUserPeerInfo> _userPeers = new Dictionary<long, CentralUserPeerInfo>();
        protected readonly Dictionary<string, CentralUserPeerInfo> _userPeersByUserId = new Dictionary<string, CentralUserPeerInfo>();
#endif
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Cluster")]
#endif
        public int clusterServerPort = 6010;

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Map Spawn")]
#endif
        public int mapSpawnMillisecondsTimeout = 0;

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("User Account")]
#endif
        public bool disableDefaultLogin = false;
        public int minUsernameLength = 2;
        public int maxUsernameLength = 24;
        public int minPasswordLength = 2;
        public int minCharacterNameLength = 2;
        public int maxCharacterNameLength = 16;
        public bool requireEmail = false;
        public bool requireEmailVerification = false;

#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        [Header("Statistic")]
#endif
        public float updateUserCountInterval = 5f;

        public System.Action onClientConnected;
        public System.Action<DisconnectReason, SocketError, UITextKeys> onClientDisconnected;
        public System.Action onClientStopped;

        private float _lastUserCountUpdateTime = float.MinValue;

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public ClusterServer ClusterServer { get; private set; }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        protected override void Start()
        {
            base.Start();
            ClusterServer = new ClusterServer(this);
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (IsServer)
            {
                ClusterServer.Update();
                if (Time.unscaledTime - _lastUserCountUpdateTime > updateUserCountInterval)
                {
                    _lastUserCountUpdateTime = Time.unscaledTime;
                    UpdateCountUsers().Forget();
                }
            }
        }

        protected async UniTaskVoid UpdateCountUsers()
        {
            // Update user count
            await DbServiceClient.UpdateUserCount(new UpdateUserCountReq()
            {
                UserCount = ClusterServer.MapUsersByCharacterId.Count,
            });
        }
#endif

        protected override void RegisterMessages()
        {
            EnableRequestResponse(MMOMessageTypes.Request, MMOMessageTypes.Response);
            // Requests
            RegisterRequestToServer<RequestUserLoginMessage, ResponseUserLoginMessage>(MMORequestTypes.RequestUserLogin, HandleRequestUserLogin);
            RegisterRequestToServer<RequestUserRegisterMessage, ResponseUserRegisterMessage>(MMORequestTypes.RequestUserRegister, HandleRequestUserRegister);
            RegisterRequestToServer<EmptyMessage, EmptyMessage>(MMORequestTypes.RequestUserLogout, HandleRequestUserLogout);
            RegisterRequestToServer<EmptyMessage, ResponseCharactersMessage>(MMORequestTypes.RequestCharacters, HandleRequestCharacters);
            RegisterRequestToServer<RequestCreateCharacterMessage, ResponseCreateCharacterMessage>(MMORequestTypes.RequestCreateCharacter, HandleRequestCreateCharacter);
            RegisterRequestToServer<RequestDeleteCharacterMessage, ResponseDeleteCharacterMessage>(MMORequestTypes.RequestDeleteCharacter, HandleRequestDeleteCharacter);
            RegisterRequestToServer<RequestSelectCharacterMessage, ResponseSelectCharacterMessage>(MMORequestTypes.RequestSelectCharacter, HandleRequestSelectCharacter);
            RegisterRequestToServer<RequestValidateAccessTokenMessage, ResponseValidateAccessTokenMessage>(MMORequestTypes.RequestValidateAccessToken, HandleRequestValidateAccessToken);
            // Client messages
            RegisterClientMessage(GameMsgTypes.Disconnect, HandleServerDisconnect);
            // Keeping `RegisterClientMessages` and `RegisterServerMessages` for backward compatibility, can use any of below dev extension methods
            this.InvokeInstanceDevExtMethods("RegisterClientMessages");
            this.InvokeInstanceDevExtMethods("RegisterServerMessages");
            this.InvokeInstanceDevExtMethods("RegisterMessages");
        }

        public async void KickClient(long connectionId, byte[] data)
        {
            if (!IsServer)
                return;
            ServerSendPacket(connectionId, 0, DeliveryMethod.ReliableOrdered, GameMsgTypes.Disconnect, (writer) => writer.PutBytesWithLength(data));
            await UniTask.Delay(500);
            ServerTransport.ServerDisconnect(connectionId);
        }

        public void KickClient(long connectionId, UITextKeys message)
        {
            if (!IsServer)
                return;
            NetDataWriter writer = new NetDataWriter();
            writer.PutPackedUShort((ushort)message);
            KickClient(connectionId, writer.Data);
        }

        protected void HandleServerDisconnect(MessageHandlerData messageHandler)
        {
            Client.SetDisconnectData(messageHandler.Reader.GetBytesWithLength());
        }

        protected virtual void Clean()
        {
            this.InvokeInstanceDevExtMethods("Clean");
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            _userPeers.Clear();
            _userPeersByUserId.Clear();
#endif
        }

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public override void OnStartServer()
        {
            this.InvokeInstanceDevExtMethods("OnStartServer");
            base.OnStartServer();
            ClusterServer.StartServer();
        }
#endif

#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
        public override void OnStopServer()
        {
            Clean();
            base.OnStopServer();
            ClusterServer.StopServer();
        }
#endif

        public override void OnStartClient(LiteNetLibClient client)
        {
            this.InvokeInstanceDevExtMethods("OnStartClient", client);
            base.OnStartClient(client);
        }

        public override void OnStopClient()
        {
            if (!IsServer)
                Clean();
            base.OnStopClient();
            if (onClientStopped != null)
                onClientStopped.Invoke();
        }

        public override void OnClientConnected()
        {
            base.OnClientConnected();
            if (onClientConnected != null)
                onClientConnected.Invoke();
        }

        public override void OnClientDisconnected(DisconnectReason reason, SocketError socketError, byte[] data)
        {
            UITextKeys message = UITextKeys.NONE;
            if (data != null && data.Length > 0)
            {
                NetDataReader reader = new NetDataReader(data);
                message = (UITextKeys)reader.GetPackedUShort();
            }
            if (onClientDisconnected != null)
                onClientDisconnected.Invoke(reason, socketError, message);
        }

        public override void OnPeerDisconnected(long connectionId, DisconnectReason reason, SocketError socketError)
        {
            base.OnPeerDisconnected(connectionId, reason, socketError);
            RemoveUserPeerByConnectionId(connectionId, out _);
        }

        public bool RemoveUserPeerByConnectionId(long connectionId, out CentralUserPeerInfo userPeerInfo)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            if (_userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                _userPeersByUserId.Remove(userPeerInfo.userId);
                _userPeers.Remove(connectionId);
                return true;
            }
            return false;
#else
            userPeerInfo = default;
            return false;
#endif
        }

        public bool RemoveUserPeerByUserId(string userId, out CentralUserPeerInfo userPeerInfo)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            if (_userPeersByUserId.TryGetValue(userId, out userPeerInfo))
            {
                _userPeersByUserId.Remove(userPeerInfo.userId);
                _userPeers.Remove(userPeerInfo.connectionId);
                return true;
            }
            return false;
#else
            userPeerInfo = default;
            return false;
#endif
        }

        public bool MapContainsUser(string userId)
        {
#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
            return ClusterServer.MapContainsUser(userId);
#else
            return false;
#endif
        }
    }
}
