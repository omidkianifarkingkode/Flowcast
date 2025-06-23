using System;

namespace Flowcast.Commons
{
    public class InvalidSuccessResultException : Exception
    {
        public InvalidSuccessResultException()
            : base("Success result cannot have an error message.") { }
    }

    public class InvalidFailureResultException : Exception
    {
        public InvalidFailureResultException()
            : base("Failure result must have an error message.") { }
    }
}
