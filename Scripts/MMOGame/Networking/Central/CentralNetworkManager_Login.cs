using Cysharp.Threading.Tasks;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public bool RequestUserLogin(string username, string password, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserLogin, new RequestUserLoginMessage()
            {
                username = username,
                password = password,
            }, responseDelegate: callback);
        }

        public bool RequestUserRegister(string username, string password, ResponseDelegate<ResponseUserRegisterMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserRegister, new RequestUserRegisterMessage()
            {
                username = username,
                password = password,
            }, responseDelegate: callback);
        }

        public bool RequestUserLogout(ResponseDelegate callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestUserLogout, EmptyMessage.Value, responseDelegate: callback);
        }

        public bool RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate<ResponseValidateAccessTokenMessage> callback)
        {
            return ClientSendRequest(MMORequestTypes.RequestValidateAccessToken, new RequestValidateAccessTokenMessage()
            {
                userId = userId,
                accessToken = accessToken,
            }, responseDelegate: callback);
        }

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserLogin(
            RequestHandlerData requestHandler,
            RequestUserLoginMessage request,
            RequestProceedResultDelegate<ResponseUserLoginMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            ValidateUserLoginResp validateUserLoginResp = await DbServiceClient.ValidateUserLoginAsync(new ValidateUserLoginReq()
            {
                Username = request.username,
                Password = request.password
            });
            string userId = validateUserLoginResp.UserId;
            string accessToken = string.Empty;
            if (string.IsNullOrEmpty(userId))
            {
                message = UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD;
                userId = string.Empty;
            }
            else if (userPeersByUserId.ContainsKey(userId) || MapContainsUser(userId))
            {
                message = UITextKeys.UI_ERROR_ALREADY_LOGGED_IN;
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
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserLoginMessage()
                {
                    message = message,
                    userId = userId,
                    accessToken = accessToken,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserRegister(
            RequestHandlerData requestHandler,
            RequestUserRegisterMessage request,
            RequestProceedResultDelegate<ResponseUserRegisterMessage> result)
        {
            UITextKeys message = UITextKeys.NONE;
            string username = request.username;
            string password = request.password;
            FindUsernameResp findUsernameResp = await DbServiceClient.FindUsernameAsync(new FindUsernameReq()
            {
                Username = username
            });
            if (findUsernameResp.FoundAmount > 0)
                message = UITextKeys.UI_ERROR_USERNAME_EXISTED;
            else if (string.IsNullOrEmpty(username) || username.Length < minUsernameLength)
                message = UITextKeys.UI_ERROR_USERNAME_TOO_SHORT;
            else if (username.Length > maxUsernameLength)
                message = UITextKeys.UI_ERROR_USERNAME_TOO_LONG;
            else if (string.IsNullOrEmpty(password) || password.Length < minPasswordLength)
                message = UITextKeys.UI_ERROR_PASSWORD_TOO_SHORT;
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
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserRegisterMessage()
                {
                    message = message,
                });
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestUserLogout(
            RequestHandlerData requestHandler,
            EmptyMessage request,
            RequestProceedResultDelegate<EmptyMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
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
            result.Invoke(AckResponseCode.Success, EmptyMessage.Value);
        }
#endif

#if UNITY_STANDALONE && !CLIENT_BUILD
        protected async UniTaskVoid HandleRequestValidateAccessToken(
            RequestHandlerData requestHandler,
            RequestValidateAccessTokenMessage request,
            RequestProceedResultDelegate<ResponseValidateAccessTokenMessage> result)
        {
            long connectionId = requestHandler.ConnectionId;
            UITextKeys message = UITextKeys.NONE;
            string userId = request.userId;
            string accessToken = request.accessToken;
            ValidateAccessTokenResp validateAccessTokenResp = await DbServiceClient.ValidateAccessTokenAsync(new ValidateAccessTokenReq()
            {
                UserId = userId,
                AccessToken = accessToken
            });
            if (!validateAccessTokenResp.IsPass)
            {
                message = UITextKeys.UI_ERROR_INVALID_USER_TOKEN;
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
                message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseValidateAccessTokenMessage()
                {
                    message = message,
                    userId = userId,
                    accessToken = accessToken,
                });
        }
#endif
    }
}
