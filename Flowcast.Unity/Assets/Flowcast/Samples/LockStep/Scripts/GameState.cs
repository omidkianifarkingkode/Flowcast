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
            w.Write(character.Health);
            w.WriteVector2(character.Position);
        });
    }

    public void ReadFrom(BinaryReader reader)
    {
        characters = reader.ReadList(r => new CharacterData
        {
            Type = (CharacterType)r.ReadInt32(),
            Health = r.ReadInt32(),
            Position = r.ReadVector2()
        });
    }

    public int GetEstimatedSize()
    {
        // 1 int (enum) + 1 int (health) + 2 floats (Vector2)
        int perCharacterSize = sizeof(int) + sizeof(int) + sizeof(float) * 2;

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
