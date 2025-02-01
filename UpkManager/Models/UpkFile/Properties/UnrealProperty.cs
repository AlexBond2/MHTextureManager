using System;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealProperty : UnrealUpkBuilderBase
    {

        #region Constructor

        public UnrealProperty()
        {
            NameIndex = new UnrealNameTableIndex();

            TypeNameIndex = new UnrealNameTableIndex();
        }

        #endregion Constructor

        #region Properties

        public UnrealNameTableIndex NameIndex { get; }

        public UnrealNameTableIndex TypeNameIndex { get; }

        public int Size { get; private set; }

        public int ArrayIndex { get; private set; }

        public UnrealPropertyValueBase Value { get; private set; }

        #endregion Properties

        #region Unreal Methods

        public async Task ReadProperty(ByteArrayReader reader, UnrealHeader header)
        {
            await Task.Run(() => NameIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);

            if (NameIndex.Name == ObjectTypes.None.ToString()) return;

            await Task.Run(() => TypeNameIndex.ReadNameTableIndex(reader, header)).ConfigureAwait(false);

            Size = reader.ReadInt32();
            ArrayIndex = reader.ReadInt32();

            Value = propertyValueFactory();

            await Value.ReadPropertyValue(reader, Size, header).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = NameIndex.GetBuilderSize();

            if (NameIndex.Name == ObjectTypes.None.ToString()) return BuilderSize;

            BuilderSize += TypeNameIndex.GetBuilderSize()
                        + sizeof(int) * 2;

            Size = Value.GetBuilderSize();

            return BuilderSize + Size;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await NameIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);

            if (NameIndex.Name == ObjectTypes.None.ToString()) return;

            await TypeNameIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);

            Writer.WriteInt32(Size);

            Writer.WriteInt32(ArrayIndex);

            await Value.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

        #region Private Methods

        private UnrealPropertyValueBase propertyValueFactory()
        {
            PropertyTypes type;

            Enum.TryParse(TypeNameIndex?.Name, true, out type);

            return type switch
            {
                PropertyTypes.BoolProperty => new UnrealPropertyBoolValue(),
                PropertyTypes.IntProperty => new UnrealPropertyIntValue(),
                PropertyTypes.FloatProperty => new UnrealPropertyFloatValue(),
                PropertyTypes.ObjectProperty => new UnrealPropertyObjectValue(),
                PropertyTypes.InterfaceProperty => new UnrealPropertyInterfaceValue(),
                PropertyTypes.ComponentProperty => new UnrealPropertyComponentValue(),
                PropertyTypes.ClassProperty => new UnrealPropertyClassValue(),
                PropertyTypes.GuidProperty => new UnrealPropertyGuidValue(),
                PropertyTypes.NameProperty => new UnrealPropertyNameValue(),
                PropertyTypes.ByteProperty => new UnrealPropertyByteValue(),
                PropertyTypes.StrProperty => new UnrealPropertyStringValue(),
                PropertyTypes.StructProperty => new UnrealPropertyStructValue(),
                PropertyTypes.ArrayProperty => new UnrealPropertyArrayValue(),
                _ => new UnrealPropertyValueBase(),
            };
        }

        #endregion Private Methods

    }

}
