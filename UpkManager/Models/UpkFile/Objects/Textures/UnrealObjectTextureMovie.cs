using System.IO;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Objects.Textures
{

    public sealed class UnrealObjectTextureMovie : UnrealObjectCompressionBase
    {

        #region Properties

        public byte[] Movie { get; private set; }

        public override bool IsExportable => true;

        public override ObjectTypes ObjectType => ObjectTypes.TextureMovie;

        public override string FileExtension => ".bik";

        public override string FileTypeDesc => "Bink Video";

        #endregion Properties

        #region Unreal Methods

        public override async Task ReadUnrealObject(ByteArrayReader reader, UnrealHeader header, UnrealExportTableEntry export, bool skipProperties, bool skipParse)
        {
            await base.ReadUnrealObject(reader, header, export, skipProperties, skipParse).ConfigureAwait(false);

            if (skipParse) return;

            await ProcessCompressedBulkData(reader, async bulkChunk =>
            {
                byte[] bik = (await bulkChunk.DecompressChunk(0).ConfigureAwait(false))?.GetBytes();

                if (bik == null || bik.Length == 0) return;

                Movie = bik;
            }).ConfigureAwait(false);
        }

        public override async Task SaveObject(string filename, object configuration)
        {
            if (Movie == null || Movie.Length == 0) return;

            await Task.Run(() => File.WriteAllBytes(filename, Movie)).ConfigureAwait(false);
        }

        #endregion Unreal Methods

    }

}
