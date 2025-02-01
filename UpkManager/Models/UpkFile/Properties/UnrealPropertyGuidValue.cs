using System;

using UpkManager.Constants;


namespace UpkManager.Models.UpkFile.Properties
{

    public sealed class UnrealPropertyGuidValue : UnrealPropertyValueBase
    {

        #region Unreal Properties

        public override PropertyTypes PropertyType => PropertyTypes.GuidProperty;

        public override string PropertyString => $"{new Guid(DataReader.GetBytes()):B}";

        #endregion Unreal Properties

    }

}
