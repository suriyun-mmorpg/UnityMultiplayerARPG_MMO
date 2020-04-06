using System.Collections.Generic;
using Google.Protobuf;
using Google.Protobuf.Collections;
using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public static class DatabaseServiceUtils
    {
        public static T FromBytes<T>(this ByteString byteStr)
            where T : INetSerializable
        {
            NetDataReader reader = new NetDataReader(byteStr.ToByteArray());
            return reader.GetValue<T>();
        }

        public static ByteString ToBytes<T>(this T data)
            where T : INetSerializable
        {
            NetDataWriter writer = new NetDataWriter();
            writer.PutValue(data);
            return ByteString.CopyFrom(writer.Data);
        }

        public static void CopyToRepeatedBytes<T>(this T[] from, RepeatedField<ByteString> to)
            where T : INetSerializable
        {
            to.Clear();
            for (int i = 0; i < from.Length; ++i)
            {
                to.Add(ToBytes(from[i]));
            }
        }

        public static void CopyToRepeatedBytes<T>(this List<T> from, RepeatedField<ByteString> to)
            where T : INetSerializable
        {
            to.Clear();
            for (int i = 0; i < from.Count; ++i)
            {
                to.Add(ToBytes(from[i]));
            }
        }

        public static T[] MakeArrayFromRepeatedBytes<T>(this RepeatedField<ByteString> from)
            where T : INetSerializable
        {
            T[] to = new T[from.Count];
            for (int i = 0; i < from.Count; ++i)
            {
                to[i] = FromBytes<T>(from[i]);
            }
            return to;
        }

        public static List<T> MakeListFromRepeatedBytes<T>(this RepeatedField<ByteString> from)
            where T : INetSerializable
        {
            List<T> to = new List<T>();
            for (int i = 0; i < from.Count; ++i)
            {
                to.Add(FromBytes<T>(from[i]));
            }
            return to;
        }
    }
}
