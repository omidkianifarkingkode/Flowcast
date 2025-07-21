using UnityEngine;

public enum CharacterType
{
    Warrior = 1,
    Mage = 2,
    Archer = 3,
    Healer = 4
}

public class CharacterData
{
    public CharacterType Type { get; set; }
    public int Health { get; set; }
    public Vector2 Position { get; set; }
}