using UnityEngine;

[System.Serializable]
public class CharacterStaticData
{
    public CharacterType Name;
    public int HP;
    public float MoveSpeed = 2f;
    public float HPDecayRate = 5f;
    public CharacterView Prefab;
}