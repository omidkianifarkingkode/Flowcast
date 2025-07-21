using Flowcast.Serialization;
using System.IO;

namespace Flowcast.Tests.Runtime.GameSynchronizationTests
{
    public class SampleGameState : IBinarySerializableGameState
    {
        public int HP { get; set; }
        public int Gold { get; set; }

        public ISerializableGameState CreateDefault()
        {
            return new SampleGameState() 
            {
                HP = 100, Gold = 100
            };
        }

        public int GetEstimatedSize()
        {
            return /*HP*/sizeof(int) + /*Gold*/sizeof(int);
        }

        public void ReadFrom(BinaryReader reader)
        {
            HP = reader.ReadInt32();
            Gold = reader.ReadInt32();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(HP);
            writer.Write(Gold);
        }
    }
}
