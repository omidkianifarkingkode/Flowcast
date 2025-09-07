// MessageBits.cs
namespace Application.Abstractions.Realtime.Routing;

/// <summary>
/// Layout: DDDDDD | VV | R | CCCCCCC  
/// (Domain[15..10], Version[9..8], Dir[7], Code[6..0])
/// </summary>
public static class MessageBits
{
    public const int SHIFT_DOMAIN = 10; // (15..10) 6 bits (0-63)
    public const int SHIFT_VER = 8;     // (9..8)   2 bits (0-3)
    public const int SHIFT_DIR = 7;     // (7)      1 bit  (0-1)
    public const int SHIFT_CODE = 0;    // (6..0)   7 bits (0-127)

    public const int DOMAIN_BITS = 6;
    public const int VER_BITS = 2;
    public const int DIR_BITS = 1;
    public const int CODE_BITS = 7;

    // Masks
    public const ushort DOMAIN_MASK = (ushort)(((1 << DOMAIN_BITS) - 1) << SHIFT_DOMAIN); // 0xFC00
    public const ushort VER_MASK =    (ushort)(((1 << VER_BITS)    - 1) << SHIFT_VER);    // 0x0300
    public const ushort DIR_MASK =    (ushort)(((1 << DIR_BITS)    - 1) << SHIFT_DIR);    // 0x0080
    public const ushort CODE_MASK =   (ushort)(((1 << CODE_BITS)   - 1) << SHIFT_CODE);   // 0x007F

    // Directions
    public const int DIR_REQ = 0;
    public const int DIR_PUSH = 1;
    public const int REQ = DIR_REQ << SHIFT_DIR; // 0
    public const int PSH = DIR_PUSH << SHIFT_DIR; // 1 << 7 (128)

    // Versions (logical V1..V4 mapped to encodings 0..3)
    public const int V1 = 0 << SHIFT_VER; // logical 1
    public const int V2 = 1 << SHIFT_VER; // logical 2
    public const int V3 = 2 << SHIFT_VER; // logical 3
    public const int V4 = 3 << SHIFT_VER; // logical 4
    public const byte DefaultVersion = 1; // logical default

    // Extractors
    public static byte GetDomain(ushort type) => (byte)((type & DOMAIN_MASK) >> SHIFT_DOMAIN);
    public static byte GetVer(ushort type)    => (byte)((type & VER_MASK) >> SHIFT_VER);  // 0..3 (map +1 if you print logical)
    public static int GetDir(ushort type)     => ((type & DIR_MASK) >> SHIFT_DIR);  // 0/1
    public static byte GetCode(ushort type)   => (byte)((type & CODE_MASK) >> SHIFT_CODE); // 0..127
}
