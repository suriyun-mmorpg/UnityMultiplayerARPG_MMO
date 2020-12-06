#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using LiteNetLibManager;
using Mono.Data.Sqlite;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
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