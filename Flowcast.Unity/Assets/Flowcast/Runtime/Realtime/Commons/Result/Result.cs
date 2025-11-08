using System;

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

        public void Match(Action onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) onSuccess();
            else onFailure(Error);
        }

        public static Result Try(Action action, string errorMessage = null)
        {
            try
            {
                action();
                return Success();
            }
            catch (Exception ex)
            {
                return Failure(errorMessage ?? ex.Message);
            }
        }


        public override string ToString() =>
            IsSuccess ? "Success" : $"Failure: {Error}";
    }
}
