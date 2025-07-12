using Flowcast.Serialization;
using System.IO;

namespace Flowcast.Tests.Runtime
{
    public class MyGameState1 : ISerializableGameState
    {
        public int Health;
    }

    public class MyGameState : IBinarySerializableGameState
    {
        public int Health;

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(Health);
        }

        public void ReadFrom(BinaryReader reader)
        {
            Health = reader.ReadInt32();
        }
    }
}
