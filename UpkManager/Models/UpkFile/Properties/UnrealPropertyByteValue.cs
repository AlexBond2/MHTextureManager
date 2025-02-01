using System;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyByteValue : UnrealPropertyNameValue
    {

        #region Properties

        private byte? byteValue { get; set; }

        #endregion Properties

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.ByteProperty;

        public override object PropertyValue => byteValue ?? base.PropertyValue;

        public override string PropertyString => byteValue.HasValue ? $"{byteValue.Value}" : base.PropertyString;

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            if (size == 8) await base.ReadPropertyValue(reader, size, header).ConfigureAwait(false);
            else byteValue = reader.ReadByte();
        }

        public override void SetPropertyValue(object value)
        {
            UnrealNameTableIndex index = value as UnrealNameTableIndex;

            if (index == null)
            {
                UnrealNameTableEntry entry = value as UnrealNameTableEntry;

                if (entry != null)
                {
                    index = new UnrealNameTableIndex();

                    index.SetNameTableIndex(entry);
                }
            }

            if (index != null)
            {
                NameIndexValue = index;

                return;
            }

            if (value is bool && byteValue.HasValue) byteValue = Convert.ToByte(value);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = byteValue.HasValue ? sizeof(byte) : NameIndexValue.GetBuilderSize();

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            if (byteValue.HasValue) Writer.WriteByte(byteValue.Value);
            else await NameIndexValue.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
