using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile
{
    public abstract class UnrealUpkBuilderBase
    {
        protected int BuilderSize { get; set; }
        protected int BuilderOffset { get; set; }
        public abstract int GetBuilderSize();
        public abstract Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset);

    }

}
