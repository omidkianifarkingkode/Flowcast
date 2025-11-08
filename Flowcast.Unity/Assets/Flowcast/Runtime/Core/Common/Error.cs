// Runtime/Core/Common/Result/Error.cs
namespace Flowcast.Core.Common
{
    public readonly struct Error
    {
        public ErrorKind Kind { get; }
        public int Status { get; }
        public string Code { get; }
        public string Message { get; }
        public string Details { get; }

        public Error(ErrorKind kind, int status, string code, string message, string details = null)
        {
            Kind = kind; Status = status; Code = code; Message = message; Details = details;
        }
        public override string ToString() => $"{Kind}({Status}) {Code}: {Message}";
        public static Error Unknown(string message, int status = 0, string code = null, string details = null)
            => new(ErrorKind.Unknown, status, code, message, details);
    }
}
