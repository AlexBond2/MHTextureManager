using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Tables
{

    public sealed class UnrealNameTableEntry : UnrealUpkBuilderBase
    {

        #region Constructor

        public UnrealNameTableEntry()
        {
            Name = new UnrealString();
        }

        #endregion Constructor

        #region Properties

        public UnrealString Name { get; private set; }

        public ulong Flags { get; private set; }

        #endregion Properties

        #region Unreal Properties

        public int TableIndex { get; set; }

        #endregion Unreal Properties

        #region Unreal Methods

        public async Task ReadNameTableEntry(ByteArrayReader reader)
        {
            await Name.ReadString(reader).ConfigureAwait(false);

            Flags = reader.ReadUInt64();
        }

        public void SetNameTableEntry(UnrealString name, ulong flags, int index)
        {
            Name = name;

            Flags = flags;

            TableIndex = index;
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = Name.GetBuilderSize()
                        + sizeof(ulong);

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await Name.WriteBuffer(Writer, 0).ConfigureAwait(false);

            Writer.WriteUInt64(Flags);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
