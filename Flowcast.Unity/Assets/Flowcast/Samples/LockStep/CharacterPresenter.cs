using System.Collections.Generic;
using UnityEngine;

public class CharacterPresenter
{
    public CharacterData Data { get; private set; }

    private Vector2 _targetPosition;
    private CharacterStaticData _staticData;
    public System.Action OnReachedTarget;

    private bool _hasReached;

    public CharacterPresenter(CharacterData data, Vector2 targetPos, CharacterStaticData staticData)
    {
        Data = data;
        _targetPosition = targetPos;
        _staticData = staticData;
    }

    public void Tick(float deltaTime)
    {
        if (_hasReached) return;

        float distanceToMove = _staticData.MoveSpeed * deltaTime;

        Vector2 direction = (_targetPosition - Data.Position).normalized;
        float distanceToTarget = Vector2.Distance(Data.Position, _targetPosition);

        if (distanceToTarget > distanceToMove)
        {
            Data.Position += direction * distanceToMove;
        }
        else
        {
            Data.Position = _targetPosition;
            _hasReached = true;
            OnReachedTarget?.Invoke();
        }

        Data.Health -= Mathf.RoundToInt(_staticData.HPDecayRate * deltaTime);
        Data.Health = Mathf.Max(0, Data.Health);
    }
}

