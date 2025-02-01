using System.Threading.Tasks;

namespace UpkManager.Compression
{

    public interface ILzoCompression
    {

        string Version { get; }
        string VersionDate { get; }

        Task<byte[]> Compress(byte[] source);

        Task Decompress(byte[] Source, byte[] Destination);

    }

}
