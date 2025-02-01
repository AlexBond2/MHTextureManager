
using DDSLib.Constants;

namespace UpkManager.Models.UpkFile.Objects.Textures
{

    public class UnrealMipMap
    {

        #region Properties

        public int Width { get; set; }
        public int Height { get; set; }
        public FileFormat OverrideFormat { get; set; }
        public byte[] ImageData { get; set; }

        #endregion Properties

    }

}
