using System.Collections.Generic;
using UnityEngine;

public class CharacterPresenter
{
    public CharacterData Data { get; private set; }
    public CharacterView View { get; internal set; }

    private CharacterStaticData _staticData;
    public System.Action OnReachedTarget;

    private Queue<Vector2> _path;
    private Vector2 _currentTarget;

    public bool HasReached;

    public CharacterPresenter(CharacterData data, IEnumerable<Vector2> pathPoints, CharacterStaticData staticData)
    {
        Data = data;
        _staticData = staticData;

        _path = new Queue<Vector2>();
        Vector2 currentPosition = Data.Position;

        bool started = false;
        foreach (var point in pathPoints)
        {
            if (!started && Vector2.Distance(point, currentPosition) < 0.1f)
            {
                started = true;
            }

            if (started || Vector2.Distance(currentPosition, point) > 0.1f)
            {
                _path.Enqueue(point);
            }
        }

        SetNextTarget();
    }

    public void SetView(CharacterView view) 
    {
        View = view;
    }

    public void Tick(float deltaTime)
    {
        if (HasReached) return;

        float distanceToMove = _staticData.MoveSpeed * deltaTime;
        float distanceToTarget = Vector2.Distance(Data.Position, _currentTarget);

        if (distanceToTarget > distanceToMove)
        {
            Vector2 direction = (_currentTarget - Data.Position).normalized;
            Data.Position += direction * distanceToMove;
        }
        else
        {
            Data.Position = _currentTarget;
            SetNextTarget();
        }

        Data.Health -= Mathf.RoundToInt(_staticData.HPDecayRate * deltaTime);
        Data.Health = Mathf.Max(0, Data.Health);
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

