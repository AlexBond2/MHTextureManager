using System.Text;
using DDSLib.Constants;
using UpkManager.Models.UpkFile.Objects.Textures;

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
            
            // TODO replace to TextureInfo.bin
            string tfcPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "TextureInfo.tsv");
            if (File.Exists(tfcPath))
                ProcessTfcFile(tfcPath);

            return [.. Entries.Values.OrderBy(entry => entry.Head.TextureName)];
        }

        private void ProcessTfcFile(string tfcFilePath)
        {
            using var sr = new StreamReader(tfcFilePath);
            string? headerLine = sr.ReadLine();

            while (!sr.EndOfStream)
            {
                string? line = sr.ReadLine();
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                string[] parts = line.Split('\t');
                if (parts.Length < 5) continue;

                string guidStr = parts[0].Trim();
                string indexStr = parts[1].Trim();
                string widthStr = parts[2].Trim();
                string heightStr = parts[3].Trim();
                string formatStr = parts[4].Trim();

                Guid guid = new(guidStr);

                if (!int.TryParse(indexStr, out int index)) continue;
                if (!int.TryParse(widthStr, out int width)) continue;
                if (!int.TryParse(heightStr, out int height)) continue;
                if (!FileFormat.TryParse(formatStr, out FileFormat overrideFormat)) continue;

                if (!Entries.TryGetValue(guid, out TextureEntry entry))
                {
                    Log($"Not found: {guid}");
                }
                else
                {
                    entry.Data.OverrideMipMap.Width = width;
                    entry.Data.OverrideMipMap.Height = height;
                    entry.Data.OverrideMipMap.OverrideFormat = overrideFormat;
                    entry.Data.OverrideMipMap.ImageData = [(byte)index];
                }
            }
        }

        private void Log(string message)
        {
            string filePath = "log.txt";

            try
            {
                using StreamWriter writer = new StreamWriter(filePath, true);
                writer.WriteLine($"{DateTime.Now}: {message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error to write {filePath}: {ex.Message}");
            }
        }

    }

    public class TextureEntry
    {
        public TextureHead Head;
        public TextureMipMaps Data;

        public TextureEntry(BinaryReader reader)
        {
            Head = new(reader);
            Data = new(reader);
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
            int nullIndex = Array.IndexOf(stringBytes, (byte)0);
            if (nullIndex >= 0)
                stringBytes = stringBytes[..nullIndex];
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
        public UnrealMipMap OverrideMipMap;
        public List<TextureMipMap> Maps;

        public TextureMipMaps(BinaryReader reader)
        {
            OverrideMipMap = new();
            TextureFileName = TextureHead.ReadString(reader);

            uint num = reader.ReadUInt32();

            Maps = [];
            for (uint i = 0; i < num; i++)
                Maps.Add(new(this, reader));
        }
    }
}
