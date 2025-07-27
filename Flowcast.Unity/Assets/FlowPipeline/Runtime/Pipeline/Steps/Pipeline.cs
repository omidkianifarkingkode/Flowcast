using FixedMathSharp;
using System;

namespace FlowPipeline
{
    public static class FlowStepFactory
    {
        public static IFlowStep<T> CreateStep<T>(Action<T, ulong, Fixed64> logic)
            => new FlowStep<T>(logic);
    }

}
