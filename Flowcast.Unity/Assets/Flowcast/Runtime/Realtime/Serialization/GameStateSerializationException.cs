using System;

namespace Flowcast.Serialization
{
    /// <summary>
    /// Custom exception for serialization failures.
    /// </summary>
    public class GameStateSerializationException : Exception
    {
        public GameStateSerializationException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}

