#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override async UniTask<List<MailListEntry>> MailList(string userId, bool onlyNewMails)
        {
            await UniTask.Yield();
            List<MailListEntry> result = new List<MailListEntry>();
            ExecuteReader((reader) =>
            {
                MailListEntry tempMail;
                while (reader.Read())
                {
                    int gold = reader.GetInt32(3);
                    string currencies = reader.GetString(4);
                    string items = reader.GetString(5);
                    tempMail = new MailListEntry()
                    {
                        Id = reader.GetInt32(0).ToString(),
                        SenderName = reader.GetString(1),
                        Title = reader.GetString(2),
                        IsRead = reader.GetBoolean(6),
                        IsClaim = reader.GetBoolean(7),
                        SentTimestamp = (int)((DateTimeOffset)reader.GetDateTime(8)).ToUnixTimeSeconds(),
                    };
                    if (onlyNewMails)
                    {
                        if (!tempMail.IsClaim && (gold != 0 || !string.IsNullOrEmpty(currencies) || !string.IsNullOrEmpty(items)))
                            result.Add(tempMail);
                        else if (!tempMail.IsRead)
                            result.Add(tempMail);
                    }
                    else
                    {
                        result.Add(tempMail);
                    }
                }
            }, "SELECT id, senderName, title, gold, currencies, items, isRead, isClaim, sentTimestamp FROM mail WHERE receiverId LIKE @receiverId AND isDelete=0 ORDER BY isRead ASC, sentTimestamp DESC",
                new SqliteParameter("@receiverId", userId));
            return result;
        }

        public override async UniTask<Mail> GetMail(string mailId, string userId)
        {
            await UniTask.Yield();
            Mail result = new Mail();
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    result.Id = reader.GetInt32(0).ToString();
                    result.EventId = reader.GetString(1);
                    result.SenderId = reader.GetString(2);
                    result.SenderName = reader.GetString(3);
                    result.ReceiverId = reader.GetString(4);
                    result.Title = reader.GetString(5);
                    result.Content = reader.GetString(6);
                    result.Gold = reader.GetInt32(7);
                    result.ReadCurrencies(reader.GetString(8));
                    result.ReadItems(reader.GetString(9));
                    result.IsRead = reader.GetBoolean(10);
                    if (reader[11] != DBNull.Value)
                        result.ReadTimestamp = (int)((DateTimeOffset)reader.GetDateTime(11)).ToUnixTimeSeconds();
                    result.IsClaim = reader.GetBoolean(12);
                    if (reader[13] != DBNull.Value)
                        result.ClaimTimestamp = (int)((DateTimeOffset)reader.GetDateTime(13)).ToUnixTimeSeconds();
                    result.SentTimestamp = (int)((DateTimeOffset)reader.GetDateTime(14)).ToUnixTimeSeconds();
                }
            }, "SELECT id, eventId, senderId, senderName, receiverId, title, content, gold, currencies, items, isRead, readTimestamp, isClaim, claimTimestamp, sentTimestamp FROM mail WHERE id=@id AND receiverId LIKE @receiverId AND isDelete=0",
                new SqliteParameter("@id", mailId),
                new SqliteParameter("@receiverId", userId));
            return result;
        }

        public override async UniTask<long> UpdateReadMailState(string mailId, string userId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId LIKE @receiverId",
                new SqliteParameter("@id", mailId),
                new SqliteParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await UniTask.Yield();
                ExecuteNonQuery("UPDATE mail SET isRead=1, readTimestamp=datetime('now') WHERE id=@id AND receiverId LIKE @receiverId AND isRead=0",
                    new SqliteParameter("@id", mailId),
                    new SqliteParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<long> UpdateClaimMailItemsState(string mailId, string userId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId LIKE @receiverId",
                new SqliteParameter("@id", mailId),
                new SqliteParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await UniTask.Yield();
                ExecuteNonQuery("UPDATE mail SET isClaim=1, claimTimestamp=datetime('now') WHERE id=@id AND receiverId LIKE @receiverId AND isClaim=0",
                    new SqliteParameter("@id", mailId),
                    new SqliteParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<long> UpdateDeleteMailState(string mailId, string userId)
        {
            object result = ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId LIKE @receiverId",
                new SqliteParameter("@id", mailId),
                new SqliteParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await UniTask.Yield();
                ExecuteNonQuery("UPDATE mail SET isDelete=1, deleteTimestamp=datetime('now') WHERE id=@id AND receiverId LIKE @receiverId AND isDelete=0",
                    new SqliteParameter("@id", mailId),
                    new SqliteParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<int> CreateMail(Mail mail)
        {
            await UniTask.Yield();
            return ExecuteNonQuery("INSERT INTO mail (eventId, senderId, senderName, receiverId, title, content, gold, currencies, items) " +
                "VALUES (@eventId, @senderId, @senderName, @receiverId, @title, @content, @gold, @currencies, @items)",
                    new SqliteParameter("@eventId", mail.EventId),
                    new SqliteParameter("@senderId", mail.SenderId),
                    new SqliteParameter("@senderName", mail.SenderName),
                    new SqliteParameter("@receiverId", mail.ReceiverId),
                    new SqliteParameter("@title", mail.Title),
                    new SqliteParameter("@content", mail.Content),
                    new SqliteParameter("@gold", mail.Gold),
                    new SqliteParameter("@currencies", mail.WriteCurrencies()),
                    new SqliteParameter("@items", mail.WriteItems()));
        }
    }
}
#endif