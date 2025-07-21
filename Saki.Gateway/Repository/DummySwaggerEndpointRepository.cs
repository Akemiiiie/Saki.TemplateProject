using System.Collections.Generic;
using MMLib.SwaggerForOcelot.Configuration;

namespace Saki.Gateway.Repository
{
    public class DummySwaggerEndpointRepository : ISwaggerEndpointConfigurationRepository
    {
        private readonly Dictionary<string, ManageSwaggerEndpointData> _endpointDatas =
            new Dictionary<string, ManageSwaggerEndpointData>()
        {
            { "WebAPIDemo2_v1", new ManageSwaggerEndpointData() { IsPublished = true } },
            { "gateway_gateway", new ManageSwaggerEndpointData() { IsPublished = true } },
        };

        public ManageSwaggerEndpointData GetSwaggerEndpoint(SwaggerEndPointOptions endPoint, string version)
        {
            var lookupKey = $"{endPoint.Key}_{version}";
            var endpointData = new ManageSwaggerEndpointData();
            if (_endpointDatas.ContainsKey(lookupKey))
            {
                endpointData = _endpointDatas[lookupKey];
            }

            return endpointData;
        }
    }
}
