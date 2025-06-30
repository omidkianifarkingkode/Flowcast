#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_ANDROID || UNITY_IOS

using UnityEngine;

namespace Flowcast.Serialization
{
    public static class XorHasherUnityExtensions
    {
        public static void Write(this IHasher hasher, Vector2 vec)
        {
            hasher.Write(vec.x);
            hasher.Write(vec.y);
        }

        public static void Write(this IHasher hasher, Vector3 vec)
        {
            hasher.Write(vec.x);
            hasher.Write(vec.y);
            hasher.Write(vec.z);
        }

        public static void Write(this IHasher hasher, Quaternion quat)
        {
            hasher.Write(quat.x);
            hasher.Write(quat.y);
            hasher.Write(quat.z);
            hasher.Write(quat.w);
        }
    }
}

#endif
