using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Properties
{

    public class UnrealPropertyNameValue : UnrealPropertyValueBase
    {

        #region Constructor

        public UnrealPropertyNameValue()
        {
            NameIndexValue = new UnrealNameTableIndex();
        }

        #endregion Constructor

        #region Properties

        protected UnrealNameTableIndex NameIndexValue { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.NameProperty;

        public override object PropertyValue => NameIndexValue;

        public override string PropertyString => NameIndexValue.Name;

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            await Task.Run(() => NameIndexValue.ReadNameTableIndex(reader, header)).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = NameIndexValue.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await NameIndexValue.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
