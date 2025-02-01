using System.Threading.Tasks;

using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Tables
{

    public abstract class UnrealExportTableEntryBuilderBase : UnrealObjectTableEntryBase
    {

        protected int BuilderSerialDataSize { get; set; }

        protected int BuilderSerialDataOffset { get; set; }

        public abstract int GetObjectSize(int CurrentOffset);

        public abstract Task<ByteArrayWriter> WriteObjectBuffer();

    }

}
