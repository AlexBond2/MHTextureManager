using System.Collections.Generic;
using System.IO;


namespace UpkManager.Models
{

    public class UnrealExportedObject
    {

        #region Properties

        public string Filename { get; set; }
        public UnrealExportedObject Parent { get; set; }
        public List<UnrealExportedObject> Children { get; set; }

        #endregion Properties

        #region Unreal Properties

        public string Name => Path.GetFileName(Filename);

        #endregion Unreal Properties

    }

}
