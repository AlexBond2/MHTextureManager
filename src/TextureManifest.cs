using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using DDSLib.Constants;
using UpkManager.Models.UpkFile.Objects.Textures;

namespace MHTextureManager
{

    public enum ModResult
    {
        Success,
        NotMatch,
        TexutureNotFound,
        Reset
    }

    public class TextureManifest
    {
        public SortedDictionary<TextureHead, TextureEntry> Entries { get; private set; } = [];

        public List<TextureEntry> LoadManifest(string filePath)
        {
            Entries.Clear();

            using (var reader = new BinaryReader(File.Open(filePath, FileMode.Open)))
            {
                uint size = reader.ReadUInt32();

                for (uint i = 0; i < size; i++)
                {
                    var entry = new TextureEntry(reader, i);
                    Entries.Add(entry.Head, entry);
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
            string headerLine = sr.ReadLine();

            var guidIndex = Entries.Keys.ToLookup(key => key.TextureGuid);

            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
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

                var keysToUpdate = guidIndex[guid].ToList();

                if (keysToUpdate.Count == 0)
                {
                    Log($"Not found: {guid}");
                    return;
                }

                foreach (var key in keysToUpdate)
                    if (Entries.TryGetValue(key, out var entry))
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

        public void SaveManifest(string filePath)
        {
            using var writer = new BinaryWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));
            uint size = (uint)Entries.Count;
            writer.Write(size);

            foreach (var entry in Entries.Values)
                entry.Write(writer);
        }

        public ModResult ApplyMod(TextureMipMapsInfo mod, string manifestPath)
        {
            string tfcPath = Path.Combine(manifestPath, mod.Updated.TextureFileName + ".tfc");
            if (!File.Exists(tfcPath)) return ModResult.TexutureNotFound;

            var guidIndex = Entries.Keys.ToLookup(key => key.TextureName);
            var keysToUpdate = guidIndex[mod.Head.TextureName].ToList();

            foreach (var key in keysToUpdate)
            {
                if (key.TextureGuid != mod.Head.TextureGuid) continue;

                if (Entries[key].Data.Maps.Count != mod.Updated.Maps.Count)
                    return ModResult.NotMatch;

                Entries[key].Data.TextureFileName = mod.Updated.TextureFileName;
                Entries[key].Data.SetData(mod.Updated);
            }

            return ModResult.Success;
        }

        public ModResult ResetMod(TextureMipMapsInfo mod, string manifestPath)
        {
            string tfcPath = Path.Combine(manifestPath, mod.Original.TextureFileName + ".tfc");
            if (!File.Exists(tfcPath)) return ModResult.TexutureNotFound;

            var guidIndex = Entries.Keys.ToLookup(key => key.TextureName);
            var keysToUpdate = guidIndex[mod.Head.TextureName].ToList();

            foreach (var key in keysToUpdate)
            {
                if (key.TextureGuid != mod.Head.TextureGuid) continue;

                if (Entries[key].Data.Maps.Count != mod.Original.Maps.Count)
                    return ModResult.NotMatch;

                Entries[key].Data.TextureFileName = mod.Original.TextureFileName;
                Entries[key].Data.SetData(mod.Original);
            }

            return ModResult.Success;
        }

        public ModResult GetStatus(TextureMipMapsInfo mod, string manifestPath)
        {
            string tfcPath = Path.Combine(manifestPath, mod.Updated.TextureFileName + ".tfc");
            if (!File.Exists(tfcPath)) return ModResult.TexutureNotFound;

            var guidIndex = Entries.Keys.ToLookup(key => key.TextureName);
            var keysToUpdate = guidIndex[mod.Head.TextureName].ToList();

            foreach (var key in keysToUpdate)
            {
                if (key.TextureGuid != mod.Head.TextureGuid) continue;

                if (Entries[key].Data.Maps.Count != mod.Updated.Maps.Count)
                    return ModResult.NotMatch;

                if (Entries[key].Data.TextureFileName == mod.Updated.TextureFileName)
                {
                    if (Entries[key].Data.Maps[^1].Offset != mod.Updated.Maps[^1].Offset)
                        return ModResult.NotMatch;

                    return ModResult.Success;
                }
            }

            return ModResult.Reset;
        }

        public TextureEntry GetTextureEntry(TextureMipMapsInfo mod)
        {
            var guidIndex = Entries.Keys.ToLookup(key => key.TextureName);
            var keys = guidIndex[mod.Head.TextureName].ToList();
            foreach (var key in keys)
                if (key.TextureGuid == mod.Head.TextureGuid) return Entries[key];
            return null;
        }
    }

