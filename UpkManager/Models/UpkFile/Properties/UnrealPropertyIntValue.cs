using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Properties
{

    public class UnrealPropertyIntValue : UnrealPropertyValueBase
    {

        #region Properties

        protected int IntValue { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.IntProperty;

        public override object PropertyValue => IntValue;

        public override string PropertyString => $"{IntValue:N0}";

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            IntValue = await Task.Run(() => reader.ReadInt32()).ConfigureAwait(false);
        }

        public override void SetPropertyValue(object value)
        {
            if (value is not int) return;

            IntValue = (int)value;
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int);

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await Task.Run(() => Writer.WriteInt32(IntValue)).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
