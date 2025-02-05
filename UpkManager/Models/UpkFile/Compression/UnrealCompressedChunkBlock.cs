using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Compression
{

    public class UnrealCompressedChunkBlock
    {

        #region Properties

        public int CompressedSize { get; set; }
        public int UncompressedSize { get; set; }
        public ByteArrayReader CompressedData { get; set; }

        #endregion Properties

        #region Unreal Methods

        public void ReadCompressedChunkBlock(ByteArrayReader reader)
        {
            CompressedSize = reader.ReadInt32();
            UncompressedSize = reader.ReadInt32();
        }

        public async Task ReadCompressedChunkBlockData(ByteArrayReader reader)
        {
            CompressedData = await reader.ReadByteArray(CompressedSize).ConfigureAwait(false);
        }

        public async Task<int> BuildCompressedChunkBlockData(ByteArrayReader reader)
        {
            UncompressedSize = reader.Remaining;

            byte[] compressed = await reader.Compress().ConfigureAwait(false);

            CompressedData = ByteArrayReader.CreateNew(compressed, 0);

            //await CompressedData.Encrypt().ConfigureAwait(false); // TODO: Fix this to use the flag

            CompressedSize = CompressedData.Remaining;

            return CompressedSize + sizeof(int) * 2;
        }

        public int BuildExistingCompressedChunkBlockData()
        {
            //await CompressedData.Encrypt().ConfigureAwait(false);

            return CompressedSize + sizeof(int) * 2;
        }

        public async Task WriteCompressedChunkBlock(ByteArrayWriter Writer)
        {
            await Task.Run(() =>
            {
                Writer.WriteInt32(CompressedSize);
                Writer.WriteInt32(UncompressedSize);
            }).ConfigureAwait(false);
        }

        public async Task WriteCompressedChunkBlockData(ByteArrayWriter Writer)
        {
            await Writer.WriteBytes(CompressedData.GetBytes()).ConfigureAwait(false);
        }

        #endregion Unreal Methods

    }

}
