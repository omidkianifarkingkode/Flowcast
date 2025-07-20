using Flowcast.Serialization;
using System.IO;
using UnityEngine;

public class GameState : IBinarySerializableGameState
{
    public CharacterData character;

    public void WriteTo(BinaryWriter writer)
    {
        writer.Write(character != null);
        if (character != null)
        {
            writer.Write(character.Health);
            writer.Write(character.Position.x);
            writer.Write(character.Position.y);
        }
    }

    public void ReadFrom(BinaryReader reader)
    {
        bool hasCharacter = reader.ReadBoolean();
        if (hasCharacter)
        {
            character = new CharacterData
            {
                Health = reader.ReadInt32(),
                Position = new Vector2(reader.ReadSingle(), reader.ReadSingle())
            };
        }
        else
        {
            character = null;
        }
    }

    public int GetEstimatedSize()
    {
        return (sizeof(int) + sizeof(float) * 2);
    }
}