using FixedMathSharp;
using Flowcast.FlowPipeline;
using FlowPipeline;
using System.Collections.Generic;
using UnityEngine;

public class CharacterPresenter : ISpawnable, IMovable, IDespawnable
{
    public CharacterData Data { get; private set; }
    public CharacterView View { get; internal set; }

    public bool ShouldDespawn => HasReached;

    private readonly CharacterStaticData _staticData;
    public System.Action OnReachedTarget;

    private Queue<Vector2d> _path;
    private Vector2d _currentTarget;

    private bool HasReached;

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

    public void Move(SimulationContext context)
    {
        if (HasReached) return;

        var distanceToMove = _staticData.MoveSpeed * context.DeltaTime;
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
    }

    public void RegisterPipline(IFlowPipeline<SimulationContext> pipeline)
    {
        if (pipeline.TryGetStep<IMovable>(out var movementStep))
            movementStep.Add(this);
        if (pipeline.TryGetStep<IDespawnable>(out var despawningStep))
            despawningStep.Add(this);
    }

    public bool ShouldSpawn(SimulationContext context)
    {
        return true;
    }

    public void OnSpawned(SimulationContext context)
    {
    }
}

