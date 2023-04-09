using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerMailHandlers : MonoBehaviour, IServerMailHandlers
    {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
        public IDatabaseClient DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseClient; }
        }
#endif

        public async UniTask<bool> SendMail(Mail mail)
        {
#if (UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE
            DatabaseApiResult<SendMailResp> resp = await DbServiceClient.SendMailAsync(new SendMailReq()
            {
                Mail = mail,
            });
            if (resp.IsSuccess && resp.Response.Error == UITextKeys.NONE)
                return true;
#endif
            return false;
        }
    }
}
