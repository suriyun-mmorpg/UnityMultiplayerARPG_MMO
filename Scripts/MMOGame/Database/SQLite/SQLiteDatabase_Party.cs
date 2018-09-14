using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override async Task<int> CreateParty(bool shareExp, bool shareItem, string leaderId)
        {
            return 0;
        }

        public override async Task<PartyMemberData> ReadParty(int id)
        {
            var result = new PartyMemberData();
            return result;
        }

        public override async Task UpdateParty(int id, bool shareExp, bool shareItem)
        {
        }

        public override async Task DeleteParty(int id)
        {
        }
    }
}
