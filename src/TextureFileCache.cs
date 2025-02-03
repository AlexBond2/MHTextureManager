using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Objects.Textures;

namespace MHTextureManager
{
    public class TextureFileCache
    {
        public UnrealObjectTexture2D Texture2D { get; }

        public TextureFileCache()
        {
            Texture2D = new();
        }

        public bool LoadFromFile(string filePath, TextureEntry entry)
        {
            if (!File.Exists(filePath)) return false;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            using (BinaryReader reader = new BinaryReader(fs))
            {
                Texture2D.ResetMipMaps(entry.Data.Maps.Count);
                foreach (var mipMap in entry.Data.Maps)
                {
                    if (mipMap.Offset + mipMap.Size > fs.Length)
                        return false;

                    fs.Seek(mipMap.Offset, SeekOrigin.Begin);
                    byte[] textureData = reader.ReadBytes((int)mipMap.Size);
                    var upkReader = ByteArrayReader.CreateNew(textureData, 0);
                    var overrideMipMap = entry.Data.OverrideMipMap;

                    Texture2D.ReadMipMapCache(upkReader, mipMap.Index, overrideMipMap).Wait();
                    break;
                }
            }
            return true;
        }
    }
}
