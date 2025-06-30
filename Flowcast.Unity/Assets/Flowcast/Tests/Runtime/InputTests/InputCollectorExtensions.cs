using Flowcast.Commons;
using Flowcast.Inputs;

namespace Flowcast.Tests.Runtime.InputTests
{
    public static class InputCollectorExtensions
    {
        public static Result AddSpawnInput(this ILocalInputCollector collector, int objectId, int x, int y)
        {
            var input = new SpawnInput(1, x, y);
            return collector.Collect(input);
        }

        // Add more for each input type
    }
}

