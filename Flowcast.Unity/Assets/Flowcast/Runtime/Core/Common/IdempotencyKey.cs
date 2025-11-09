// Runtime/Core/Common/IdempotencyKey.cs
namespace Flowcast.Core.Common
{
    public static class IdempotencyKey
    {
        public static string New() => System.Guid.NewGuid().ToString("N");
    }
}
