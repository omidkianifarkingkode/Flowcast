using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Flowcast.Serialization
{
    public static class BinarySerializationExtensions
    {
        // Write a list of serializable objects
        public static void WriteList<T>(this BinaryWriter writer, List<T> list, Action<BinaryWriter, T> writeElement)
        {
            writer.Write(list.Count);
            foreach (var element in list)
            {
                writeElement(writer, element);
            }
        }

        // Read a list of serializable objects
        public static List<T> ReadList<T>(this BinaryReader reader, Func<BinaryReader, T> readElement)
        {
            int count = reader.ReadInt32();
            var list = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                list.Add(readElement(reader));
            }
            return list;
        }

        // Write a serializable object
        public static void WriteObject<T>(this BinaryWriter writer, T obj, Action<BinaryWriter, T> writeAction)
        {
            writeAction(writer, obj);
        }

        // Read a serializable object
        public static T ReadObject<T>(this BinaryReader reader, Func<BinaryReader, T> readFunc)
        {
            return readFunc(reader);
        }

        public static void WriteVector2(this BinaryWriter writer, Vector2 v)
        {
            writer.Write(v.x);
            writer.Write(v.y);
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            return new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }

    }
}
