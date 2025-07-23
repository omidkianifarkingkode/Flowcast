using FixedMathSharp;
using UnityEngine;

[System.Serializable]
public class CharacterStaticData
{
    public CharacterType Name;
    [HideInInspector] public Fixed64 HP => (Fixed64)_hp;
    [HideInInspector] public Fixed64 MoveSpeed => (Fixed64)_moveSpeed;
    [HideInInspector] public Fixed64 HPDecayRate => (Fixed64)_hpDecayRate;
    public CharacterView Prefab;

    [SerializeField] int _hp;
    [SerializeField] float _moveSpeed = 2;
    [SerializeField] float _hpDecayRate = 2;
}