using FixedMathSharp;
using Flowcast.Serialization;
using System.Collections.Generic;
using System.IO;

public class GameState : IBinarySerializableGameState
{
    public List<CharacterData> characters = new();

    public void WriteTo(BinaryWriter writer)
    {
        writer.WriteList(characters, (w, character) =>
        {
            w.Write((int)character.Type);
            w.Write(character.Health.m_rawValue);
            w.Write(character.Position.x.m_rawValue);
            w.Write(character.Position.y.m_rawValue);
            w.Write(character.PathIndex);
        });
    }

    public void ReadFrom(BinaryReader reader)
    {
        characters = reader.ReadList(r => new CharacterData
        {
            Type = (CharacterType)r.ReadInt32(),
            Health = Fixed64.FromRaw(r.ReadInt64()),
            Position = new Vector2d(r.ReadInt64(), r.ReadInt64()),
            PathIndex = r.ReadInt32(),
        });
    }

    public int GetEstimatedSize()
    {
        // 1 int (enum) + 1 int (health) + 2 floats (Vector2) + 1 int (pathIndex)
        int perCharacterSize = sizeof(int) + sizeof(int) + sizeof(float) * 2 + sizeof(int);

        // 1 int for the list count + per-character size * number of characters
        return sizeof(int) + characters.Count * perCharacterSize;
    }


    public ISerializableGameState CreateDefault()
    {
        return new GameState
        {
            characters = new List<CharacterData>()
        };
    }
}
