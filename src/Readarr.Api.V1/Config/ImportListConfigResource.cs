using NzbDrone.Core.Configuration;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Config
{
    public class ImportListConfigResource : RestResource
    {
        public int ImportListSyncInterval { get; set; }
    }

    public static class ImportListConfigResourceMapper
    {
        public static ImportListConfigResource ToResource(IConfigService model)
        {
            return new ImportListConfigResource
            {
                ImportListSyncInterval = model.ImportListSyncInterval
            };
        }
    }
}
