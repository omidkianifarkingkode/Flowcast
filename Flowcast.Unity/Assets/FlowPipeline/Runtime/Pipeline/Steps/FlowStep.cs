using FixedMathSharp;
using System.Collections.Generic;
using System;

namespace FlowPipeline
{
    public class FlowStep<TEntity, TContext> : IFlowStep<TEntity, TContext> where TContext : struct
    {
        protected readonly List<TEntity> _entities = new();
        protected readonly Action<TEntity, TContext> _processEntity;

        public FlowStep() { }

        public FlowStep(Action<TEntity, TContext> processEntity)
        {
            _processEntity = processEntity ?? throw new ArgumentNullException(nameof(processEntity));
        }

        public virtual void Add(TEntity entity) => _entities.Add(entity);
        public virtual void Remove(TEntity entity) => _entities.Remove(entity);
        public virtual void Clear() => _entities.Clear();

        public virtual void Process(TContext context)
        {
            foreach (var entity in _entities)
            {
                _processEntity(entity, context);
            }
        }
    }

}
