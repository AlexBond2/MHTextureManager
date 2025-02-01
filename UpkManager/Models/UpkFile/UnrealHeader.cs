using Str.Common.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Compression;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile
{

    public sealed class UnrealHeader : UnrealHeaderBuilderBase
    {

        #region Private Fields

        private ByteArrayReader reader;

        private ByteArrayWriter writer;

        #endregion Private Fields

        #region Constructor

        public UnrealHeader(ByteArrayReader Reader)
        {
            reader = Reader;

            Group = new UnrealString();

            GenerationTable = new List<UnrealGenerationTableEntry>();

            CompressedChunks = new List<UnrealCompressedChunk>();

            NameTable = new List<UnrealNameTableEntry>();

            ExportTable = new List<UnrealExportTableEntry>();
            ImportTable = new List<UnrealImportTableEntry>();
        }

        #endregion Constructor

        #region Properties

        public uint Signature { get; private set; }

        public ushort Version { get; private set; }

        public ushort Licensee { get; private set; }

        public int Size { get; private set; }

        public UnrealString Group { get; }

        public uint Flags { get; private set; }

        public int NameTableCount { get; private set; }

        public int NameTableOffset { get; private set; }

        public int ExportTableCount { get; private set; }

        public int ExportTableOffset { get; private set; }

        public int ImportTableCount { get; private set; }

        public int ImportTableOffset { get; private set; }

        public int DependsTableOffset { get; private set; }

        public byte[] Guid { get; private set; }

        public int GenerationTableCount { get; private set; }

        public List<UnrealGenerationTableEntry> GenerationTable { get; private set; }

        public uint EngineVersion { get; private set; }

        public uint CookerVersion { get; private set; }

        public uint CompressionFlags { get; private set; }

        public int CompressionTableCount { get; private set; }

        public List<UnrealCompressedChunk> CompressedChunks { get; private set; }

        public uint Unknown1 { get; private set; }

        public uint Unknown2 { get; private set; }

        public List<UnrealNameTableEntry> NameTable { get; }

        public List<UnrealExportTableEntry> ExportTable { get; }

        public List<UnrealImportTableEntry> ImportTable { get; }

        public byte[] DependsTable { get; private set; } // (Size - DependsOffset) bytes; or ExportTableCount * 4 bytes;

        #endregion Properties

        #region Unreal Properties

        public string FullFilename { get; set; }

        public string Filename => Path.GetFileName(FullFilename);

        public long FileSize { get; set; }

        #endregion Unreal Properties

        #region Unreal Methods

        public async Task ReadHeaderAsync(Action<UnrealLoadProgress> progress)
        {
            UnrealLoadProgress message = new UnrealLoadProgress { Text = "Parsing Header..." };

            progress?.Invoke(message);

            await readUpkHeader().ConfigureAwait(false);

            const CompressionTypes validCompression = CompressionTypes.LZO | CompressionTypes.LZO_ENC;

            if (((CompressionTypes)CompressionFlags & validCompression) > 0)
            {
                message.Text = "Decompressing...";

                progress?.Invoke(message);

                reader = await decompressChunks().ConfigureAwait(false);
            }
            else if (CompressionFlags > 0) throw new Exception($"Unsupported compression type 0x{CompressionFlags:X8}.");

            await readNameTable(progress).ConfigureAwait(false);

            await readImportTable(progress).ConfigureAwait(false);

            await readExportTable(progress).ConfigureAwait(false);

            message.Text = "Slicing and Dicing...";

            progress?.Invoke(message);

            await readDependsTable().ConfigureAwait(false);

            await decodePointers().ConfigureAwait(false);

            message.Text = "Reading Objects...";
            message.Total = ExportTableCount;

            progress?.Invoke(message);

            await ExportTable.ForEachAsync(export =>
            {
                return export.ReadUnrealObject(reader).ContinueWith(t =>
                {
                    message.IncrementCurrent();

                    if (ExportTableCount > 100) progress?.Invoke(message);
                });
            }).ConfigureAwait(false);

            message.IsComplete = true;

            progress?.Invoke(message);
        }

        public UnrealObjectTableEntryBase GetObjectTableEntry(int reference)
        {
            if (reference == 0) return null;

            if (reference < 0 && -reference - 1 < ImportTableCount) return ImportTable[-reference - 1];
            if (reference > 0 && reference - 1 < ExportTableCount) return ExportTable[reference - 1];

            throw new Exception($"Object reference ({reference:X8}) is out of range of both the Import and Export Tables.");
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            if (CompressedChunks.Any()) throw new NotSupportedException("Cannot rebuild compressed files. Yet.");

            BuilderSize = sizeof(uint) * 7
                        + sizeof(ushort) * 2
                        + sizeof(int) * 10
                        + Group.GetBuilderSize()
                        + Guid.Length
                        + GenerationTable.Sum(gen => gen.GetBuilderSize());

            BuilderNameTableOffset = BuilderSize;

            BuilderSize += NameTable.Sum(name => name.GetBuilderSize());

            BuilderImportTableOffset = BuilderSize;

            BuilderSize += ImportTable.Sum(import => import.GetBuilderSize());

            BuilderExportTableOffset = BuilderSize;

            BuilderSize += ExportTable.Sum(export => export.GetBuilderSize());

            BuilderDependsTableOffset = BuilderSize;

            BuilderSize += DependsTable.Length;

            ExportTable.Aggregate(BuilderSize, (current, export) => current + export.GetObjectSize(current));

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            writer = Writer;

            await writeUpkHeader().ConfigureAwait(false);

            await writeNameTable().ConfigureAwait(false);

            await writeImportTable().ConfigureAwait(false);

            await encodePointers().ConfigureAwait(false);

            await writeExportTable().ConfigureAwait(false);

            await writeDependsTable().ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

        #region Private Methods

        private async Task readUpkHeader()
        {
            reader.Seek(0);

            Signature = reader.ReadUInt32();

            if (Signature != Signatures.Signature) throw new Exception("File is not a properly formatted UPK file.");

            Version = reader.ReadUInt16();
            Licensee = reader.ReadUInt16();

            Size = reader.ReadInt32();

            await Group.ReadString(reader).ConfigureAwait(false);

            Flags = reader.ReadUInt32();

            NameTableCount = reader.ReadInt32();
            NameTableOffset = reader.ReadInt32();

            ExportTableCount = reader.ReadInt32();
            ExportTableOffset = reader.ReadInt32();

            ImportTableCount = reader.ReadInt32();
            ImportTableOffset = reader.ReadInt32();

            DependsTableOffset = reader.ReadInt32();

            Guid = await reader.ReadBytes(16).ConfigureAwait(false);

            GenerationTableCount = reader.ReadInt32();

            GenerationTable = await readGenerationTable().ConfigureAwait(false);

            EngineVersion = reader.ReadUInt32();
            CookerVersion = reader.ReadUInt32();

            CompressionFlags = reader.ReadUInt32();

            CompressionTableCount = reader.ReadInt32();

            CompressedChunks = await readCompressedChunksTable().ConfigureAwait(false);

            Unknown1 = reader.ReadUInt32();
            Unknown2 = reader.ReadUInt32();
        }

        private async Task writeUpkHeader()
        {
            writer.Seek(0);

            writer.WriteUInt32(Signature);

            writer.WriteUInt16(Version);
            writer.WriteUInt16(Licensee);

            writer.WriteInt32(BuilderSize);

            await Group.WriteBuffer(writer, 0).ConfigureAwait(false);

            writer.WriteUInt32(Flags);

            writer.WriteInt32(NameTable.Count);
            writer.WriteInt32(BuilderNameTableOffset);

            writer.WriteInt32(ExportTable.Count);
            writer.WriteInt32(BuilderExportTableOffset);

            writer.WriteInt32(ImportTable.Count);
            writer.WriteInt32(BuilderImportTableOffset);

            writer.WriteInt32(BuilderDependsTableOffset);

            await writer.WriteBytes(Guid).ConfigureAwait(false);

            writer.WriteInt32(GenerationTable.Count);

            await writeGenerationTable().ConfigureAwait(false);

            writer.WriteUInt32(EngineVersion);
            writer.WriteUInt32(CookerVersion);

            writer.WriteUInt32(CompressionFlags);

            writer.WriteInt32(CompressedChunks.Count);

            writer.WriteUInt32(Unknown1);
            writer.WriteUInt32(Unknown2);
        }

        private async Task<List<UnrealGenerationTableEntry>> readGenerationTable()
        {
            List<UnrealGenerationTableEntry> generations = new List<UnrealGenerationTableEntry>();

            for (int i = 0; i < GenerationTableCount; ++i)
            {
                UnrealGenerationTableEntry info = new UnrealGenerationTableEntry();

                await Task.Run(() => info.ReadGenerationTableEntry(reader)).ConfigureAwait(false);

                generations.Add(info);
            }

            return generations;
        }

        private async Task writeGenerationTable()
        {
            foreach (UnrealGenerationTableEntry entry in GenerationTable)
            {
                await entry.WriteBuffer(writer, 0).ConfigureAwait(false);
            }
        }

        private async Task<List<UnrealCompressedChunk>> readCompressedChunksTable()
        {
            List<UnrealCompressedChunk> chunks = new List<UnrealCompressedChunk>();

            for (int i = 0; i < CompressionTableCount; ++i)
            {
                UnrealCompressedChunk chunk = new UnrealCompressedChunk();

                await chunk.ReadCompressedChunk(reader).ConfigureAwait(false);

                chunks.Add(chunk);
            }

            return chunks;
        }

        private async Task<ByteArrayReader> decompressChunks()
        {
            int start = CompressedChunks.Min(ch => ch.UncompressedOffset);

            int totalSize = CompressedChunks.SelectMany(ch => ch.Header.Blocks).Aggregate(start, (total, block) => total + block.UncompressedSize);

            byte[] data = new byte[totalSize];

            foreach (UnrealCompressedChunk chunk in CompressedChunks)
            {
                byte[] chunkData = new byte[chunk.Header.Blocks.Sum(block => block.UncompressedSize)];

                int uncompressedOffset = 0;

                foreach (UnrealCompressedChunkBlock block in chunk.Header.Blocks)
                {
                    if (((CompressionTypes)CompressionFlags & CompressionTypes.LZO_ENC) > 0) await block.CompressedData.Decrypt().ConfigureAwait(false);

                    byte[] decompressed = await block.CompressedData.Decompress(block.UncompressedSize).ConfigureAwait(false);

                    int offset = uncompressedOffset;

                    await Task.Run(() => Array.ConstrainedCopy(decompressed, 0, chunkData, offset, block.UncompressedSize)).ConfigureAwait(false);

                    uncompressedOffset += block.UncompressedSize;
                }

                await Task.Run(() => Array.ConstrainedCopy(chunkData, 0, data, chunk.UncompressedOffset, chunk.Header.UncompressedSize)).ConfigureAwait(false);
            }

            return ByteArrayReader.CreateNew(data, start);
        }

        private async Task readNameTable(Action<UnrealLoadProgress> progress)
        {
            UnrealLoadProgress message = new UnrealLoadProgress { Text = "Reading Name Table...", Current = 0, Total = NameTableCount };

            reader.Seek(NameTableOffset);

            for (int i = 0; i < NameTableCount; ++i)
            {
                UnrealNameTableEntry name = new UnrealNameTableEntry { TableIndex = i };

                await name.ReadNameTableEntry(reader).ConfigureAwait(false);

                NameTable.Add(name);

                message.IncrementCurrent();

                if (NameTableCount > 100) progress?.Invoke(message);
            }
        }

        private async Task writeNameTable()
        {
            foreach (UnrealNameTableEntry entry in NameTable)
            {
                await entry.WriteBuffer(writer, 0).ConfigureAwait(false);
            }
        }

        private async Task readImportTable(Action<UnrealLoadProgress> progress)
        {
            UnrealLoadProgress message = new UnrealLoadProgress { Text = "Reading Import Table...", Current = 0, Total = ImportTableCount };

            reader.Seek(ImportTableOffset);

            for (int i = 0; i < ImportTableCount; ++i)
            {
                UnrealImportTableEntry import = new UnrealImportTableEntry { TableIndex = -(i + 1) };

                await import.ReadImportTableEntry(reader, this).ConfigureAwait(false);

                ImportTable.Add(import);

                message.IncrementCurrent();

                if (ImportTableCount > 100) progress?.Invoke(message);
            }

            message.Text = "Expanding References...";
            message.Current = 0;
            message.Total = 0;

            progress?.Invoke(message);

            await ImportTable.ForEachAsync(import => Task.Run(() => import.ExpandReferences(this))).ConfigureAwait(false);
        }

        private async Task writeImportTable()
        {
            foreach (UnrealImportTableEntry entry in ImportTable)
            {
                await entry.WriteBuffer(writer, 0).ConfigureAwait(false);
            }
        }

        private async Task readExportTable(Action<UnrealLoadProgress> progress)
        {
            UnrealLoadProgress message = new UnrealLoadProgress { Text = "Reading Export Table...", Current = 0, Total = ExportTableCount };

            reader.Seek(ExportTableOffset);

            for (int i = 0; i < ExportTableCount; ++i)
            {
                UnrealExportTableEntry export = new UnrealExportTableEntry { TableIndex = i + 1 };

                await export.ReadExportTableEntry(reader, this).ConfigureAwait(false);

                ExportTable.Add(export);

                message.IncrementCurrent();

                if (ExportTableCount > 100) progress?.Invoke(message);
            }

            message.Text = "Expanding References...";
            message.Current = 0;
            message.Total = 0;

            progress?.Invoke(message);

            await ExportTable.ForEachAsync(export => Task.Run(() => export.ExpandReferences(this))).ConfigureAwait(false);
        }

        private async Task writeExportTable()
        {
            foreach (UnrealExportTableEntry entry in ExportTable)
            {
                await entry.WriteBuffer(writer, 0).ConfigureAwait(false);
            }
        }

        private async Task readDependsTable()
        {
            reader.Seek(DependsTableOffset);

            DependsTable = await reader.ReadBytes(ExportTableCount * sizeof(uint)).ConfigureAwait(false);
        }

        private async Task writeDependsTable()
        {
            byte[] bytes = Enumerable.Repeat((byte)0, ExportTable.Count * sizeof(uint)).ToArray();

            await writer.WriteBytes(bytes).ConfigureAwait(false);
        }

        #region External Code

        /// <summary>
        /// https://github.com/gildor2/UModel/blob/c871f9d534e0bd42a17b4d4268c0ecc59dd7191e/Unreal/UnPackage.cpp#L1274
        /// </summary>
        private async Task decodePointers()
        {
            uint code1 = (((uint)Size & 0xffu) << 24)
                       | (((uint)NameTableCount & 0xffu) << 16)
                       | (((uint)NameTableOffset & 0xffu) << 8)
                       | ((uint)ExportTableCount & 0xffu);

            int code2 = (ExportTableOffset + ImportTableCount + ImportTableOffset) & 0x1f;

            await Task.Run(() =>
            {
                for (int i = 0; i < ExportTable.Count; ++i) ExportTable[i].DecodePointer(code1, code2, i);
            }).ConfigureAwait(false);
        }

        private async Task encodePointers()
        {
            uint code1 = (((uint)BuilderSize & 0xffu) << 24)
                       | (((uint)NameTable.Count & 0xffu) << 16)
                       | (((uint)BuilderNameTableOffset & 0xffu) << 8)
                       | ((uint)ExportTable.Count & 0xffu);

            int code2 = (BuilderExportTableOffset + ImportTable.Count + BuilderImportTableOffset) & 0x1f;

            await Task.Run(() =>
            {
                for (int i = 0; i < ExportTable.Count; ++i) ExportTable[i].EncodePointer(code1, code2, i);
            }).ConfigureAwait(false);
        }

        #endregion External Code

        #endregion Private Methods

    }

}
