namespace Flowcast.Commons
{
    public interface IHasher
    {
        void Write(int value);
        void Write(uint value);
        void Write(long value);
        void Write(ulong value);
        void Write(float value);
        void Write(double value);
        void Write(bool value);
        void Write(char value);
        void Write(string value);
        void WriteBytes(byte[] data);

        uint GetHash();
        void Reset();
    }

    public static class HashExtensions 
    {
        public static uint ComputeHash(this IHasher hasher, byte[] data)
        {
            if (data == null || data.Length == 0)
                return 0;

            hasher.Reset();
            hasher.WriteBytes(data);
            return hasher.GetHash();
        }
    }
}

