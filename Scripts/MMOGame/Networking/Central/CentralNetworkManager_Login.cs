using LiteNetLib;
using LiteNetLibManager;
using System;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestUserLogin(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserLoginMessage();
            message.username = username;
            message.password = password;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestUserLogin, message, callback);
        }

        public uint RequestUserRegister(string username, string password, AckMessageCallback callback)
        {
            var message = new RequestUserRegisterMessage();
            message.username = username;
            message.password = password;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestUserRegister, message, callback);
        }

        public uint RequestUserLogout(AckMessageCallback callback)
        {
            var message = new BaseAckMessage();
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestUserLogout, message, callback);
        }

        public uint RequestValidateAccessToken(string userId, string accessToken, AckMessageCallback callback)
        {
            var message = new RequestValidateAccessTokenMessage();
            message.userId = userId;
            message.accessToken = accessToken;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestValidateAccessToken, message, callback);
        }

        public uint RequestFacebookLogin(string id, string accessToken, AckMessageCallback callback)
        {
            var message = new RequestFacebookLoginMessage();
            message.id = id;
            message.accessToken = accessToken;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestFacebookLogin, message, callback);
        }

        public uint RequestGooglePlayLogin(string idToken, AckMessageCallback callback)
        {
            var message = new RequestGooglePlayLoginMessage();
            message.idToken = idToken;
            return Client.ClientSendAckPacket(SendOptions.ReliableOrdered, MMOMessageTypes.RequestGooglePlayLogin, message, callback);
        }

        protected async void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var userId = await Database.ValidateUserLogin(message.username, message.password);
            var accessToken = string.Empty;
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
                var userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected async void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestUserRegisterMessage>();
            var error = ResponseUserRegisterMessage.Error.None;
            var username = message.username;
            var password = message.password;
            if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooShortUsername;
            else if (username.Length > maxUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooLongUsername;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                error = ResponseUserRegisterMessage.Error.TooShortPassword;
            else if (await Database.FindUsername(username) > 0)
                error = ResponseUserRegisterMessage.Error.UsernameAlreadyExisted;
            else
                await Database.CreateUserLogin(username, password);
            var responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserRegister, responseMessage);
        }

        protected async void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(connectionId);
                await Database.UpdateAccessToken(userPeerInfo.userId, string.Empty);
            }
            var responseMessage = new BaseAckMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogout, responseMessage);
        }

        protected async void HandleRequestValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestValidateAccessTokenMessage>();
            var error = ResponseValidateAccessTokenMessage.Error.None;
            var userId = message.userId;
            var accessToken = message.accessToken;
            if (!await Database.ValidateAccessToken(userId, accessToken))
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
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseValidateAccessTokenMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseValidateAccessToken, responseMessage);
        }

        protected async void HandleRequestFacebookLogin(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestFacebookLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var userId = await Database.FacebookLogin(message.id, message.accessToken);
            var accessToken = string.Empty;
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
                var userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected async void HandleRequestGooglePlayLogin(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestGooglePlayLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var userId = await Database.GooglePlayLogin(message.idToken);
            var accessToken = string.Empty;
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
                var userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await Database.UpdateAccessToken(userId, accessToken);
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleResponseUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseUserLoginMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseUserRegisterMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }

        protected void HandleResponseValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            var transportHandler = messageHandler.transportHandler;
            var message = messageHandler.ReadMessage<ResponseValidateAccessTokenMessage>();
            var ackId = message.ackId;
            transportHandler.TriggerAck(ackId, message.responseCode, message);
        }
    }
}
