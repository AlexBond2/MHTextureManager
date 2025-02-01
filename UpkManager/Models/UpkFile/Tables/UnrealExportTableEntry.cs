using System;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Objects;
using UpkManager.Models.UpkFile.Objects.Sounds;
using UpkManager.Models.UpkFile.Objects.Textures;


namespace UpkManager.Models.UpkFile.Tables
{

    public sealed class UnrealExportTableEntry : UnrealExportTableEntryBuilderBase
    {

        #region Constructor

        internal UnrealExportTableEntry()
        {
            NameTableIndex = new UnrealNameTableIndex();
        }

        #endregion Constructor

        #region Properties

        public int TypeReference { get; private set; }

        public int ParentReference { get; private set; }
        //
        // OwnerReference in ObjectTableEntryBase
        //
        // NameTableIndex in ObjectTableEntryBase
        //
        public int ArchetypeReference { get; private set; }

        public uint FlagsHigh { get; private set; }

        public uint FlagsLow { get; private set; }

        public int SerialDataSize { get; private set; }

        public int SerialDataOffset { get; private set; }

        public uint ExportFlags { get; private set; }

        public int NetObjectCount { get; private set; }

        public byte[] Guid { get; private set; }

        public uint Unknown1 { get; private set; }

        public byte[] Unknown2 { get; private set; } // 4 * NetObjectCount bytes

        #endregion Properties

        #region Unreal Properties

        public ByteArrayReader UnrealObjectReader { get; private set; }

        public UnrealObjectBase UnrealObject { get; private set; }

        public UnrealNameTableIndex TypeReferenceNameIndex { get; private set; }

        public UnrealNameTableIndex ParentReferenceNameIndex { get; private set; }

        public UnrealNameTableIndex OwnerReferenceNameIndex { get; private set; }

        public UnrealNameTableIndex ArchetypeReferenceNameIndex { get; private set; }

        #endregion Unreal Properties

        #region Unreal Methods

        internal async Task ReadExportTableEntry(ByteArrayReader reader, UnrealHeader header)
        {
            TypeReference = reader.ReadInt32();
            ParentReference = reader.ReadInt32();
            OwnerReference = reader.ReadInt32();

            NameTableIndex.ReadNameTableIndex(reader, header);

            ArchetypeReference = reader.ReadInt32();

            FlagsHigh = reader.ReadUInt32();
            FlagsLow = reader.ReadUInt32();

            SerialDataSize = reader.ReadInt32();
            SerialDataOffset = reader.ReadInt32();

            ExportFlags = reader.ReadUInt32();

            NetObjectCount = reader.ReadInt32();

            Guid = await reader.ReadBytes(16).ConfigureAwait(false);

            Unknown1 = reader.ReadUInt32();

            Unknown2 = await reader.ReadBytes(sizeof(uint) * NetObjectCount).ConfigureAwait(false);
        }

        internal void DecodePointer(uint code1, int code2, int index)
        {
            uint size = (uint)SerialDataSize;
            uint offset = (uint)SerialDataOffset;

            decodePointer(ref size, code1, code2, index);
            decodePointer(ref offset, code1, code2, index);

            SerialDataSize = (int)size;
            SerialDataOffset = (int)offset;
        }

        internal void EncodePointer(uint code1, int code2, int index)
        {
            uint size = (uint)SerialDataSize;
            uint offset = (uint)SerialDataOffset;

            encodePointer(ref size, code1, code2, index);
            encodePointer(ref offset, code1, code2, index);

            BuilderSerialDataSize = (int)size;
            BuilderSerialDataOffset = (int)offset;
        }

        internal void ExpandReferences(UnrealHeader header)
        {
            TypeReferenceNameIndex = header.GetObjectTableEntry(TypeReference)?.NameTableIndex;
            ParentReferenceNameIndex = header.GetObjectTableEntry(ParentReference)?.NameTableIndex;
            OwnerReferenceNameIndex = header.GetObjectTableEntry(OwnerReference)?.NameTableIndex;
            ArchetypeReferenceNameIndex = header.GetObjectTableEntry(ArchetypeReference)?.NameTableIndex;
        }

        internal async Task ReadUnrealObject(ByteArrayReader reader)
        {
            UnrealObjectReader = await reader.Splice(SerialDataOffset, SerialDataSize).ConfigureAwait(false);
        }

