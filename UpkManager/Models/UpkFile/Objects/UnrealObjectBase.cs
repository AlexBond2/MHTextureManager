using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using UpkManager.Constants;
using UpkManager.Helpers;
using UpkManager.Models.UpkFile.Properties;
using UpkManager.Models.UpkFile.Tables;


namespace UpkManager.Models.UpkFile.Objects
{

    public class UnrealObjectBase : UnrealUpkBuilderBase
    {

        #region Constructor

        public UnrealObjectBase()
        {
            PropertyHeader = new UnrealPropertyHeader();
        }

        #endregion Constructor

        #region Properties

        public UnrealPropertyHeader PropertyHeader { get; }

        public ByteArrayReader AdditionalDataReader { get; private set; }

        #endregion Properties

        #region Unreal Properties

        public int AdditionalDataOffset { get; private set; }

        public virtual bool IsExportable => false;

        public virtual ViewableTypes Viewable => ViewableTypes.Unknown;

        public virtual ObjectTypes ObjectType => ObjectTypes.Unknown;

        public virtual string FileExtension => String.Empty;

        public virtual string FileTypeDesc => String.Empty;

        #endregion Unreal Properties

        #region Unreal Methods

        public virtual async Task ReadUnrealObject(ByteArrayReader reader, UnrealHeader header, UnrealExportTableEntry export, bool skipProperties, bool skipParse)
        {
            if (!skipProperties) await PropertyHeader.ReadPropertyHeader(reader, header).ConfigureAwait(false);

            AdditionalDataOffset = export.SerialDataOffset + reader.CurrentOffset;

            AdditionalDataReader = await reader.Splice().ConfigureAwait(false);
        }

        public virtual async Task SaveObject(string filename, object configuration)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public virtual async Task SetObject(string filename, List<UnrealNameTableEntry> nameTable, object configuration)
        {
            await Task.CompletedTask.ConfigureAwait(false);
        }

        public virtual Stream GetObjectStream()
        {
            return null;
        }

        #endregion Unreal Methods

        #region UnrealUpkBuilderBase Implementation

        public override int GetBuilderSize()
        {
            BuilderSize = PropertyHeader.GetBuilderSize()
                        + AdditionalDataReader?.GetBytes().Length ?? 0;

            return BuilderSize;
        }

        public override async Task WriteBuffer(ByteArrayWriter Writer, int CurrentOffset)
        {
            await PropertyHeader.WriteBuffer(Writer, CurrentOffset).ConfigureAwait(false);

            await Writer.WriteBytes(AdditionalDataReader?.GetBytes()).ConfigureAwait(false);
        }

        #endregion UnrealUpkBuilderBase Implementation

    }

}
