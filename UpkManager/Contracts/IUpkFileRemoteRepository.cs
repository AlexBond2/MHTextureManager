using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using UpkManager.Models;


namespace UpkManager.Contracts
{

    public interface IUpkFileRemoteRepository
    {

        Task<List<UnrealUpkFile>> LoadUpkFiles(CancellationToken token);

        Task SaveUpkFile(UnrealUpkFile File);

        Task SaveUpkFile(List<UnrealUpkFile> Files);

        void Shutdown();

    }

}
