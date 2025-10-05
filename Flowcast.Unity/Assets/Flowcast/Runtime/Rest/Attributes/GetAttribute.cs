using System;

namespace Flowcast.Rest.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GetAttribute : Attribute
    {
        public string Route { get; }
        public GetAttribute(string route) => Route = route;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class PostAttribute : Attribute
    {
        public string Route { get; }
        public PostAttribute(string route) => Route = route;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CacheAttribute : Attribute
    {
        public int Duration { get; }
        public CacheAttribute(int seconds) => Duration = seconds;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class RequireAuthAttribute : Attribute { }
}