    public class TextureMipMapsInfo
    {
        public TextureHead Head { get; set; }
        public TextureMipMaps Original { get; set; }
        public TextureMipMaps Updated { get; set; }
        public TextureMipMapsInfo() { }
        public TextureMipMapsInfo(TextureEntry entry)
        {
            Head = entry.Head;
            Original = new(entry.Data);
        }

        public void Update(TextureMipMaps data)
        {
            Updated = new(data);
        }

        public void SaveBackup(string modsPath)
        {
            var modName = $"backup_{Head.TextureName}_{DateTime.Now:yyyyMMdd_HHmmss}.json";
            string modPath = Path.Combine(modsPath, modName);

            var modArray = new[] { this };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true 
            };

            using var fileStream = new FileStream(modPath, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(fileStream, modArray, options);
        }

        public static bool SaveMods(List<TextureMipMapsInfo> mods, string fileName)
        {
            var seen = new HashSet<string>();
            foreach (var mod in mods)
                if (!seen.Add(mod.Head.TextureName))
                    return false;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            using var fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            JsonSerializer.Serialize(fileStream, mods, options);

            return true;
        }
    }

    public class TextureEntry
    {
        public TextureHead Head;
        public TextureMipMaps Data;

        public TextureEntry(BinaryReader reader, uint index)
        {
            Head = new(reader, index);
            Data = new(this, reader);
        }

        public void Write(BinaryWriter writer)
        {
            Head.Write(writer);
            Data.Write(writer);
        }
    }

    public struct TextureHead : IComparable<TextureHead> 
    {
        public string TextureName { get; set; }
        public Guid TextureGuid { get; set; }

        [JsonIgnore]
        public uint HashIndex; // use HashIndex without the hash function

        public TextureHead(BinaryReader reader, uint index)
        {
            HashIndex = index;
            TextureName = ReadString(reader);
            TextureGuid = new Guid(reader.ReadBytes(16));
        }

        public override int GetHashCode()
        {
            // TODO Hash function https://github.com/stephank/surreal/blob/master/Core/Inc/UnFile.h#L331C14
            return TextureName?.GetHashCode() ?? 0;
        }

        public int CompareTo(TextureHead other)
        {
            return HashIndex.CompareTo(other.HashIndex);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            TextureHead other = (TextureHead)obj;
            return HashIndex == other.HashIndex;
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

        public void Write(BinaryWriter writer)
        {
            WriteString(writer, TextureName);
            writer.Write(TextureGuid.ToByteArray());
        }

        public static void WriteString(BinaryWriter writer, string value)
        {
            byte[] stringBytes = Encoding.UTF8.GetBytes(value + '\0');
            writer.Write((uint)stringBytes.Length);
            writer.Write(stringBytes);
        }
    }

    public class TextureMipMap
    {
        [JsonIgnore]
        public uint Index;
        public uint Offset { get; set; }
        public uint Size { get; set; }        
        
        [JsonIgnore]
        public TextureEntry Entry;

        public TextureMipMap() { }

        public TextureMipMap(TextureMipMap map)
        {
            Index = map.Index;
            Offset = map.Offset;
            Size = map.Size;
        }

        public TextureMipMap(TextureEntry entry, BinaryReader reader)
        {
            Entry = entry;
            Index = reader.ReadUInt32();
            Offset = reader.ReadUInt32();
            Size = reader.ReadUInt32();
        }

        public override string ToString()
        {
            return Index.ToString();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Index);
            writer.Write(Offset);
            writer.Write(Size);
        }
    }

    public class TextureMipMaps
    {
        public string TextureFileName { get; set; }
        public List<TextureMipMap> Maps { get; set; }

        [JsonIgnore]
        public UnrealMipMap OverrideMipMap;

        public TextureMipMaps() { }

        public TextureMipMaps(TextureMipMaps data)
        {
            TextureFileName = data.TextureFileName;

            Maps = [];
            foreach (var map in data.Maps)
                Maps.Add(new(map));
        }

        public TextureMipMaps(TextureEntry entry, BinaryReader reader)
        {
            OverrideMipMap = new();
            TextureFileName = TextureHead.ReadString(reader);

            uint num = reader.ReadUInt32();

            Maps = [];
            for (uint i = 0; i < num; i++)
                Maps.Add(new(entry, reader));
        }

        public void Write(BinaryWriter writer)
        {
            TextureHead.WriteString(writer, TextureFileName);

            uint num = (uint)Maps.Count;
            writer.Write(num);

            foreach (var map in Maps)
                map.Write(writer);
        }

        public void SetData(TextureMipMaps updated)
        {
            int num = Maps.Count;
            for (int i = 0; i < num; i++)
            {
                Maps[i].Offset = updated.Maps[i].Offset;
                Maps[i].Size = updated.Maps[i].Size;
            }
        }
    }
}
