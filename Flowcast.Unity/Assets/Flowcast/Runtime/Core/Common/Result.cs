// Runtime/Core/Common/Result/Result.cs
using System;

namespace Flowcast.Core.Common
{
    public readonly struct Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public Error Error { get; }
        public ResponseMeta Meta { get; }

        private Result(T value, ResponseMeta meta)
        {
            IsSuccess = true;
            Value = value;
            Error = default;
            Meta = meta;
        }
        private Result(Error error, ResponseMeta meta)
        {
            IsSuccess = false;
            Value = default;
            Error = error;
            Meta = meta;
        }

        public static Result<T> Success(T value, ResponseMeta meta = default) => new(value, meta);
        public static Result<T> Failure(Error error, ResponseMeta meta = default) => new(error, meta);
    }
}
