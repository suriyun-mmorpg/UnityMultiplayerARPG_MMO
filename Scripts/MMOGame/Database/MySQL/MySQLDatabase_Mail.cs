#if UNITY_STANDALONE && !CLIENT_BUILD
using System.Collections.Generic;
using MySqlConnector;
using Cysharp.Threading.Tasks;
using System;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override async UniTask<List<MailListEntry>> MailList(string userId, bool onlyNewMails)
        {
            List<MailListEntry> result = new List<MailListEntry>();
            await ExecuteReader((reader) =>
            {
                MailListEntry tempMail;
                while (reader.Read())
                {
                    int gold = reader.GetInt32(3);
                    string currencies = reader.GetString(4);
                    string items = reader.GetString(5);
                    tempMail = new MailListEntry()
                    {
                        Id = reader.GetInt64(0).ToString(),
                        SenderName = reader.GetString(1),
                        Title = reader.GetString(2),
                        IsRead = reader.GetBoolean(6),
                        IsClaim = reader.GetBoolean(7),
                        SentTimestamp = (int)(reader.GetDateTime(8).Ticks / TimeSpan.TicksPerMillisecond),
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
            }, "SELECT id, senderName, title, gold, currencies, items, isRead, isClaim, sentTimestamp FROM mail WHERE receiverId=@receiverId AND isDelete=0 ORDER BY sentTimestamp DESC",
                new MySqlParameter("@receiverId", userId));
            return result;
        }

        public override async UniTask<Mail> GetMail(string mailId, string userId)
        {
            Mail result = new Mail();
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                {
                    result.Id = reader.GetInt64(0).ToString();
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
                        result.ReadTimestamp = (int)(reader.GetDateTime(11).Ticks / TimeSpan.TicksPerMillisecond);
                    result.IsClaim = reader.GetBoolean(12);
                    if (reader[13] != DBNull.Value)
                        result.ClaimTimestamp = (int)(reader.GetDateTime(13).Ticks / TimeSpan.TicksPerMillisecond);
                    result.SentTimestamp = (int)(reader.GetDateTime(14).Ticks / TimeSpan.TicksPerMillisecond);
                }
            }, "SELECT id, eventId, senderId, senderName, receiverId, title, content, gold, currencies, items, isRead, readTimestamp, isClaim, claimTimestamp, sentTimestamp FROM mail WHERE id=@id AND receiverId=@receiverId AND isDelete=0",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            return result;
        }

        public override async UniTask<long> UpdateReadMailState(string mailId, string userId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await ExecuteNonQuery("UPDATE mail SET isRead=1, readTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isRead=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<long> UpdateClaimMailItemsState(string mailId, string userId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await ExecuteNonQuery("UPDATE mail SET isClaim=1, claimTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isClaim=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<long> UpdateDeleteMailState(string mailId, string userId)
        {
            object result = await ExecuteScalar("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                await ExecuteNonQuery("UPDATE mail SET isDelete=1, deleteTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isDelete=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override async UniTask<int> CreateMail(Mail mail)
        {
            return await ExecuteNonQuery("INSERT INTO mail (eventId, senderId, senderName, receiverId, title, content, gold, currencies, items) " +
                "VALUES (@eventId, @senderId, @senderName, @receiverId, @title, @content, @gold, @currencies, @items)",
                    new MySqlParameter("@eventId", mail.EventId),
                    new MySqlParameter("@senderId", mail.SenderId),
                    new MySqlParameter("@senderName", mail.SenderName),
                    new MySqlParameter("@receiverId", mail.ReceiverId),
                    new MySqlParameter("@title", mail.Title),
                    new MySqlParameter("@content", mail.Content),
                    new MySqlParameter("@gold", mail.Gold),
                    new MySqlParameter("@currencies", mail.WriteCurrencies()),
                    new MySqlParameter("@items", mail.WriteItems()));
        }
    }
}
#endif