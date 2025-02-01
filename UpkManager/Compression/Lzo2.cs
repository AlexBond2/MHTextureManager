using System;
using System.Runtime.InteropServices;

namespace UpkManager.Compression
{

    internal static class Lzo2
    {

        #region Private Fields

        private const string LzoDll64 = @"lzo2_64.dll";

        #endregion Private Fields

        #region Imports

        #region 64 Bit

        [DllImport(LzoDll64, EntryPoint = "__lzo_init_v2")]
        internal static extern int lzo_init_64(uint v, int s1, int s2, int s3, int s4, int s5, int s6, int s7, int s8, int s9);

        [DllImport(LzoDll64, EntryPoint = "lzo_version_string")]
        internal static extern IntPtr lzo_version_string_64();

        [DllImport(LzoDll64)]
        internal static extern string lzo_version_date();

        [DllImport(LzoDll64, EntryPoint = "lzo1x_1_compress", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lzo1x_1_compress_64(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);

        [DllImport(LzoDll64, EntryPoint = "lzo1x_decompress_safe", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int lzo1x_decompress_64(byte[] src, int src_len, byte[] dst, ref int dst_len, byte[] wrkmem);

        #endregion 64 Bit

        #endregion Imports

    }

}
