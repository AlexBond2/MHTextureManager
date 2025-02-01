

namespace UpkManager.Models.UpkFile
{

    public abstract class UnrealHeaderBuilderBase : UnrealUpkBuilderBase
    {

        protected int BuilderNameTableOffset { get; set; }
        protected int BuilderExportTableOffset { get; set; }
        protected int BuilderImportTableOffset { get; set; }
        protected int BuilderDependsTableOffset { get; set; }

    }

}
