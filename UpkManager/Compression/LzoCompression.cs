using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace UpkManager.Compression
{
    public sealed class LzoCompression : ILzoCompression
    {

        #region Private Fields

        private const long WorkMemorySize_1x_1 = 16384L * 4;

        private static readonly bool is64Bit;

        #endregion Private Fields

        #region Constructor

        static LzoCompression()
        {
            is64Bit = IntPtr.Size == 8;
        }

        public LzoCompression()
        {
            int init = Lzo2.lzo_init_64(1, -1, -1, -1, -1, -1, -1, -1, -1, -1);

            if (init != 0) throw new Exception("Initialization of lzo2.dll failed.");
        }

        #endregion Constructor

        #region ILzoCompression Implementation

        public string Version
        {
            get
            {
                IntPtr strPtr = Lzo2.lzo_version_string_64();

                string version = Marshal.PtrToStringAnsi(strPtr);

                return version;
            }
        }

        public string VersionDate => Lzo2.lzo_version_date();

        public async Task<byte[]> Compress(byte[] Source)
        {
            byte[] compressed = new byte[Source.Length + Source.Length / 64 + 16 + 3 + 4];

            int compressedSize = 0;

            await Task.Run(() =>
            {
                byte[] workMemory = new byte[WorkMemorySize_1x_1];

                Lzo2.lzo1x_1_compress_64(Source, Source.Length, compressed, ref compressedSize, workMemory);
            }).ConfigureAwait(false);

            byte[] sizedToFit = new byte[compressedSize];

            await Task.Run(() => Array.ConstrainedCopy(compressed, 0, sizedToFit, 0, compressedSize)).ConfigureAwait(false);

            return sizedToFit;
        }

        public async Task Decompress(byte[] Source, byte[] Destination)
        {
            await Task.Run(() =>
            {
                int destinationSize = Destination.Length;

                Lzo2.lzo1x_decompress_64(Source, Source.Length, Destination, ref destinationSize, null);
            }).ConfigureAwait(false);
        }

        #endregion ILzoCompression Implementation

    }

}
