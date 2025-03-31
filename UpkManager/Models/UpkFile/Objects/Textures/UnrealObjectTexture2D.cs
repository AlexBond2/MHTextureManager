using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using DDSLib;
using DDSLib.Constants;

using UpkManager.Constants;
using UpkManager.Extensions;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Compression;
using UpkManager.Models.UpkFile.Properties;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Objects.Textures
{

    public class UnrealObjectTexture2D : UnrealObjectCompressionBase
    {

        #region Constructor

        public UnrealObjectTexture2D()
        {
            MipMaps = new();
        }

        #endregion Constructor

        #region Properties

        public int MipMapsCount { get; private set; }

        public List<UnrealMipMap> MipMaps { get; }

        public byte[] Guid { get; private set; }

        #endregion Properties

        #region Unreal Properties

        public override bool IsExportable => true;

        public override ViewableTypes Viewable => ViewableTypes.Image;

        public override ObjectTypes ObjectType => ObjectTypes.Texture2D;

        public override string FileExtension => ".dds";

        public override string FileTypeDesc => "Direct Draw Surface";

        #endregion Unreal Properties

        #region Unreal Methods

        public override async Task ReadUnrealObject(ByteArrayReader reader, UnrealHeader header, UnrealExportTableEntry export, bool skipProperties, bool skipParse)
        {
            await base.ReadUnrealObject(reader, header, export, skipProperties, skipParse).ConfigureAwait(false);

            if (skipParse) return;

            MipMapsCount = reader.ReadInt32();

            for (int i = 0; i < MipMapsCount; ++i)
            {
                await ProcessCompressedBulkData(reader, async bulkChunk =>
                {
                    UnrealMipMap mip = new UnrealMipMap
                    {
                        Width = reader.ReadInt32(),
                        Height = reader.ReadInt32()
                    };

                    if (mip.Width >= 4 || mip.Height >= 4) 
                        mip.ImageData = (await bulkChunk.DecompressChunk(0).ConfigureAwait(false))?.GetBytes();

                    MipMaps.Add(mip);
                }).ConfigureAwait(false);
            }

            Guid = await reader.ReadBytes(16).ConfigureAwait(false);
        }

        public void ResetMipMaps(int count)
        {
            MipMaps.Clear();
            MipMapsCount = count;
        }

        public async Task ReadMipMapCache(ByteArrayReader upkReader, uint index, UnrealMipMap overrideMipMap)
        {
            var header = new UnrealCompressedChunkHeader();

            await header.ReadCompressedChunkHeader(upkReader, 1, 0, 0).ConfigureAwait(false);

            if (TryGetImageProperties(header, (int)index, overrideMipMap, out int width, out int height, out FileFormat format))
            {
                UnrealMipMap mip = new()
                {
                    Width = width,
                    Height = height,
                    OverrideFormat = format
                };

                if (mip.Width >= 4 || mip.Height >= 4) 
                    mip.ImageData = (await header.DecompressChunk().ConfigureAwait(false))?.GetBytes();

                MipMaps.Add(mip);
            }
        }

        public bool TryGetImageProperties(UnrealCompressedChunkHeader header, int index, UnrealMipMap overrideMipMap, out int width, out int height, out FileFormat ddsFormat)
        {
            if (overrideMipMap.Width > 0)
            {
                int shift = index - overrideMipMap.ImageData[0];
                if (shift < 0)
                {
                    width = overrideMipMap.Width << -shift;
                    height = overrideMipMap.Height << -shift;
                }
                else
                {
                    width = overrideMipMap.Width >> shift;
                    height = overrideMipMap.Height >> shift;
                }
                ddsFormat = overrideMipMap.OverrideFormat;
                return true;
            }

            switch (header.Blocks.Count)
            {
                case 2:
                    width = 512;
                    height = 512;
                    ddsFormat = FileFormat.DXT5;
                    break;
                case 4:
                    width = 1024;
                    height = 1024;
                    ddsFormat = FileFormat.DXT1;
                    break;
                case 8:
                    width = 1024;
                    height = 1024;
                    ddsFormat = FileFormat.DXT5;
                    break;
                case 16:
                    width = 2048;
                    height = 2048;
                    ddsFormat = FileFormat.DXT1;
                    break;
                case 32:
                    width = 2048;
                    height = 2048;
                    ddsFormat = FileFormat.DXT5;
                    break;
                case 64:
                    width = 4096;
                    height = 4096;
                    ddsFormat = FileFormat.DXT1;
                    break;
                case 128:
                    width = 4096;
                    height = 4096;
                    ddsFormat = FileFormat.DXT5;
                    break;
                default:
                    switch (header.Blocks[0].UncompressedSize)
                    {
                        case 0x0200:
                            width = 32;
                            height = 32;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x0800:
                            width = 64;
                            height = 64;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x0FF0:
                            width = 68;
                            height = 60;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x1000:
                            width = 64;
                            height = 64;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x2000:
                            width = 128;
                            height = 128;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x2420:
                            width = 136;
                            height = 136;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x2FD0:
                            width = 180;
                            height = 136;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x3200:
                            width = 158;
                            height = 158;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x4000:
                            width = 128;
                            height = 128;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x5F00:
                            width = 152;
                            height = 160;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x5FA0:
                            width = 180;
                            height = 136;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x8000:
                            width = 256;
                            height = 256;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0xBF40:
                            width = 360;
                            height = 272;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0xDAC0:
                            width = 280;
                            height = 400;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x10000:
                            width = 256;
                            height = 256;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x17C00:
                            width = 152;
                            height = 160;
                            ddsFormat = FileFormat.A8R8G8B8;
                            break;
                        case 0x1EC30:
                            width = 300;
                            height = 420;
                            ddsFormat = FileFormat.DXT5;
                            break;
                        case 0x1F5B8:
                            width = 676;
                            height = 380;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        case 0x20000:
                            width = 512;
                            height = 512;
                            ddsFormat = FileFormat.DXT1;
                            break;
                        default:
                            width = 0;
                            height = 0;
                            ddsFormat = FileFormat.Unknown;
                            return false;
                    }
                    break;
            }
            return true;
        }

        public override async Task SaveObject(string filename, object configuration)
        {
            if (MipMaps == null || !MipMaps.Any()) return;

            DdsSaveConfig config = configuration as DdsSaveConfig ?? new DdsSaveConfig(FileFormat.Unknown, 0, 0, false, false);

            FileFormat format;

            UnrealMipMap mipMap = MipMaps
                .Where(mm => mm.ImageData != null && mm.ImageData.Length > 0)
                .OrderByDescending(mm => mm.Width > mm.Height ? mm.Width : mm.Height).FirstOrDefault();

            if (mipMap == null) return;

            Stream memory = buildDdsImage(MipMaps.IndexOf(mipMap), out format);

            if (memory == null) return;

            DdsFile ddsImage = new DdsFile(memory);

            FileStream ddsStream = new FileStream(filename, FileMode.Create);

            config.FileFormat = format;

            await Task.Run(() => ddsImage.Save(ddsStream, config)).ConfigureAwait(false);

            ddsStream.Close();

            memory.Close();
        }

        public override async Task SetObject(string filename, List<UnrealNameTableEntry> nameTable, object configuration)
        {
            DdsSaveConfig config = configuration as DdsSaveConfig ?? new DdsSaveConfig(FileFormat.Unknown, 0, 0, false, false);

            DdsFile image = await Task.Run(() => new DdsFile(filename)).ConfigureAwait(false);

            bool skipFirstMip = false;

            int width = image.Width;
            int height = image.Height;

            if (MipMaps[0].ImageData == null || MipMaps[0].ImageData.Length == 0)
            {
                width *= 2;
                height *= 2;

                skipFirstMip = true;
            }

            UnrealPropertyIntValue sizeX = PropertyHeader.GetProperty("SizeX").FirstOrDefault()?.Value as UnrealPropertyIntValue;
            UnrealPropertyIntValue sizeY = PropertyHeader.GetProperty("SizeY").FirstOrDefault()?.Value as UnrealPropertyIntValue;

            sizeX?.SetPropertyValue(skipFirstMip ? width * 2 : width);
            sizeY?.SetPropertyValue(skipFirstMip ? height * 2 : height);

            UnrealPropertyIntValue mipTailBaseIdx = PropertyHeader.GetProperty("MipTailBaseIdx").FirstOrDefault()?.Value as UnrealPropertyIntValue;

            int indexSize = width > height ? width : height;

            mipTailBaseIdx?.SetPropertyValue((int)Math.Log(skipFirstMip ? indexSize * 2 : indexSize, 2));

            UnrealPropertyStringValue filePath = PropertyHeader.GetProperty("SourceFilePath").FirstOrDefault()?.Value as UnrealPropertyStringValue;
            UnrealPropertyStringValue fileTime = PropertyHeader.GetProperty("SourceFileTimestamp").FirstOrDefault()?.Value as UnrealPropertyStringValue;

            filePath?.SetPropertyValue(filename);
            fileTime?.SetPropertyValue(File.GetLastWriteTime(filename).ToString("yyyy-MM-dd hh:mm:ss"));

            UnrealPropertyByteValue pfFormat = PropertyHeader.GetProperty("Format").FirstOrDefault()?.Value as UnrealPropertyByteValue;

            FileFormat imageFormat = FileFormat.Unknown;

            if (pfFormat != null) imageFormat = DdsPixelFormat.ParseFileFormat(pfFormat.PropertyString);

            if (imageFormat == FileFormat.Unknown) throw new Exception($"Unknown DDS File Format ({pfFormat?.PropertyString ?? "Unknown"}).");

            if (config.FileFormat == FileFormat.Unknown) config.FileFormat = imageFormat;
            else
            {
                string formatStr = DdsPixelFormat.BuildFileFormat(config.FileFormat);

                UnrealNameTableEntry formatTableEntry = nameTable.SingleOrDefault(nt => nt.Name.String == formatStr) ?? nameTable.AddUnrealNameTableEntry(formatStr);

                pfFormat?.SetPropertyValue(formatTableEntry);
            }

            MipMaps.Clear();

            if (skipFirstMip)
            {
                MipMaps.Add(new UnrealMipMap
                {
                    ImageData = null,
                    Width = width,
                    Height = height
                });
            }

            image.GenerateMipMaps(4, 4);

            foreach (DdsMipMap mipMap in image.MipMaps.OrderByDescending(mip => mip.Width))
            {
                MipMaps.Add(new UnrealMipMap
                {
                    ImageData = image.WriteMipMap(mipMap, config),
                    Width = mipMap.Width,
                    Height = mipMap.Height
                });
            }
        }

        public override Stream GetObjectStream()
        {
            if (MipMaps == null || MipMaps.Count == 0) return null;

            FileFormat format;

            UnrealMipMap mipMap = MipMaps
                .Where(mm => mm.ImageData != null && mm.ImageData.Length > 0)
                .OrderByDescending(mm => mm.Width > mm.Height ? mm.Width : mm.Height)
                .FirstOrDefault();

            return mipMap == null ? null : buildDdsImage(MipMaps.IndexOf(mipMap), out format);
        }

        public Stream GetMipMapsStream()
        {
            if (MipMaps == null || MipMaps.Count == 0) return null;

            var orderedMipMaps = MipMaps.OrderByDescending(mip => mip.Width);

            UnrealMipMap mipMap = orderedMipMaps.FirstOrDefault();

            var ddsHeader = new DdsHeader(new DdsSaveConfig(mipMap.OverrideFormat, 0, 0, false, false), mipMap.Width, mipMap.Height, MipMaps.Count);
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            ddsHeader.Write(writer);
            foreach (var map in orderedMipMaps)
                stream.Write(map.ImageData, 0, map.ImageData.Length);

            stream.Flush();
            stream.Position = 0;

            return stream;
        }

        public Stream GetObjectStream(int mipMapIndex)
        {
            FileFormat format;
            return buildDdsImage(mipMapIndex, out format);
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public byte[] WriteMipMapChache(int index)
        {
            if (index >= MipMaps.Count) return [];

            // build compressed Chunks
            int dataSize = GetCompressedMipMapSize(index);
            if (dataSize == 0) return [];

            // write header and chunks
            var writer = ByteArrayWriter.CreateNew(dataSize);
            if (CompressedChunks.Count <= index) return [];
            CompressedChunks[index].Header.WriteCompressedChunkHeader(writer, 0).Wait();

            // write compressed data in stream
            return writer.GetBytes();
        }

        public override int GetBuilderSize()
        {
            BuilderSize = PropertyHeader.GetBuilderSize()
                        + base.GetBuilderSize()
                        + sizeof(int);

            foreach (UnrealMipMap mipMap in MipMaps)
            {
                BulkDataCompressionTypes flags = mipMap.ImageData == null || 
                    mipMap.ImageData.Length == 0 
                    ? BulkDataCompressionTypes.Unused | BulkDataCompressionTypes.StoreInSeparatefile 
                    : BulkDataCompressionTypes.LZO_ENC;

                BuilderSize += Task.Run(() => ProcessUncompressedBulkData(ByteArrayReader.CreateNew(mipMap.ImageData, 0), flags)).Result
                            + sizeof(int) * 2;
            }

            BuilderSize += Guid.Length;

            return BuilderSize;
        }

        public int GetCompressedMipMapSize(int index)
        {
            if (index >= MipMaps.Count) return 0;

            BuilderSize = base.GetBuilderSize() + sizeof(int); // need sizeof(int)?
            var mipMap = MipMaps[index];

            BulkDataCompressionTypes flags = mipMap.ImageData == null ||
                mipMap.ImageData.Length == 0
                ? BulkDataCompressionTypes.Unused | BulkDataCompressionTypes.StoreInSeparatefile
                : BulkDataCompressionTypes.LZO_ENC;

            BuilderSize += Task.Run(() => ProcessUncompressedBulkData(ByteArrayReader.CreateNew(mipMap.ImageData, 0), flags)).Result
                        + sizeof(int) * 2;

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await PropertyHeader.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);

            await base.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);

            Writer.WriteInt32(MipMaps.Count);

            for (int i = 0; i < MipMaps.Count; ++i)
            {
                await CompressedChunks[i].WriteCompressedChunk(Writer, CurrentOffset).ConfigureAwait(false);

                Writer.WriteInt32(MipMaps[i].Width);
                Writer.WriteInt32(MipMaps[i].Height);
            }

            await Writer.WriteBytes(Guid).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

        #region Private Methods

        private Stream buildDdsImage(int mipMapIndex, out FileFormat imageFormat)
        {
            var mipMap = MipMaps[mipMapIndex];

            if (mipMap.OverrideFormat == FileFormat.Unknown)
            {
                imageFormat = FileFormat.Unknown;
                if (PropertyHeader.GetProperty("Format").FirstOrDefault()?.Value is not UnrealPropertyByteValue formatProp) return null;
                string format = formatProp.PropertyString;
                imageFormat = DdsPixelFormat.ParseFileFormat(format);
            } 
            else
            {
                imageFormat = mipMap.OverrideFormat;
            }

            var ddsHeader = new DdsHeader(new DdsSaveConfig(imageFormat, 0, 0, false, false), mipMap.Width, mipMap.Height);
            var stream = new MemoryStream();
            var writer = new BinaryWriter(stream);

            ddsHeader.Write(writer);
            stream.Write(mipMap.ImageData, 0, mipMap.ImageData.Length);

            stream.Flush();
            stream.Position = 0;

            return stream;
        }

        public void ResetCompressedChunks()
        {
            CompressedChunks.Clear();
        }

        public void ExpandMipMaps(int count, List<DdsMipMap> mipMaps)
        {
            MipMapsCount = count;
            int maxIndex = MipMaps.Count;
            var format = MipMaps[0].OverrideFormat;
            for (int index = maxIndex; index < MipMapsCount; index++)
            {
                UnrealMipMap mip = new UnrealMipMap
                {
                    Width = mipMaps[index].Width,
                    Height = mipMaps[index].Height,
                    OverrideFormat = format
                };
                MipMaps.Add(mip);
            }
        }

        #endregion Private Methods

    }

}
