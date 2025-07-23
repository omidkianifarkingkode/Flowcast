using FixedMathSharp;
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
    public Fixed64 Health { get; set; }
    public Vector2d Position { get; set; }
    public int PathIndex { get; set; }
}