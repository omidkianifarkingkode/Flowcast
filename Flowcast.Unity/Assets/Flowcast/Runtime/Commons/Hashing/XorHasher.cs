﻿using System;

namespace Flowcast.Commons
{
    public class XorHasher : IHasher
    {
        private uint _hash;

        public void Write(int value)
        {
            _hash ^= (uint)value;
        }

        public void Write(uint value)
        {
            _hash ^= value;
        }

        public void Write(long value)
        {
            _hash ^= (uint)(value & 0xFFFFFFFF);
            _hash ^= (uint)(value >> 32);
        }

        public void Write(ulong value)
        {
            _hash ^= (uint)(value & 0xFFFFFFFF);
            _hash ^= (uint)(value >> 32);
        }

        public void Write(float value)
        {
            var bits = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            _hash ^= bits;
        }

        public void Write(double value)
        {
            var bits = BitConverter.ToUInt64(BitConverter.GetBytes(value), 0);
            Write(bits);
        }

        public void Write(bool value)
        {
            _hash ^= value ? 1u : 0u;
        }

        public void Write(char value)
        {
            _hash ^= value;
        }

        public void Write(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (char c in value)
                Write(c);
        }

        public void WriteBytes(byte[] data)
        {
            for (int i = 0; i < data.Length; i += 4)
            {
                uint chunk = 0;

                if (i + 3 < data.Length)
                    chunk = BitConverter.ToUInt32(data, i);
                else
                {
                    for (int j = 0; j < data.Length - i; j++)
                    {
                        chunk |= (uint)(data[i + j] << (8 * j));
                    }
                }

                Write(chunk);
            }
        }



        public uint GetHash()
        {
            return _hash;
        }

        public void Reset()
        {
            _hash = 0;
        }
    }


}

