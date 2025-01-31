
namespace DDSLib
{

    public sealed class DdsMipMap
    {

        internal DdsMipMap(int width, int height, byte[] mipMap = null)
        {
            Width = width;
            Height = height;

            MipMap = mipMap;
        }

        public int Width { get; set; }

        public int Height { get; set; }

        public byte[] MipMap { get; set; }

    }

}
