namespace Flowcast.Commons
{
    public class Result
    {
        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string Error { get; }

        protected Result(bool isSuccess, string error)
        {
            if (isSuccess && error != null) throw new InvalidSuccessResultException();
            if (!isSuccess && error == null) throw new InvalidFailureResultException();

            IsSuccess = isSuccess;
            Error = error;
        }

        public static Result Success() => new(true, null);
        public static Result Failure(string error) => new(false, error);

        public override string ToString() =>
            IsSuccess ? "Success" : $"Failure: {Error}";
    }
}
