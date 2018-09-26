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

        protected void HandleRequestUserLogin(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestUserLoginRoutine(messageHandler));
        }

        private IEnumerator HandleRequestUserLoginRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestUserLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var validateJob = new ValidateUserLoginJob(Database, message.username, message.password);
            validateJob.Start();
            yield return StartCoroutine(validateJob.WaitFor());
            var userId = validateJob.result;
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
                var updateAccessTokenJob = new UpdateAccessTokenJob(Database, userId, accessToken);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleRequestUserRegister(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestUserRegisterRoutine(messageHandler));
        }

        private IEnumerator HandleRequestUserRegisterRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestUserRegisterMessage>();
            var error = ResponseUserRegisterMessage.Error.None;
            var username = message.username;
            var password = message.password;
            var findUsernameJob = new FindUsernameJob(Database, username);
            findUsernameJob.Start();
            yield return StartCoroutine(findUsernameJob.WaitFor());
            if (findUsernameJob.result > 0)
                error = ResponseUserRegisterMessage.Error.UsernameAlreadyExisted;
            else if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooShortUsername;
            else if (username.Length > maxUsernameLength)
                error = ResponseUserRegisterMessage.Error.TooLongUsername;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                error = ResponseUserRegisterMessage.Error.TooShortPassword;
            else
            {
                var createUserLoginJob = new CreateUserLoginJob(Database, username, password);
                createUserLoginJob.Start();
                yield return StartCoroutine(createUserLoginJob.WaitFor());
            }
            var responseMessage = new ResponseUserRegisterMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserRegister, responseMessage);
        }

        protected void HandleRequestUserLogout(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestUserLogoutRoutine(messageHandler));
        }

        private IEnumerator HandleRequestUserLogoutRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<BaseAckMessage>();
            CentralUserPeerInfo userPeerInfo;
            if (userPeers.TryGetValue(connectionId, out userPeerInfo))
            {
                userPeersByUserId.Remove(userPeerInfo.userId);
                userPeers.Remove(connectionId);
                var updateAccessTokenJob = new UpdateAccessTokenJob(Database, userPeerInfo.userId, string.Empty);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
            }
            var responseMessage = new BaseAckMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = AckResponseCode.Success;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogout, responseMessage);
        }

        protected void HandleRequestValidateAccessToken(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestValidateAccessTokenRoutine(messageHandler));
        }

        private IEnumerator HandleRequestValidateAccessTokenRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestValidateAccessTokenMessage>();
            var error = ResponseValidateAccessTokenMessage.Error.None;
            var userId = message.userId;
            var accessToken = message.accessToken;
            var validateAccessTokenJob = new ValidateAccessTokenJob(Database, userId, accessToken);
            validateAccessTokenJob.Start();
            yield return StartCoroutine(validateAccessTokenJob.WaitFor());
            if (!validateAccessTokenJob.result)
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
                var updateAccessTokenJob = new UpdateAccessTokenJob(Database, userId, accessToken);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
            }
            var responseMessage = new ResponseValidateAccessTokenMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseValidateAccessToken, responseMessage);
        }

        protected void HandleRequestFacebookLogin(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestFacebookLoginRoutine(messageHandler));
        }

        private IEnumerator HandleRequestFacebookLoginRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestFacebookLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var job = new FacebookLoginJob(Database, message.id, message.accessToken);
            job.Start();
            yield return StartCoroutine(job.WaitFor());
            var userId = job.result;
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
                var updateAccessTokenJob = new UpdateAccessTokenJob(Database, userId, accessToken);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
            }
            var responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, SendOptions.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }

        protected void HandleRequestGooglePlayLogin(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestGooglePlayLoginRoutine(messageHandler));
        }

        IEnumerator HandleRequestGooglePlayLoginRoutine(LiteNetLibMessageHandler messageHandler)
        {
            var connectionId = messageHandler.connectionId;
            var message = messageHandler.ReadMessage<RequestGooglePlayLoginMessage>();
            var error = ResponseUserLoginMessage.Error.None;
            var job = new GooglePlayLoginJob(Database, message.idToken);
            job.Start();
            yield return StartCoroutine(job.WaitFor());
            var userId = job.result;
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
                var updateAccessTokenJob = new UpdateAccessTokenJob(Database, userId, accessToken);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
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
