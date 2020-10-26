using Cysharp.Threading.Tasks;
using LiteNetLib;
using LiteNetLibManager;
using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestUserLogin(string username, string password, AckMessageCallback<ResponseUserLoginMessage> callback)
        {
            RequestUserLoginMessage message = new RequestUserLoginMessage();
            message.username = username;
            message.password = password;
            return ClientSendRequest(MMOMessageTypes.RequestUserLogin, message, callback);
        }

        public uint RequestUserRegister(string username, string password, AckMessageCallback<ResponseUserRegisterMessage> callback)
        {
            RequestUserRegisterMessage message = new RequestUserRegisterMessage();
            message.username = username;
            message.password = password;
            return ClientSendRequest(MMOMessageTypes.RequestUserRegister, message, callback);
        }

        public uint RequestUserLogout(AckMessageCallback<BaseAckMessage> callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return ClientSendRequest(MMOMessageTypes.RequestUserLogout, message, callback);
        }

        public uint RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback<ResponseValidateAccessTokenMessage> callback)
        {
            RequestValidateAccessTokenMessage message = new RequestValidateAccessTokenMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            return ClientSendRequest(MMOMessageTypes.RequestValidateAccessToken, message, callback);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserLoginRoutine(messageHandler).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestUserLoginRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestUserLoginMessage message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            ResponseUserLoginMessage.Error error = ResponseUserLoginMessage.Error.None;
            ValidateUserLoginResp validateUserLoginResp = await DbServiceClient.ValidateUserLoginAsync(new ValidateUserLoginReq()
            {
                Username = message.username,
                Password = message.password
            });
            string userId = validateUserLoginResp.UserId;
            string accessToken = string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                error = ResponseUserLoginMessage.Error.InvalidUsernameOrPassword;
                userId = string.Empty;
            }
            else if (userPeersByUserId.ContainsKey(userId) || MapContainsUser(userId))
            {
                error = ResponseUserLoginMessage.Error.AlreadyLogin;
                userId = string.Empty;
            }
            else
            {
                CentralUserPeerInfo userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userId,
                    AccessToken = accessToken
                });
            }
            ServerSendResponse(connectionId, new ResponseUserLoginMessage()
            {
                ackId = message.ackId,
                responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                error = error,
                userId = userId,
                accessToken = accessToken,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserRegisterRoutine(messageHandler).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestUserRegisterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestUserRegisterMessage message = messageHandler.ReadMessage<RequestUserRegisterMessage>();
            ResponseUserRegisterMessage.Error error = ResponseUserRegisterMessage.Error.None;
            string username = message.username;
            string password = message.password;
            FindUsernameResp findUsernameResp = await DbServiceClient.FindUsernameAsync(new FindUsernameReq()
            {
                Username = username
            });
            if (findUsernameResp.FoundAmount > 0)
                error = ResponseUserRegisterMessage.Error.UsernameAlreadyExisted;
            else if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooShortUsername;
            else if (username.Length > maxUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooLongUsername;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                error = ResponseUserRegisterMessage.Error.TooShortPassword;
            else
            {
                await DbServiceClient.CreateUserLoginAsync(new CreateUserLoginReq()
                {
                    Username = username,
                    Password = password
                });
            }
            ServerSendResponse(connectionId, new ResponseUserRegisterMessage()
            {
                ackId = message.ackId,
                responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                error = error,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserLogoutRoutine(messageHandler).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestUserLogoutRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(connectionId);
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userPeerInfo.userId,
                    AccessToken = string.Empty
                });
            }
            ServerSendResponse(connectionId, new BaseAckMessage()
            {
                ackId = message.ackId,
                responseCode = AckResponseCode.Success,
            });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected void HandleRequestValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestValidateAccessTokenRoutine(messageHandler).Forget();
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        private async UniTaskVoid HandleRequestValidateAccessTokenRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestValidateAccessTokenMessage message = messageHandler.ReadMessage<RequestValidateAccessTokenMessage>();
            ResponseValidateAccessTokenMessage.Error error = ResponseValidateAccessTokenMessage.Error.None;
            string userId = message.userId;
            string accessToken = message.accessToken;
            ValidateAccessTokenResp validateAccessTokenResp = await DbServiceClient.ValidateAccessTokenAsync(new ValidateAccessTokenReq()
            {
                UserId = userId,
                AccessToken = accessToken
            });
            if (!validateAccessTokenResp.IsPass)
            {
                error = ResponseValidateAccessTokenMessage.Error.InvalidAccessToken;
                userId = string.Empty;
                accessToken = string.Empty;
            }
            else
            {
                CentralUserPeerInfo userPeerInfo;
                if (userPeersByUserId.TryGetValue(userId, out userPeerInfo))
                {
                    userPeersByUserId.Remove(userPeerInfo.userId);
                    userPeers.Remove(userPeerInfo.connectionId);
                }
                userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userPeerInfo.userId,
                    AccessToken = accessToken
                });
            }
            ServerSendResponse(connectionId, new ResponseValidateAccessTokenMessage()
            {
                ackId = message.ackId,
                responseCode = error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                error = error,
                userId = userId,
                accessToken = accessToken,
            });
        }
#endif
    }
}
