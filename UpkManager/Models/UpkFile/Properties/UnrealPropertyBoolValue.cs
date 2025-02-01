using System;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Properties
{

    internal sealed class UnrealPropertyBoolValue : UnrealPropertyValueBase
    {

        #region Properties

        private uint boolValue { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.BoolProperty;

        public override object PropertyValue => boolValue;

        public override string PropertyString => $"{boolValue != 0}";

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            boolValue = await Task.Run(() => reader.ReadUInt32()).ConfigureAwait(false);
        }

        public override void SetPropertyValue(object value)
        {
            if (!(value is bool)) return;

            boolValue = Convert.ToUInt32((bool)value);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(uint);

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await Task.Run(() => Writer.WriteUInt32(boolValue)).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
