using NzbDrone.Core.Localization;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.HealthCheck.Checks
{
    public class MetadataSourceHealthCheck : HealthCheckBase
    {
        private readonly IMetadataRequestBuilder _requestBuilder;

        public MetadataSourceHealthCheck(IMetadataRequestBuilder requestBuilder, ILocalizationService localizationService)
            : base(localizationService)
        {
            _requestBuilder = requestBuilder;
        }

        public override HealthCheck Check()
        {
            if (_requestBuilder.IsUsingSecondary)
            {
                return new HealthCheck(GetType(), HealthCheckResult.Warning, _localizationService.GetLocalizedString("MetadataSourceSecondaryInUse"), "#metadata-secondary-source");
            }

            return new HealthCheck(GetType());
        }
    }
}
