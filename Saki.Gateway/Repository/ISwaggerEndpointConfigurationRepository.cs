using MMLib.SwaggerForOcelot.Configuration;

namespace Saki.Gateway.Repository
{
    public interface ISwaggerEndpointConfigurationRepository
    {
        ManageSwaggerEndpointData GetSwaggerEndpoint(SwaggerEndPointOptions endPoint, string version);
    }
}
