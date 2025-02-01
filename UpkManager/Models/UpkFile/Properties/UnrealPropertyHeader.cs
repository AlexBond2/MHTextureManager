using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyHeader : UnrealUpkBuilderBase
    {

        #region Constructor

        internal UnrealPropertyHeader()
        {
            Properties = new List<UnrealProperty>();
        }

        #endregion Constructor

        #region Properties

        public int TypeIndex { get; private set; }

        public List<UnrealProperty> Properties { get; }

        #endregion Properties

        #region Unreal Methods

        internal async Task ReadPropertyHeader(ByteArrayReader reader, UnrealHeader header)
        {
            TypeIndex = reader.ReadInt32();

            do
            {
                UnrealProperty property = new UnrealProperty();

                await property.ReadProperty(reader, header).ConfigureAwait(false);

                Properties.Add(property);

                if (property.NameIndex.Name == ObjectTypes.None.ToString()) break;
            }
            while (true);
        }

        internal List<UnrealProperty> GetProperty(string name)
        {
            return Properties.Where(p => p.NameIndex.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = sizeof(int)
                        + Properties.Sum(p => p.GetBuilderSize());

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            Writer.WriteInt32(TypeIndex);

            foreach (UnrealProperty property in Properties) await property.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
