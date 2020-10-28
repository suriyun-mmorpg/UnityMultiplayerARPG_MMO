using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestUserLogin(string username, string password, ResponseDelegate callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserLogin, new RequestUserLoginMessage()
            {
                username = username,
                password = password,
            }, responseDelegate: callback);
        }

        public bool RequestUserRegister(string username, string password, ResponseDelegate callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserRegister, new RequestUserRegisterMessage()
            {
                username = username,
                password = password,
            }, responseDelegate: callback);
        }

        public bool RequestUserLogout(ResponseDelegate callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserLogout, new EmptyMessage(), responseDelegate: callback);
        }

        public bool RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestValidateAccessToken, new RequestValidateAccessTokenMessage()
            {
                userId = userId,
                accessToken = accessToken,
            }, responseDelegate: callback);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserLogin(
            long connectionId, NetDataReader reader,
            RequestUserLoginMessage request,
            RequestProceedResultDelegate<ResponseUserLoginMessage> result)
        {
            ResponseUserLoginMessage.Error error = ResponseUserLoginMessage.Error.None;
            ValidateUserLoginResp validateUserLoginResp = await DbServiceClient.ValidateUserLoginAsync(new ValidateUserLoginReq()
            {
                Username = request.username,
                Password = request.password
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
                userPeerInfo.accessToken = accessToken = Regex.Replace(System.Convert.ToBase64String(System.Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userId,
                    AccessToken = accessToken
                });
            }
            // Response
            result.Invoke(
                error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserLoginMessage()
                {
                    error = error,
                    userId = userId,
                    accessToken = accessToken,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserRegister(
            long connectionId, NetDataReader reader,
            RequestUserRegisterMessage request,
            RequestProceedResultDelegate<ResponseUserRegisterMessage> result)
        {
            ResponseUserRegisterMessage.Error error = ResponseUserRegisterMessage.Error.None;
            string username = request.username;
            string password = request.password;
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
            // Response
            result.Invoke(
                error == ResponseUserRegisterMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserRegisterMessage()
                {
                    error = error,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserLogout(
            long connectionId, NetDataReader reader,
            EmptyMessage request,
            RequestProceedResultDelegate<EmptyMessage> result)
        {
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
            // Response
            result.Invoke(AckResponseCode.Success, new EmptyMessage());
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestValidateAccessToken(
            long connectionId, NetDataReader reader,
            RequestValidateAccessTokenMessage request,
            RequestProceedResultDelegate<ResponseValidateAccessTokenMessage> result)
        {
            ResponseValidateAccessTokenMessage.Error error = ResponseValidateAccessTokenMessage.Error.None;
            string userId = request.userId;
            string accessToken = request.accessToken;
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
                userPeerInfo.accessToken = accessToken = Regex.Replace(System.Convert.ToBase64String(System.Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userPeerInfo.userId,
                    AccessToken = accessToken
                });
            }
            // Response
            result.Invoke(
                error == ResponseValidateAccessTokenMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseValidateAccessTokenMessage()
                {
                    error = error,
                    userId = userId,
                    accessToken = accessToken,
                });
        }
#endif
    }
}
