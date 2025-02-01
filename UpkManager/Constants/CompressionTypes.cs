using System;

namespace UpkManager.Constants
{

    [Flags]
    public enum CompressionTypes : uint
    {

        ZLIB = 0x00000001,
        LZO = 0x00000002,
        LZX = 0x00000004,
        LZO_ENC = 0x00000008

    }

}
