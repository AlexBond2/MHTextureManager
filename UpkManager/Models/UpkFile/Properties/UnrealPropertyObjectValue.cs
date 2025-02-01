using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Properties
{

    public class UnrealPropertyObjectValue : UnrealPropertyIntValue
    {

        #region Properties

        public UnrealNameTableIndex ObjectIndexName { get; private set; }

        public override PropertyTypes PropertyType => PropertyTypes.ObjectProperty;

        public override string PropertyString => ObjectIndexName.Name;

        #endregion Properties

        #region Unreal Methods

        public override async Task ReadPropertyValue(ByteArrayReader reader, int size, UnrealHeader header)
        {
            await base.ReadPropertyValue(reader, size, header).ConfigureAwait(false);

            ObjectIndexName = header.GetObjectTableEntry(IntValue)?.NameTableIndex;
        }

        #endregion Unreal Methods

    }

}
