using System.IO;

namespace Flowcast.Serialization
{
    /// <summary>
    /// Marker interface for game state objects that can be serialized.
    /// <para>IMPORTANT: Implementations should be reference types (classes), not structs.</para>
    /// </summary>
    public interface ISerializableGameState
    {
        ISerializableGameState CreateDefault();
    }

    /// <summary>
    /// Optional interface for binary serialization of game state.
    /// Used only when a BinarySerializer is configured.
    /// </summary>
    public interface IBinarySerializableGameState : ISerializableGameState
    {
        void WriteTo(BinaryWriter writer);
        void ReadFrom(BinaryReader reader);

        /// <summary>
        /// Estimated size in bytes when serialized.
        /// Used for efficient MemoryStream preallocation.
        /// </summary>
        int GetEstimatedSize();
    }
}

