using System;
using System.Net;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MetadataSource
{
    public interface IMetadataRequestBuilder
    {
        IHttpRequestBuilderFactory GetRequestBuilder();
        T Execute<T>(Func<IHttpRequestBuilderFactory, HttpRequest> build, Func<HttpRequest, T> execute)
            where T : HttpResponse;
        bool IsUsingSecondary { get; }
        bool HasSecondary { get; }
    }

    public class MetadataRequestBuilder : IMetadataRequestBuilder
    {
        private static readonly TimeSpan Backoff = TimeSpan.FromMinutes(5);

        private readonly IConfigService _configService;

        private readonly IReadarrCloudRequestBuilder _defaultRequestFactory;

        private readonly object _lock = new object();

        private DateTime _primaryDownUntil = DateTime.MinValue;
        private DateTime _secondaryDownUntil = DateTime.MinValue;

        public MetadataRequestBuilder(IConfigService configService, IReadarrCloudRequestBuilder defaultRequestBuilder)
        {
            _configService = configService;
            _defaultRequestFactory = defaultRequestBuilder;
        }

        public bool HasSecondary => GetSecondarySource().IsNotNullOrWhiteSpace();

        public bool IsUsingSecondary
        {
            get
            {
                lock (_lock)
                {
                    return UseSecondary();
                }
            }
        }

        public IHttpRequestBuilderFactory GetRequestBuilder()
        {
            lock (_lock)
            {
                return BuildRequestBuilder(ActiveSource());
            }
        }

        public T Execute<T>(Func<IHttpRequestBuilderFactory, HttpRequest> build, Func<HttpRequest, T> execute)
            where T : HttpResponse
        {
            string source;
            T response = null;
            Exception error = null;

            lock (_lock)
            {
                source = ActiveSource();
            }

            try
            {
                response = execute(BuildRequest(build, source));
            }
            catch (HttpException e)
            {
                error = e;
            }

            if ((error != null || IsUnhealthy(response)) && HasSecondary)
            {
                lock (_lock)
                {
                    MarkDown(source);
                    source = ActiveSource();
                }

                try
                {
                    response = execute(BuildRequest(build, source));
                    error = null;
                }
                catch (HttpException e)
                {
                    error = e;
                }
            }

            lock (_lock)
            {
                if (error != null || IsUnhealthy(response))
                {
                    MarkDown(source);
                }
                else
                {
                    MarkHealthy(source);
                }
            }

            if (error != null)
            {
                throw error;
            }

            return response;
        }

        private HttpRequest BuildRequest(Func<IHttpRequestBuilderFactory, HttpRequest> build, string source)
        {
            return build(BuildRequestBuilder(source));
        }

        private static bool IsUnhealthy(HttpResponse response)
        {
            if (response == null)
            {
                return true;
            }

            return response.StatusCode == HttpStatusCode.RequestTimeout ||
                   response.StatusCode == HttpStatusCode.TooManyRequests ||
                   (int)response.StatusCode >= 500;
        }

        private bool UseSecondary()
        {
            if (!HasSecondary)
            {
                return false;
            }

            var primaryDown = DateTime.UtcNow < _primaryDownUntil;
            var secondaryDown = DateTime.UtcNow < _secondaryDownUntil;

            return primaryDown && !secondaryDown;
        }

        private string ActiveSource()
        {
            return UseSecondary() ? GetSecondarySource() : GetPrimarySource();
        }

        private void MarkDown(string source)
        {
            if (IsSecondary(source))
            {
                _secondaryDownUntil = DateTime.UtcNow + Backoff;
            }
            else
            {
                _primaryDownUntil = DateTime.UtcNow + Backoff;
            }
        }

        private void MarkHealthy(string source)
        {
            if (IsSecondary(source))
            {
                _secondaryDownUntil = DateTime.MinValue;
            }
            else
            {
                _primaryDownUntil = DateTime.MinValue;
            }
        }

        private bool IsSecondary(string source)
        {
            return HasSecondary && GetSecondarySource().Equals(source, StringComparison.OrdinalIgnoreCase);
        }

        private string GetPrimarySource()
        {
            return _configService.MetadataSource;
        }

        private string GetSecondarySource()
        {
            return _configService.MetadataSourceSecondary;
        }

        private IHttpRequestBuilderFactory BuildRequestBuilder(string source)
        {
            if (source.IsNotNullOrWhiteSpace())
            {
                return new HttpRequestBuilder(source.TrimEnd("/") + "/{route}").KeepAlive().CreateFactory();
            }

            return _defaultRequestFactory.Metadata;
        }
    }
}