        public async Task ParseUnrealObject(UnrealHeader header, bool skipProperties, bool skipParse)
        {
            UnrealObject = objectTypeFactory();

            await UnrealObject.ReadUnrealObject(UnrealObjectReader, header, this, skipProperties, skipParse).ConfigureAwait(false);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int) * 7
                        + sizeof(uint) * 4
                        + NameTableIndex.GetBuilderSize()
                        + Guid.Length
                        + Unknown2.Length;

            return BuilderSize;
        }

        public override int GetObjectSize(int CurrentOffset)
        {
            if (UnrealObject == null) throw new Exception("All objects in file must be fully parsed before writing back to disk.");

            SerialDataOffset = BuilderSerialDataOffset = CurrentOffset;

            SerialDataSize = BuilderSerialDataSize = UnrealObject.GetBuilderSize();

            return BuilderSerialDataSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            Writer.WriteInt32(TypeReference);
            Writer.WriteInt32(ParentReference);
            Writer.WriteInt32(OwnerReference);

            await NameTableIndex.WriteBuffer(Writer, 0).ConfigureAwait(false);

            Writer.WriteInt32(ArchetypeReference);

            Writer.WriteUInt32(FlagsHigh);
            Writer.WriteUInt32(FlagsLow);

            Writer.WriteInt32(BuilderSerialDataSize);
            Writer.WriteInt32(BuilderSerialDataOffset);

            Writer.WriteUInt32(ExportFlags);

            Writer.WriteInt32(NetObjectCount);

            await Writer.WriteBytes(Guid).ConfigureAwait(false);

            Writer.WriteUInt32(Unknown1);

            await Writer.WriteBytes(Unknown2).ConfigureAwait(false);
        }

        public override async Task<ByteArrayWriter> WriteObjectBuffer()
        {
            ByteArrayWriter writer = ByteArrayWriter.CreateNew(SerialDataSize);

            await UnrealObject.WriteBuffer(writer, SerialDataOffset).ConfigureAwait(false);

            return writer;
        }

        #endregion UnrealUpkBuilderBase Implementation

        #region Private Methods

        private UnrealObjectBase objectTypeFactory()
        {
            Enum.TryParse(TypeReferenceNameIndex?.Name, true, out ObjectTypes type);

            if (type == ObjectTypes.Unknown && TypeReferenceNameIndex != null)
            {
                if (TypeReferenceNameIndex.Name.StartsWith("CustomUIComp", StringComparison.CurrentCultureIgnoreCase) ||
                    TypeReferenceNameIndex.Name.StartsWith("Distribution", StringComparison.CurrentCultureIgnoreCase) ||
                    TypeReferenceNameIndex.Name.StartsWith("UIComp", StringComparison.CurrentCultureIgnoreCase) ||
                    TypeReferenceNameIndex.Name.EndsWith("Component", StringComparison.CurrentCultureIgnoreCase)) type = ObjectTypes.ArchetypeObjectReference;
            }

            return type switch
            {
                ObjectTypes.ArchetypeObjectReference => new UnrealObjectArchetypeBase(),
                ObjectTypes.ObjectRedirector => new UnrealObjectObjectRedirector(),
                ObjectTypes.ShadowMapTexture2D => new UnrealObjectShadowMapTexture2D(),
                ObjectTypes.SoundNodeWave => new UnrealObjectSoundNodeWave(),
                ObjectTypes.Texture2D => new UnrealObjectTexture2D(),
                ObjectTypes.TextureMovie => new UnrealObjectTextureMovie(),
                _ => new UnrealObjectBase(),
            };
        }

        private static void decodePointer(ref uint value, uint code1, int code2, int index)
        {
            uint tmp1 = ror32(value, (index + code2) & 0x1f);
            uint tmp2 = ror32(code1, index % 32);

            value = tmp2 ^ tmp1;
        }

        private static void encodePointer(ref uint value, uint code1, int code2, int index)
        {
            uint tmp2 = ror32(code1, index % 32);

            uint tmp1 = value ^ tmp2;

            value = rol32(tmp1, (index + code2) & 0x1f);
        }

        private static uint ror32(uint val, int shift)
        {
            return (val >> shift) | (val << (32 - shift));
        }

        private static uint rol32(uint val, int shift)
        {
            return (val >> (32 - shift)) | (val << shift);
        }

        #endregion Private Methods

    }

}
