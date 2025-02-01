using System.Collections.Generic;
using System.Threading.Tasks;

using UpkManager.Models;
using UpkManager.Models.UpkFile;


namespace UpkManager.Contracts
{

    public interface IUpkFileRepository
    {

        Task LoadDirectoryRecursiveFlat(List<UnrealUpkFile> ParentFile, string ParentPath, string Path, string Filter, bool isRoot = true);

        Task LoadDirectoryRecursive(UnrealExportedObject Parent, string Path);

        Task<UnrealHeader> LoadUpkFile(string Filename);

        Task SaveUpkFile(UnrealHeader Header, string Filename);

        Task<UnrealVersion> GetGameVersion(string GamePath);

        Task<string> GetGameLocale(string GamePath);

    }

}
