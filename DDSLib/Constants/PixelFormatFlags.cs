using System;


namespace DDSLib.Constants
{

    [Flags]
    internal enum PixelFormatFlags
    {

        FourCC = 0x00000004,
        RGB = 0x00000040,
        RGBA = 0x00000041,
        Gray = 0x00020000,
        VU = 0x00080000

    }

}
