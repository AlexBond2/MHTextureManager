using DDSLib;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Objects.Textures;

namespace MHTextureManager
{
    public enum ImportType
    {
        New = 0,
        Add = 1,
        Replace = 2,
    }

    public enum WriteResult
    {
        Success,
        MipMapError,
        SizeReplaceError,
    }

    public class TextureFileCache
    {
        public UnrealObjectTexture2D Texture2D { get; }

        public TextureEntry Entry { get; private set; }
        public bool Loaded { get; private set; }

        public TextureFileCache()
        {
            Texture2D = new();
        }

        public bool LoadFromFile(string filePath, TextureEntry entry)
        {
            if (Loaded && Entry == entry) return true;

            if (!File.Exists(filePath)) return false;

            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new BinaryReader(fs);

            bool onlyFirst = false;

            if (Entry != entry)
            {
                Texture2D.ResetMipMaps(entry.Data.Maps.Count);
                Loaded = false;
                onlyFirst = true;
                Entry = entry;
            }

            int mipmapIndex = 0;
            foreach (var mipMap in entry.Data.Maps)
            {
                if (0 == mipmapIndex++ && onlyFirst == false) continue; 

                if (mipMap.Offset + mipMap.Size > fs.Length)
                    return false;

                fs.Seek(mipMap.Offset, SeekOrigin.Begin);
                byte[] textureData = reader.ReadBytes((int)mipMap.Size);
                var upkReader = ByteArrayReader.CreateNew(textureData, 0);
                var overrideMipMap = entry.Data.OverrideMipMap;

                Texture2D.ReadMipMapCache(upkReader, mipMap.Index, overrideMipMap).Wait();
                if (onlyFirst) break;
            }

            Loaded = entry.Data.Maps.Count == Texture2D.MipMaps.Count;

            return true;
        }

        public WriteResult WriteTexture(string texturePath, string textureCacheName, ImportType importType, DdsFile ddsHeader)
        {
            string tfcPath = Path.Combine(texturePath, textureCacheName + ".tfc");

            using FileStream fs = importType switch
            {
                ImportType.New => new FileStream(tfcPath, FileMode.Create, FileAccess.Write),
                ImportType.Add => new FileStream(tfcPath, FileMode.Append, FileAccess.Write),
                ImportType.Replace => new FileStream(tfcPath, FileMode.Open, FileAccess.ReadWrite),
                _ => throw new ArgumentException("Invalid import type", nameof(importType))
            };

            int index = 0;

            if (Texture2D.MipMaps.Count <= index || ddsHeader.MipMaps.Count <= index)
                return WriteResult.MipMapError;

            Texture2D.ResetCompressedChunks();
            foreach (var mipMap in Entry.Data.Maps)
            {
                if (importType == ImportType.Replace)
                    fs.Seek(mipMap.Offset, SeekOrigin.Begin);

                Texture2D.MipMaps[index].ImageData = ddsHeader.MipMaps[index].MipMap;

                var data = Texture2D.WriteMipMapChache(index);

                if (data.Length == 0) return WriteResult.MipMapError;

                if (importType == ImportType.Replace && data.Length > mipMap.Size)
                    return WriteResult.SizeReplaceError;

                mipMap.Offset = (uint)fs.Position;
                fs.Write(data);
                mipMap.Size = (uint)data.Length;

                index++;
            }

            Entry.Data.TextureFileName = textureCacheName;

            return WriteResult.Success;
        }
    }
}
