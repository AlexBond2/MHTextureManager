using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyStructValue : UnrealPropertyValueBase
    {

        #region Constructor

        public UnrealPropertyStructValue()
        {
            StructNameIndex = new UnrealNameTableIndex();
        }

        #endregion Constructor

        #region Properties

        public UnrealNameTableIndex StructNameIndex { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.StructProperty;

        public override string PropertyString => StructNameIndex.Name;

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            await Task.Run(() => StructNameIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);

            await base.ReadPropertyValue(reader, size, header).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = StructNameIndex.GetBuilderSize()
                        + base.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await StructNameIndex.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);

            await base.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
