using AutoMapper;

namespace UpkManager.Contracts
{

    public interface IAutoMapperConfiguration
    {

        void RegisterMappings(IMapperConfigurationExpression config);

    }

}
