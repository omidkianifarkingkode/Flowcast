using Flowcast.Commons;
using System.Collections.Generic;

namespace Flowcast.Inputs
{
    public interface IInputCollector
    {
        Result Collect(IInput input);
        IReadOnlyCollection<IInput> BufferedInputs { get; }
    }
}

