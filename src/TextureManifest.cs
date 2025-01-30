using System.Text;

namespace MHTextureManager
{

    public class TextureManifest
    {
        public Dictionary<Guid, TextureEntry> Entries { get; private set; } = [];

        public List<TextureEntry> LoadManifestFromFile(string filePath)
        {
            Entries.Clear();

            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                uint size = reader.ReadUInt32();

                for (uint i = 0; i < size; i++)
                {
                    var entry = new TextureEntry(reader);
                    Entries[entry.Head.TextureGUID] = entry;
                }
            }

            return [.. Entries.Values.OrderBy(entry => entry.Head.TextureName)];
        }
    }

    public class TextureEntry
    {
        public TextureHead Head;
        public TextureMipMaps Data;

        public TextureEntry(BinaryReader reader)
        {
            Head = new (reader);
            Data = new (reader);
        }
    }

    public class TextureHead
    {
        public string TextureName;
        public Guid TextureGUID;

        public TextureHead(BinaryReader reader)
        {
            TextureName = ReadString(reader);
            TextureGUID = new Guid(reader.ReadBytes(16));
        }

        public static string ReadString(BinaryReader reader)
        {
            uint length = reader.ReadUInt32();
            byte[] stringBytes = reader.ReadBytes((int)length);
            return Encoding.UTF8.GetString(stringBytes);
        }
    }

    public class TextureMipMap
    {
        public TextureMipMaps Parent;
        public uint Index;
        public uint Offset;
        public uint Size;

        public TextureMipMap(TextureMipMaps parent, BinaryReader reader)
        {
            Parent = parent;
            Index = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }
    }

    public class TextureMipMaps
    {
        public string TextureFileName;
        public List<TextureMipMap> Maps;

        public TextureMipMaps(BinaryReader reader)
        {
            TextureFileName = TextureHead.ReadString(reader);

            uint num = reader.ReadUInt32();

            Maps = [];
            for (uint i = 0; i < num; i++)
                Maps.Add(new(this, reader));
        }
    }
}
