#if NET || NETCOREAPP || ((UNITY_EDITOR || UNITY_SERVER) && UNITY_STANDALONE)
using System.Collections.Generic;
using MySqlConnector;
using System;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override List<MailListEntry> MailList(string userId, bool onlyNewMails)
        {
            List<MailListEntry> result = new List<MailListEntry>();
            ExecuteReaderSync((reader) =>
            {
                MailListEntry tempMail;
                while (reader.Read())
                {
                    int gold = reader.GetInt32(3);
                    int cash = reader.GetInt32(4);
                    string currencies = reader.GetString(5);
                    string items = reader.GetString(6);
                    tempMail = new MailListEntry()
                    {
                        Id = reader.GetInt64(0).ToString(),
                        SenderName = reader.GetString(1),
                        Title = reader.GetString(2),
                        IsRead = reader.GetBoolean(7),
                        IsClaim = reader.GetBoolean(8),
                        SentTimestamp = ((DateTimeOffset)reader.GetDateTime(9)).ToUnixTimeSeconds(),
                    };
                    if (onlyNewMails)
                    {
                        if (!tempMail.IsClaim && (gold != 0 || cash != 0 || !string.IsNullOrEmpty(currencies) || !string.IsNullOrEmpty(items)))
                            result.Add(tempMail);
                        else if (!tempMail.IsRead)
                            result.Add(tempMail);
                    }
                    else
                    {
                        result.Add(tempMail);
                    }
                }
            }, "SELECT id, senderName, title, gold, cash, currencies, items, isRead, isClaim, sentTimestamp FROM mail WHERE receiverId=@receiverId AND isDelete=0 ORDER BY isRead ASC, sentTimestamp DESC",
                new MySqlParameter("@receiverId", userId));
            return result;
        }

        public override Mail GetMail(string mailId, string userId)
        {
            Mail result = new Mail();
            ExecuteReaderSync((reader) =>
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
                    result.Cash = reader.GetInt32(8);
                    result.ReadCurrencies(reader.GetString(9));
                    result.ReadItems(reader.GetString(10));
                    result.IsRead = reader.GetBoolean(11);
                    if (reader[12] != DBNull.Value)
                        result.ReadTimestamp = ((DateTimeOffset)reader.GetDateTime(12)).ToUnixTimeSeconds();
                    result.IsClaim = reader.GetBoolean(13);
                    if (reader[14] != DBNull.Value)
                        result.ClaimTimestamp = ((DateTimeOffset)reader.GetDateTime(14)).ToUnixTimeSeconds();
                    result.SentTimestamp = ((DateTimeOffset)reader.GetDateTime(15)).ToUnixTimeSeconds();
                }
            }, "SELECT id, eventId, senderId, senderName, receiverId, title, content, gold, cash, currencies, items, isRead, readTimestamp, isClaim, claimTimestamp, sentTimestamp FROM mail WHERE id=@id AND receiverId=@receiverId AND isDelete=0",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            return result;
        }

        public override long UpdateReadMailState(string mailId, string userId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                ExecuteNonQuerySync("UPDATE mail SET isRead=1, readTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isRead=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override long UpdateClaimMailItemsState(string mailId, string userId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                ExecuteNonQuerySync("UPDATE mail SET isClaim=1, claimTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isClaim=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override long UpdateDeleteMailState(string mailId, string userId)
        {
            object result = ExecuteScalarSync("SELECT COUNT(*) FROM mail WHERE id=@id AND receiverId=@receiverId",
                new MySqlParameter("@id", mailId),
                new MySqlParameter("@receiverId", userId));
            long count = result != null ? (long)result : 0;
            if (count > 0)
            {
                ExecuteNonQuerySync("UPDATE mail SET isDelete=1, deleteTimestamp=NOW() WHERE id=@id AND receiverId=@receiverId AND isDelete=0",
                    new MySqlParameter("@id", mailId),
                    new MySqlParameter("@receiverId", userId));
            }
            return count;
        }

        public override int CreateMail(Mail mail)
        {
            return ExecuteNonQuerySync("INSERT INTO mail (eventId, senderId, senderName, receiverId, title, content, gold, cash, currencies, items) " +
                "VALUES (@eventId, @senderId, @senderName, @receiverId, @title, @content, @gold, @cash, @currencies, @items)",
                    new MySqlParameter("@eventId", mail.EventId),
                    new MySqlParameter("@senderId", mail.SenderId),
                    new MySqlParameter("@senderName", mail.SenderName),
                    new MySqlParameter("@receiverId", mail.ReceiverId),
                    new MySqlParameter("@title", mail.Title),
                    new MySqlParameter("@content", mail.Content),
                    new MySqlParameter("@gold", mail.Gold),
                    new MySqlParameter("@cash", mail.Cash),
                    new MySqlParameter("@currencies", mail.WriteCurrencies()),
                    new MySqlParameter("@items", mail.WriteItems()));
        }

        public override int GetMailNotification(string userId)
        {
            int count = 0;
            ExecuteReaderSync((reader) =>
            {
                while (reader.Read())
                {
                    int gold = reader.GetInt32(0);
                    int cash = reader.GetInt32(1);
                    string currencies = reader.GetString(2);
                    string items = reader.GetString(3);
                    bool isRead = reader.GetBoolean(4);
                    bool isClaim = reader.GetBoolean(5);
                    if (!isClaim && (gold != 0 || !string.IsNullOrEmpty(currencies) || !string.IsNullOrEmpty(items)))
                        count++;
                    else if (!isRead)
                        count++;
                }
            }, "SELECT gold, cash, currencies, items, isRead, isClaim FROM mail WHERE receiverId=@receiverId AND isDelete=0",
                new MySqlParameter("@receiverId", userId));
            return count;
        }
    }
}
#endif