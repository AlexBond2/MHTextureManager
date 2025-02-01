using System;
using System.Collections.Generic;
using System.Linq;

using UpkManager.Models.UpkFile;


namespace UpkManager.Models
{

    public sealed class UnrealUpkFile
    {

        #region Private Fields

        private long filesize;

        private string filehash;

        #endregion Private Fields

        #region Constructor

        public UnrealUpkFile()
        {
            Exports = new List<UnrealExportVersion>();

            ModdedFiles = new List<UnrealUpkFile>();
        }

        #endregion Constructor

        #region Properties

        public string Id { get; set; }

        public string ContentsRoot { get; set; }
        public string Package { get; set; }
        public List<UnrealExportVersion> Exports { get; set; } // Version => Type => Names
        public string Notes { get; set; }

        #endregion Properties

        #region Unreal Properties

        public UnrealHeader Header { get; set; }
        public UnrealVersion CurrentVersion { get; set; }
        public string CurrentLocale { get; set; }

        public long Filesize
        {
            get => GetCurrentExports()?.Filesize ?? filesize;
            set => filesize = value;
        }

        public string Filehash
        {
            get => GetCurrentExports()?.Filehash ?? filehash;
            set => filehash = value;
        }

        public string GameFilename { get; set; }
        public string Filename => $"{Package}.upk";
        public List<UnrealUpkFile> ModdedFiles { get; set; }
        public bool IsModded => ModdedFiles.Any();
        public DateTime? LastAccess { get; set; }
        public string NewFilehash { get; set; }
        public string NewLocale { get; set; }

        #endregion Unreal Properties

        #region Unreal Methods

        public UnrealExportVersion GetCurrentExports()
        {
            return GetExports(CurrentVersion, CurrentLocale);
        }

        public UnrealExportVersion GetExports(UnrealVersion version, string locale)
        {
            //
            // Check for exact version match
            //
            UnrealExportVersion found = Exports.SingleOrDefault(v => v.Versions.Contains(version) && v.Locale == locale);

            if (found != null) return found;
            //
            // Otherwise just return for the max version
            //
            UnrealVersion max = Exports.Where(v => v.Locale == locale).SelectMany(v => v.Versions).Max();

            if (max != null)
            {
                found = Exports.Single(v => v.Versions.Contains(max) && v.Locale == locale);

                return found;
            }
            //
            // Otherwise there is nothing to return
            //
            return null;
        }

        public UnrealVersion GetLeastVersion()
        {
            UnrealExportVersion version = GetCurrentExports();

            return version.Versions.Max();
        }

        #endregion Unreal Methods

    }

    public sealed class UnrealExportVersion
    {
        public UnrealExportVersion()
        {
            Types = new List<UnrealExportType>();
        }
        public List<UnrealVersion> Versions { get; set; }
        public string Locale { get; set; }
        public long Filesize { get; set; }
        public string Filehash { get; set; }
        public List<UnrealExportType> Types { get; set; }

    }

    public sealed class UnrealExportType
    {
        public UnrealExportType()
        {
            ExportNames = new List<string>();
        }
        public string Name { get; set; }
        public List<string> ExportNames { get; set; }
    }

}
