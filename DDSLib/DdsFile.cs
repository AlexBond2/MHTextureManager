using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

using DDSLib.Compression;
using DDSLib.Constants;
using DDSLib.Extensions;


namespace DDSLib
{

    public sealed class DdsFile
    {

        #region Private Fields

        private DdsHeader header;

        private byte[] largestMipMap;

        #endregion Private Fields

        #region Constructors

        public DdsFile() { }

        public DdsFile(string filename, bool header = false)
        {
            Load(File.OpenRead(filename), header);
        }

        public DdsFile(Stream stream)
        {
            Load(stream);
        }

        #endregion Constructors

        #region Public Properties

        public int Width => (int)header.Width;

        public int Height => (int)header.Height;

        public FileFormat FileFormat { get; private set; }
        public List<DdsMipMap> MipMaps { get; private set; }

        public BitmapSource BitmapSource => new RgbaBitmapSource(largestMipMap, Width);


        #endregion Public Properties

        #region Public Methods

        public void RegenMipMaps(int count)
    {
            using var input = new MemoryStream();
            input.Write(largestMipMap);
            input.Position = 0;

            if ((header.PixelFormat.Flags & (int)PixelFormatFlags.FourCC) != 0)
            {
                SquishFlags squishFlags = FileFormat switch
                {
                    FileFormat.DXT1 => SquishFlags.Dxt1,
                    FileFormat.DXT3 => SquishFlags.Dxt3,
                    FileFormat.DXT5 => SquishFlags.Dxt5,
                    _ => SquishFlags.Unknown
                };

                largestMipMap = ReadCompressedMipMap(input, Width, Height, squishFlags, false);
            } 
            else
            {
                largestMipMap = ReadFormatMipMap(input, Width, Height, FileFormat, false);
            }

            GenerateMipMaps(4, 4, count);

            var saveConfig = new DdsSaveConfig(FileFormat, 0, 0, false, false);

            foreach (var mipMap in MipMaps)
                mipMap.MipMap = WriteMipMap(mipMap, saveConfig);
        }

        public void GenerateMipMaps(int minMipWidth = 1, int minMipHeight = 1, int mipMax = 0)
        {
            int mipCount = mipMax > 0 ? mipMax : DdsHeader.CountMipMaps(Width, Height);

            int mipWidth = Width;
            int mipHeight = Height;

            MipMaps = [new DdsMipMap(Width, Height, largestMipMap)];

            for (int mipLoop = 1; mipLoop < mipCount; mipLoop++)
            {
                if (mipWidth > minMipWidth) mipWidth /= 2;
                if (mipHeight > minMipHeight) mipHeight /= 2;

                var writeSize = new DdsMipMap(mipWidth, mipHeight);

                var mipMap = new WriteableBitmap(BitmapSource);

                writeSize.MipMap = mipMap.ResizeHighQuality(writeSize.Width, writeSize.Height).ConvertToRgba();

                MipMaps.Add(writeSize);
            }
        }

