#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using MySqlConnector;
using LiteNetLibManager;
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override UniTask<List<MailListEntry>> MailList(string userId, bool onlyNewMails)
        {
            return default;
        }

        public override UniTask<Mail> GetMail(string mailId, string userId)
        {
            return default;
        }

        public override UniTask<long> UpdateReadMailState(string mailId, string userId)
        {
            return default;
        }

        public override UniTask<long> UpdateClaimMailItemsState(string mailId, string userId)
        {
            return default;
        }

        public override UniTask<long> UpdateDeleteMailState(string mailId, string userId)
        {
            return default;
        }

        public override UniTask<int> CreateMail(Mail mail)
        {
            return default;
        }
    }
}
#endif