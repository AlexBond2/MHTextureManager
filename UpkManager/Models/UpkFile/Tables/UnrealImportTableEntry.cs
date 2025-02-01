using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Tables
{

    public sealed class UnrealImportTableEntry : UnrealObjectTableEntryBase
    {

        #region Constructor

        public UnrealImportTableEntry()
        {
            PackageNameIndex = new UnrealNameTableIndex();
            TypeNameIndex = new UnrealNameTableIndex();
            NameTableIndex = new UnrealNameTableIndex();
        }

        #endregion Constructor

        #region Properties

        public UnrealNameTableIndex PackageNameIndex { get; }

        public UnrealNameTableIndex TypeNameIndex { get; }
        //
        // OwnerReference in ObjectTableEntryBase
        //
        // NameTableIndex in ObjectTableEntryBase
        //
        #endregion Properties

        #region Unreal Properties

        public UnrealNameTableIndex OwnerReferenceNameIndex { get; set; }

        #endregion Unreal Properties

        #region Unreal Methods

        public async Task ReadImportTableEntry(ByteArrayReader reader, UnrealHeader header)
        {
            await Task.Run(() => PackageNameIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);

            await Task.Run(() => TypeNameIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);

            OwnerReference = reader.ReadInt32();

            await Task.Run(() => NameTableIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);
        }

        public void ExpandReferences(UnrealHeader header)
        {
            OwnerReferenceNameIndex = header.GetObjectTableEntry(OwnerReference)?.NameTableIndex;
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = PackageNameIndex.GetBuilderSize()
                        + TypeNameIndex.GetBuilderSize()
                        + sizeof(int)
                        + NameTableIndex.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await PackageNameIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);

            await TypeNameIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);

            Writer.WriteInt32(OwnerReference);

            await NameTableIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
