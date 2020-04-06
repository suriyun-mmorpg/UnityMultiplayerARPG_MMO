using LiteNetLib;
using LiteNetLibManager;
using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            RequestUserLoginMessage message = new RequestUserLoginMessage();
            message.username = username;
            message.password = password;
            return ClientSendRequest(MMOMessageTypes.RequestUserLogin, message, callback);
        }

        public uint RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            RequestUserRegisterMessage message = new RequestUserRegisterMessage();
            message.username = username;
            message.password = password;
            return ClientSendRequest(MMOMessageTypes.RequestUserRegister, message, callback);
        }

        public uint RequestUserLogout(AckMessageCallback callback)
        {
            BaseAckMessage message = new BaseAckMessage();
            return ClientSendRequest(MMOMessageTypes.RequestUserLogout, message, callback);
        }

        public uint RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback callback)
        {
            RequestValidateAccessTokenMessage message = new RequestValidateAccessTokenMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            return ClientSendRequest(MMOMessageTypes.RequestValidateAccessToken, message, callback);
        }

        protected void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserLoginRoutine(messageHandler);
        }

        private async void HandleRequestUserLoginRoutine(LiteNetLibMessageHandler messageHandler)
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
            ResponseUserLoginMessage responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserRegisterRoutine(messageHandler);
        }

        private async void HandleRequestUserRegisterRoutine(LiteNetLibMessageHandler messageHandler)
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
            ResponseUserRegisterMessage responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseUserRegister, responseMessage);
        }

        protected void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestUserLogoutRoutine(messageHandler);
        }

        private async void HandleRequestUserLogoutRoutine(LiteNetLibMessageHandler messageHandler)
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
            BaseAckMessage responseMessage = new BaseAckMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseUserLogout, responseMessage);
        }

        protected void HandleRequestValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestValidateAccessTokenRoutine(messageHandler);
        }

        private async void HandleRequestValidateAccessTokenRoutine(LiteNetLibMessageHandler messageHandler)
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
            ResponseValidateAccessTokenMessage responseMessage = new ResponseValidateAccessTokenMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseValidateAccessToken, responseMessage);
        }

        protected void HandleResponseUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseUserLoginMessage message = messageHandler.ReadMessage<ResponseUserLoginMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseUserRegisterMessage message = messageHandler.ReadMessage<ResponseUserRegisterMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            BaseAckMessage message = messageHandler.ReadMessage<BaseAckMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }

        protected void HandleResponseValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            TransportHandler transportHandler = messageHandler.transportHandler;
            ResponseValidateAccessTokenMessage message = messageHandler.ReadMessage<ResponseValidateAccessTokenMessage>();
            transportHandler.ReadResponse(message.ackId, message.responseCode, message);
        }
    }
}
