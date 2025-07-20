using UnityEngine;

public static class CharacterFactory
{
    public static CharacterView SpawnCharacter(CharacterView prefab, CharacterData data)
    {
        // Instantiate view prefab
        var view = Object.Instantiate(prefab, data.Position, Quaternion.identity);

        return view;
    }
}