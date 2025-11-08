// Runtime/Core/Common/Result/ErrorKind.cs
namespace Flowcast.Core.Common
{
    public enum ErrorKind { None = 0, Client, Auth, Transient, Server, Canceled, Unknown }
}
