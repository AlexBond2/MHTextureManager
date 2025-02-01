using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Objects.Sounds
{

    public class UnrealObjectSoundNodeWave : UnrealObjectCompressionBase
    {

        #region Constructor

        public UnrealObjectSoundNodeWave()
        {
            Sounds = new List<byte[]>();
        }

        #endregion Constructor

        #region Properties

        public List<byte[]> Sounds { get; }

        public override bool IsExportable => true;

        public override ViewableTypes Viewable => ViewableTypes.Sound;

        public override ObjectTypes ObjectType => ObjectTypes.SoundNodeWave;

        public override string FileExtension => ".ogg";

        public override string FileTypeDesc => "Ogg Vorbis";

        #endregion Properties

        #region Unreal Methods

        public override async Task ReadUnrealObject(ByteArrayReader reader, UnrealHeader header, UnrealExportTableEntry export, bool skipProperties, bool skipParse)
        {
            await base.ReadUnrealObject(reader, header, export, skipProperties, skipParse).ConfigureAwait(false);

            if (skipParse) return;

            bool done = false;

            do
            {
                await ProcessCompressedBulkData(reader, async bulkChunk =>
                {
                    byte[] ogg = (await bulkChunk.DecompressChunk(0).ConfigureAwait(false))?.GetBytes();

                    if (ogg == null || ogg.Length == 0)
                    {
                        done = true;

                        return;
                    }

                    Sounds.Add(ogg);
                }).ConfigureAwait(false);
            } while (!done);
        }

        public override async Task SaveObject(string filename, object configuration)
        {
            if (!Sounds.Any()) return;

            await Task.Run(() => File.WriteAllBytes(filename, Sounds[0])).ConfigureAwait(false);

            if (Sounds.Count == 1) return;

            string name = Path.GetFileNameWithoutExtension(filename);
            string ext = Path.GetExtension(filename);

            for (int i = 1; i < Sounds.Count; ++i)
            {
                string soundFilename = Path.Combine(Path.GetDirectoryName(filename), $"{name}_{i}{ext}");

                int i1 = i;

                await Task.Run(() => File.WriteAllBytes(soundFilename, Sounds[i1])).ConfigureAwait(false);
            }
        }

        public override Stream GetObjectStream()
        {
            return Sounds.Any() ? new MemoryStream(Sounds[0]) : null;
        }

        #endregion Unreal Methods

    }

}
