using System;

namespace Flowcast.Commons
{
    public class Result<T> : Result
    {
        public T Value { get; }

        protected Result(bool isSuccess, string error, T value)
            : base(isSuccess, error)
        {
            Value = value;
        }

        public static Result<T> Success(T value) => new(true, null, value);

        public static new Result<T> Failure(string error) => new(false, error, default);

        public static Result<T> Try(Func<T> func, string customError = null)
        {
            try
            {
                return Success(func());
            }
            catch (Exception ex)
            {
                return Failure(customError ?? ex.Message);
            }
        }

        public void Match(Action<T> onSuccess, Action<string> onFailure)
        {
            if (IsSuccess) onSuccess(Value);
            else onFailure(Error);
        }

        public Result<U> Map<U>(Func<T, U> func)
        {
            if (IsFailure) return Result<U>.Failure(Error);
            return Result<U>.Success(func(Value));
        }

        public Result<U> Bind<U>(Func<T, Result<U>> func)
        {
            if (IsFailure) return Result<U>.Failure(Error);
            return func(Value);
        }

        public Result<T> OnSuccess(Action<T> action)
        {
            if (IsSuccess) action(Value);
            return this;
        }

        public Result<T> OnFailure(Action<string> action)
        {
            if (IsFailure) action(Error);
            return this;
        }

        public override string ToString() =>
            IsSuccess ? $"Success: {Value}" : $"Failure: {Error}";

        // Optional implicit conversion (use with caution)
        public static implicit operator Result<T>(T value) => Success(value);
    }
}
