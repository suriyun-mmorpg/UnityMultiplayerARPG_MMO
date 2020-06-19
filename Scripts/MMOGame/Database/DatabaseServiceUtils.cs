using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public static class DatabaseServiceUtils
    {
        public static T FromByteString<T>(this ByteString byteStr)
            where T : INetSerializable
        {
            NetDataReader reader = new NetDataReader(byteStr.ToByteArray());
            return reader.GetValue<T>();
        }

        public static ByteString ToByteString<T>(this T data)
            where T : INetSerializable
        {
            NetDataWriter writer = new NetDataWriter();
            writer.PutValue(data);
            return ByteString.CopyFrom(writer.Data);
        }

        public static void CopyToRepeatedByteString<T>(this T[] from, RepeatedField<ByteString> to)
            where T : INetSerializable
        {
            to.Clear();
            for (int i = 0; i < from.Length; ++i)
            {
                to.Add(ToByteString(from[i]));
            }
        }

        public static void CopyToRepeatedByteString<T>(this List<T> from, RepeatedField<ByteString> to)
            where T : INetSerializable
        {
            to.Clear();
            for (int i = 0; i < from.Count; ++i)
            {
                to.Add(ToByteString(from[i]));
            }
        }

        public static void CopyToRepeatedByteString<T>(this IList<T> from, RepeatedField<ByteString> to)
            where T : INetSerializable
        {
            to.Clear();
            for (int i = 0; i < from.Count; ++i)
            {
                to.Add(ToByteString(from[i]));
            }
        }

        public static T[] MakeArrayFromRepeatedByteString<T>(this RepeatedField<ByteString> from)
            where T : INetSerializable
        {
            T[] to = new T[from.Count];
            for (int i = 0; i < from.Count; ++i)
            {
                to[i] = FromByteString<T>(from[i]);
            }
            return to;
        }

        public static List<T> MakeListFromRepeatedByteString<T>(this RepeatedField<ByteString> from)
            where T : INetSerializable
        {
            List<T> to = new List<T>();
            for (int i = 0; i < from.Count; ++i)
            {
                to.Add(FromByteString<T>(from[i]));
            }
            return to;
        }
    }
}
