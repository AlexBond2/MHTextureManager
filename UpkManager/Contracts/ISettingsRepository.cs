using System.Threading.Tasks;

using UpkManager.Models;


namespace UpkManager.Contracts
{

    public interface ISettingsRepository
    {

        Task<UnrealSettings> LoadSettingsAsync();

        Task SaveSettings(UnrealSettings Settings);

    }

}
