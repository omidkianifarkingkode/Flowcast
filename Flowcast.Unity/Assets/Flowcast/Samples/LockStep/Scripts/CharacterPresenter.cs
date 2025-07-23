using FixedMathSharp;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPresenter
{
    public CharacterData Data { get; private set; }
    public CharacterView View { get; internal set; }

    private CharacterStaticData _staticData;
    public System.Action OnReachedTarget;

    private Queue<Vector2d> _path;
    private Vector2d _currentTarget;

    public bool HasReached;

    public CharacterPresenter(CharacterData data, IEnumerable<Vector2> pathPoints, CharacterStaticData staticData)
    {
        Data = data;
        _staticData = staticData;

        _path = new Queue<Vector2d>();

        int index = 0;
        foreach (var point in pathPoints)
        {
            if (index++ >= Data.PathIndex)
                _path.Enqueue(point.ToVector2d());
        }

        SetNextTarget();
    }

    public void SetView(CharacterView view) 
    {
        View = view;
    }

    public void Tick(Fixed64 deltaTime)
    {
        if (HasReached) return;

        var distanceToMove = _staticData.MoveSpeed * deltaTime;
        var distanceToTarget = Vector2d.Distance(Data.Position, _currentTarget);

        if (distanceToTarget > distanceToMove)
        {
            Vector2d direction = (_currentTarget - Data.Position).Normal;
            Data.Position += direction * distanceToMove;
        }
        else
        {
            Data.Position = _currentTarget;
            Data.PathIndex++;
            SetNextTarget();
        }

        Data.Health -= _staticData.HPDecayRate * deltaTime;
        Data.Health = Data.Health < Fixed64.Zero ? Fixed64.Zero : Data.Health;
    }

    private void SetNextTarget()
    {
        if (_path.Count > 0)
        {
            _currentTarget = _path.Dequeue();
        }
        else
        {
            HasReached = true;
            OnReachedTarget?.Invoke();
        }
    }
}

