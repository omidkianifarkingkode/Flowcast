using System.IO;

namespace Flowcast.Serialization
{
    public interface ISerializableGameState
    {
        void WriteTo(BinaryWriter writer);
        void ReadFrom(BinaryReader reader);
    }
}

