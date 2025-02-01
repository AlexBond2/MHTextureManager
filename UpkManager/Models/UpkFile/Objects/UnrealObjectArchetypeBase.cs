using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Objects
{

    public class UnrealObjectArchetypeBase : UnrealObjectBase
    {

        #region Properties

        public int ArchetypeObjectReference { get; private set; } // This is still just a guess but it is related to the export object having an ArchetypeReference and it seems to point to a type.

        #endregion Properties

        #region Unreal Properties

        public override ObjectTypes ObjectType => ObjectTypes.ArchetypeObjectReference;

        public UnrealNameTableIndex ArchetypeObjectNameIndex { get; private set; }

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadUnrealObject(ByteArrayReader reader, UnrealHeader header, UnrealExportTableEntry export, bool skipProperties, bool skipParse)
        {
            ArchetypeObjectReference = reader.ReadInt32();

            ArchetypeObjectNameIndex = header.GetObjectTableEntry(ArchetypeObjectReference)?.NameTableIndex;

            await base.ReadUnrealObject(reader, header, export, skipProperties, skipParse).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int)
                        + base.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            Writer.WriteInt32(ArchetypeObjectReference);

            await base.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
