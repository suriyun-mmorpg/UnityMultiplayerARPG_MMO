using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class MMOServerMailHandlers : MonoBehaviour, IServerMailHandlers
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public DatabaseNetworkManager DbServiceClient
        {
            get { return MMOServerInstance.Singleton.DatabaseNetworkManager; }
        }
#endif

        public async UniTask<bool> SendMail(Mail mail)
        {
#if UNITY_STANDALONE && !CLIENT_BUILD
            SendMailResp resp = await DbServiceClient.SendMailAsync(new SendMailReq()
            {
                Mail = mail,
            });
            if (resp.Error == 0)
                return true;
#endif
            return false;
        }
    }
}
