namespace Flowcast.Rest.Core
{
    public class Result<T>
    {
        public T Value { get; }
        public Error Error { get; }
        public bool IsSuccess => Error == null;

        private Result(T value, Error error)
        {
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new(value, null);
        public static Result<T> Fail(Error error) => new(default, error);
    }

    public class Error
    {
        public int? StatusCode { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Details { get; set; }
    }

    // Marker interface for request models
    public interface IRequest<TResponse> { }
}
