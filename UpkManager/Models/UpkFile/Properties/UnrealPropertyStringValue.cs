using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyStringValue : UnrealPropertyValueBase
    {

        #region Constructor

        public UnrealPropertyStringValue()
        {
            stringValue = new UnrealString();
        }

        #endregion Constructor

        #region Properties

        private UnrealString stringValue { get; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.StrProperty;

        public override object PropertyValue => stringValue;

        public override string PropertyString => stringValue.String;

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            await stringValue.ReadString(reader).ConfigureAwait(false);
        }

        public override void SetPropertyValue(object value)
        {
            if (value is not string str) return;

            stringValue.SetString(str);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = stringValue.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await stringValue.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
