

namespace UpkManager.Models.UpkFile.Tables
{

    public abstract class UnrealObjectTableEntryBase : UnrealUpkBuilderBase
    {

        #region Properties

        public int OwnerReference { get; protected set; }

        public UnrealNameTableIndex NameTableIndex { get; protected set; }

        #endregion Properties

        #region Unreal Properties

        public int TableIndex { get; set; }

        #endregion Unreal Properties

    }

}
