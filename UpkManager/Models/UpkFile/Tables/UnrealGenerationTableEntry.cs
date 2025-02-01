using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Tables
{

    public sealed class UnrealGenerationTableEntry : UnrealUpkBuilderBase
    {

        #region Properties

        public int ExportTableCount { get; private set; }

        public int NameTableCount { get; private set; }

        public int NetObjectCount { get; private set; }

        #endregion Properties

        #region Unreal Methods

        public void ReadGenerationTableEntry(ByteArrayReader data)
        {
            ExportTableCount = data.ReadInt32();
            NameTableCount = data.ReadInt32();
            NetObjectCount = data.ReadInt32();
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int) * 3;

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await Task.Run(() =>
            {
                Writer.WriteInt32(ExportTableCount);

                Writer.WriteInt32(NameTableCount);

                Writer.WriteInt32(NetObjectCount);
            }).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
