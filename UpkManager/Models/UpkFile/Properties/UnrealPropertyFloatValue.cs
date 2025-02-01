using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyFloatValue : UnrealPropertyValueBase
    {

        #region Properties

        private float floatValue { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.FloatProperty;

        public override object PropertyValue => floatValue;

        public override string PropertyString => $"{floatValue}";

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            floatValue = await Task.Run(() => reader.ReadSingle()).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(float);

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await Task.Run(() => Writer.WriteSingle(floatValue)).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