        public void Load(Stream input, bool onlyHeader = false)
        {
            using var reader = new BinaryReader(input);
            //
            // Read the DDS tag. If it's not right, then bail..
            //
            uint signature = reader.ReadUInt32();

            if (signature != HeaderValues.DdsSignature) throw new FormatException("File does not appear to be a DDS image");

            header = new DdsHeader();
            //
            // Read everything in.. for now assume it worked like a charm..
            //
            header.Read(reader);
            MipMaps = [];

            if ((header.PixelFormat.Flags & (int)PixelFormatFlags.FourCC) != 0)
            {
                SquishFlags squishFlags;

                switch (header.PixelFormat.FourCC)
                {
                    case FourCCFormat.Dxt1:
                        {
                            squishFlags = SquishFlags.Dxt1;
                            FileFormat = FileFormat.DXT1;
                            break;
                        }
                    case FourCCFormat.Dxt3:
                        {
                            squishFlags = SquishFlags.Dxt3;
                            FileFormat = FileFormat.DXT3;
                            break;
                        }

                    case FourCCFormat.Dxt5:
                        {
                            squishFlags = SquishFlags.Dxt5;
                            FileFormat = FileFormat.DXT5;
                            break;
                        }
                    default:
                        {
                            throw new FormatException("File is not a supported DDS format");
                        }
                }

                ReadCompressedMipMaps(input, squishFlags, onlyHeader);
            }
            else
            {
                //
                // We can only deal with the non-DXT formats we know about..  this is a bit of a mess..
                // Sorry..
                //
                var fileFormat = FileFormat.Unknown;
                var pixelFormat = header.PixelFormat;

                if ((pixelFormat.Flags == (int)PixelFormatFlags.RGBA) && (pixelFormat.RgbBitCount == 32) &&
                    (pixelFormat.RBitMask == 0x00ff0000) && (pixelFormat.GBitMask == 0x0000ff00) &&
                    (pixelFormat.BBitMask == 0x000000ff) && (pixelFormat.ABitMask == 0xff000000)) fileFormat = FileFormat.A8R8G8B8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGB) && (pixelFormat.RgbBitCount == 32) &&
                         (pixelFormat.RBitMask == 0x00ff0000) && (pixelFormat.GBitMask == 0x0000ff00) &&
                         (pixelFormat.BBitMask == 0x000000ff) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.X8R8G8B8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGBA) && (pixelFormat.RgbBitCount == 32) &&
                         (pixelFormat.RBitMask == 0x000000ff) && (pixelFormat.GBitMask == 0x0000ff00) &&
                         (pixelFormat.BBitMask == 0x00ff0000) && (pixelFormat.ABitMask == 0xff000000)) fileFormat = FileFormat.A8B8G8R8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGB) && (pixelFormat.RgbBitCount == 32) &&
                         (pixelFormat.RBitMask == 0x000000ff) && (pixelFormat.GBitMask == 0x0000ff00) &&
                         (pixelFormat.BBitMask == 0x00ff0000) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.X8B8G8R8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGBA) && (pixelFormat.RgbBitCount == 16) &&
                         (pixelFormat.RBitMask == 0x00007c00) && (pixelFormat.GBitMask == 0x000003e0) &&
                         (pixelFormat.BBitMask == 0x0000001f) && (pixelFormat.ABitMask == 0x00008000)) fileFormat = FileFormat.A1R5G5B5;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGBA) && (pixelFormat.RgbBitCount == 16) &&
                         (pixelFormat.RBitMask == 0x00000f00) && (pixelFormat.GBitMask == 0x000000f0) &&
                         (pixelFormat.BBitMask == 0x0000000f) && (pixelFormat.ABitMask == 0x0000f000)) fileFormat = FileFormat.A4R4G4B4;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGB) && (pixelFormat.RgbBitCount == 24) &&
                         (pixelFormat.RBitMask == 0x00ff0000) && (pixelFormat.GBitMask == 0x0000ff00) &&
                         (pixelFormat.BBitMask == 0x000000ff) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.R8G8B8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.RGB) && (pixelFormat.RgbBitCount == 16) &&
                         (pixelFormat.RBitMask == 0x0000f800) && (pixelFormat.GBitMask == 0x000007e0) &&
                         (pixelFormat.BBitMask == 0x0000001f) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.R5G6B5;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.Gray) && (pixelFormat.RgbBitCount == 8) &&
                         (pixelFormat.RBitMask == 0x000000ff) && (pixelFormat.GBitMask == 0x00000000) &&
                         (pixelFormat.BBitMask == 0x00000000) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.G8;
                else if ((pixelFormat.Flags == (int)PixelFormatFlags.VU) && (pixelFormat.RgbBitCount == 16) &&
                         (pixelFormat.RBitMask == 0x000000ff) && (pixelFormat.GBitMask == 0x0000ff00) &&
                         (pixelFormat.BBitMask == 0x00000000) && (pixelFormat.ABitMask == 0x00000000)) fileFormat = FileFormat.V8U8;
                //
                // If fileFormat is still invalid, then it's an unsupported format.
                //
                if (fileFormat == FileFormat.Unknown) throw new FormatException("File is not a supported DDS format");
                FileFormat = fileFormat;

                ReadFormatMipMaps(input, fileFormat, onlyHeader);         
            }

            if (MipMaps.Count > 0) largestMipMap = MipMaps[0].MipMap;
        }

        private byte[] ReadFormatMipMap(Stream input, int width, int height, FileFormat fileFormat, bool onlyHeader)
        {
            //
            // Size of a source pixel, in bytes
            //
            int srcPixelSize = ((int)header.PixelFormat.RgbBitCount / 8);
            //
            // We need the pitch for a row, so we can allocate enough memory for the load.
            //
            int rowPitch;

            if ((header.HeaderFlags & (int)HeaderFlags.Pitch) != 0)
            {
                //
                // Pitch specified.. so we can use directly
                //
                rowPitch = (int)header.PitchOrLinearSize;
            }
            else if ((header.HeaderFlags & (int)HeaderFlags.LinearSize) != 0)
            {
                //
                // Linear size specified.. compute row pitch. Of course, this should never happen
                // as linear size is *supposed* to be for compressed textures. But Microsoft don't
                // always play by the rules when it comes to DDS output.
                //
                rowPitch = (int)header.PitchOrLinearSize / (int)height;
            }
            else
            {
                //
                // Another case of Microsoft not obeying their standard is the 'Convert to..' shell extension
                // that ships in the DirectX SDK. Seems to always leave flags empty..so no indication of pitch
                // or linear size. And - to cap it all off - they leave pitchOrLinearSize as *zero*. Zero??? If
                // we get this bizarre set of inputs, we just go 'screw it' and compute row pitch ourselves.
                //
                rowPitch = (int)width * srcPixelSize;
            }
            //
            // Ok.. now, we need to allocate room for the bytes to read in from.. it's rowPitch bytes * height
            //
            byte[] readPixelData = new byte[rowPitch * height];

            input.Read(readPixelData, 0, readPixelData.GetLength(0));

            if (onlyHeader) return readPixelData;
            //
            // We now need space for the real pixel data.. that's width * height * 4..
            //
            var mipmap = new byte[width * height * 4];
            //
            // And now we have the arduous task of filling that up with stuff..
            //
            for (int destY = 0; destY < (int)height; destY++)
            {
                for (int destX = 0; destX < (int)width; destX++)
                {
                    //
                    // Compute source pixel offset
                    //
                    int srcPixelOffset = destY * rowPitch + destX * srcPixelSize;
                    //
                    // Read our pixel
                    //
                    uint pixelColour = 0;
                    uint pixelRed = 0;
                    uint pixelGreen = 0;
                    uint pixelBlue = 0;
                    uint pixelAlpha = 0;
                    //
                    // Build our pixel colour as a DWORD
                    //
                    for (int loop = 0; loop < srcPixelSize; loop++) pixelColour |= (uint)(readPixelData[srcPixelOffset + loop] << (8 * loop));

                    switch (fileFormat)
                    {
                        case FileFormat.A8R8G8B8:
                            {
                                pixelAlpha = (pixelColour >> 24) & 0xff;
                                pixelRed = (pixelColour >> 16) & 0xff;
                                pixelGreen = (pixelColour >> 8) & 0xff;
                                pixelBlue = (pixelColour >> 0) & 0xff;

                                break;
                            }
                        case FileFormat.X8R8G8B8:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = (pixelColour >> 16) & 0xff;
                                pixelGreen = (pixelColour >> 8) & 0xff;
                                pixelBlue = (pixelColour >> 0) & 0xff;

                                break;
                            }
                        case FileFormat.A8B8G8R8:
                            {
                                pixelAlpha = (pixelColour >> 24) & 0xff;
                                pixelRed = (pixelColour >> 0) & 0xff;
                                pixelGreen = (pixelColour >> 8) & 0xff;
                                pixelBlue = (pixelColour >> 16) & 0xff;

                                break;
                            }
                        case FileFormat.X8B8G8R8:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = (pixelColour >> 0) & 0xff;
                                pixelGreen = (pixelColour >> 8) & 0xff;
                                pixelBlue = (pixelColour >> 16) & 0xff;

                                break;
                            }
                        case FileFormat.A1R5G5B5:
                            {
                                pixelAlpha = (pixelColour >> 15) & 0xff;
                                pixelRed = (pixelColour >> 10) & 0x1f;
                                pixelGreen = (pixelColour >> 5) & 0x1f;
                                pixelBlue = (pixelColour >> 0) & 0x1f;

                                pixelRed = (pixelRed << 3) | (pixelRed >> 2);
                                pixelGreen = (pixelGreen << 3) | (pixelGreen >> 2);
                                pixelBlue = (pixelBlue << 3) | (pixelBlue >> 2);

                                break;
                            }
                        case FileFormat.A4R4G4B4:
                            {
                                pixelAlpha = (pixelColour >> 12) & 0xff;
                                pixelRed = (pixelColour >> 8) & 0x0f;
                                pixelGreen = (pixelColour >> 4) & 0x0f;
                                pixelBlue = (pixelColour >> 0) & 0x0f;

                                pixelAlpha = (pixelAlpha << 4) | (pixelAlpha >> 0);
                                pixelRed = (pixelRed << 4) | (pixelRed >> 0);
                                pixelGreen = (pixelGreen << 4) | (pixelGreen >> 0);
                                pixelBlue = (pixelBlue << 4) | (pixelBlue >> 0);

                                break;
                            }
                        case FileFormat.R8G8B8:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = (pixelColour >> 16) & 0xff;
                                pixelGreen = (pixelColour >> 8) & 0xff;
                                pixelBlue = (pixelColour >> 0) & 0xff;

                                break;
                            }
                        case FileFormat.V8U8:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = (byte)(((sbyte)(pixelColour & 0xff)) + 128);
                                pixelGreen = (byte)(((sbyte)((pixelColour >> 8) & 0xff)) + 128);
                                pixelBlue = 0xff;

                                break;
                            }
                        case FileFormat.R5G6B5:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = (pixelColour >> 11) & 0x1f;
                                pixelGreen = (pixelColour >> 5) & 0x3f;
                                pixelBlue = (pixelColour >> 0) & 0x1f;

                                pixelRed = (pixelRed << 3) | (pixelRed >> 2);
                                pixelGreen = (pixelGreen << 2) | (pixelGreen >> 4);
                                pixelBlue = (pixelBlue << 3) | (pixelBlue >> 2);

                                break;
                            }
                        case FileFormat.G8:
                            {
                                pixelAlpha = 0xff;

                                pixelRed = pixelGreen = pixelBlue = pixelColour & 0xff;

                                break;
                            }
                    }
                    //
                    // Write the colours away..
                    //
                    int destPixelOffset = destY * (int)width * 4 + destX * 4;

                    mipmap[destPixelOffset + 0] = (byte)pixelRed;
                    mipmap[destPixelOffset + 1] = (byte)pixelGreen;
                    mipmap[destPixelOffset + 2] = (byte)pixelBlue;
                    mipmap[destPixelOffset + 3] = (byte)pixelAlpha;
                }
            }
            
            return mipmap;
        }

        private byte[] ReadCompressedMipMap(Stream input, int width, int height, SquishFlags squishFlags, bool onlyHeader = false)
        {
            //
            // Compute size of compressed block area
            //
            int blockCount = (width + 3) / 4 * ((height + 3) / 4);
            int blockSize = (squishFlags & SquishFlags.Dxt1) != 0 ? 8 : 16;
            //
            // Allocate room for compressed blocks, and read data into it.
            //
            byte[] compressedBlocks = new byte[blockCount * blockSize];

            input.Read(compressedBlocks, 0, compressedBlocks.GetLength(0));

            if (onlyHeader) return compressedBlocks;

            //
            // Now decompress..
            //
            return DdsSquish.DecompressImage(width, height, compressedBlocks, squishFlags, null);
        }

        private void ReadFormatMipMaps(Stream input, FileFormat fileFormat, bool onlyHeader)
        {
            int count = (int)header.MipMapCount;
            if (count == 0) count = 1;

            int width = Width;
            int heigth = Height;

            while (count > 0)
            {
                var mipMap = ReadFormatMipMap(input, width, heigth, fileFormat, onlyHeader);
                MipMaps.Add(new(width, heigth, mipMap));

                if (width > 1) width /= 2;
                if (heigth > 1) heigth /= 2;

                count--;
            }
        }

        private void ReadCompressedMipMaps(Stream input, SquishFlags squishFlags, bool onlyHeader)
        {
            int count = (int)header.MipMapCount;
            if (count == 0) count = 1;

            int width = Width;
            int heigth = Height;

            while (count > 0)
            {
                var mipMap = ReadCompressedMipMap(input, width, heigth, squishFlags, onlyHeader);
                MipMaps.Add(new(width, heigth, mipMap));

                if (width > 1) width /= 2;
                if (heigth > 1) heigth /= 2;
                count--;
            }
        }

        public void Save(Stream output, DdsSaveConfig saveConfig)
        {
            BinaryWriter writer = new BinaryWriter(output);

            header = new DdsHeader(saveConfig, Width, Height);

            header.Write(writer);

            if (saveConfig.GenerateMipMaps) GenerateMipMaps();

            foreach (DdsMipMap mipMap in MipMaps.OrderByDescending(mip => mip.Width))
            {
                byte[] outputData = WriteMipMap(mipMap, saveConfig);

                output.Write(outputData, 0, outputData.Length);
            }

            output.Flush();
        }

        public byte[] WriteMipMap(DdsMipMap mipMap, DdsSaveConfig saveConfig)
        {
            byte[] outputData;

            if (saveConfig.FileFormat >= FileFormat.DXT1 && saveConfig.FileFormat <= FileFormat.DXT5)
            {
                outputData = DdsSquish.CompressImage(mipMap.MipMap, mipMap.Width, mipMap.Height, saveConfig.GetSquishFlags(), null);
            }
            else
            {
                int pixelWidth = (int)header.PitchOrLinearSize / Width;

                int mipPitch = pixelWidth * mipMap.Width;

                outputData = new byte[mipPitch * mipMap.Height];

                outputData.Initialize();

                for (int i = 0; i < mipMap.MipMap.Length; i += 4)
                {
                    uint pixelData = 0;

                    byte R = mipMap.MipMap[i + 0];
                    byte G = mipMap.MipMap[i + 1];
                    byte B = mipMap.MipMap[i + 2];
                    byte A = mipMap.MipMap[i + 3];

                    switch (saveConfig.FileFormat)
                    {
                        case FileFormat.A8R8G8B8:
                            {
                                pixelData = ((uint)A << 24) |
                                            ((uint)R << 16) |
                                            ((uint)G << 8) |
                                            ((uint)B << 0);
                                break;
                            }
                        case FileFormat.X8R8G8B8:
                            {
                                pixelData = ((uint)R << 16) |
                                            ((uint)G << 8) |
                                            ((uint)B << 0);
                                break;
                            }
                        case FileFormat.A8B8G8R8:
                            {
                                pixelData = ((uint)A << 24) |
                                            ((uint)B << 16) |
                                            ((uint)G << 8) |
                                            ((uint)R << 0);
                                break;
                            }
                        case FileFormat.X8B8G8R8:
                            {
                                pixelData = ((uint)B << 16) |
                                            ((uint)G << 8) |
                                            ((uint)R << 0);
                                break;
                            }
                        case FileFormat.A1R5G5B5:
                            {
                                pixelData = ((uint)(A != 0 ? 1 : 0) << 15) |
                                            ((uint)(R >> 3) << 10) |
                                            ((uint)(G >> 3) << 5) |
                                            ((uint)(B >> 3) << 0);
                                break;
                            }
                        case FileFormat.A4R4G4B4:
                            {
                                pixelData = ((uint)(A >> 4) << 12) |
                                            ((uint)(R >> 4) << 8) |
                                            ((uint)(G >> 4) << 4) |
                                            ((uint)(B >> 4) << 0);
                                break;
                            }
                        case FileFormat.R8G8B8:
                            {
                                pixelData = ((uint)R << 16) |
                                            ((uint)G << 8) |
                                            ((uint)B << 0);
                                break;
                            }
                        case FileFormat.V8U8:
                            {
                                sbyte u = (sbyte)(R - 128);
                                sbyte v = (sbyte)(G - 128);

                                pixelData = ((uint)(byte)u) |
                                            ((uint)(byte)v << 8);
                                break;
                            }
                        case FileFormat.R5G6B5:
                            {
                                pixelData = ((uint)(R >> 3) << 11) |
                                            ((uint)(G >> 2) << 5) |
                                            ((uint)(B >> 3) << 0);
                                break;
                            }
                        case FileFormat.G8:
                            {
                                pixelData = (uint)((R + G + B) / 3.0 + 0.5);

                                break;
                            }
                    }

                    int pixelOffset = i / 4 * pixelWidth;

                    for (int j = 0; j < pixelWidth; j++) outputData[pixelOffset + j] = (byte)((pixelData >> (8 * j)) & 0xff);
                }
            }

            return outputData;
        }

        #endregion Public Methods

    }

}
