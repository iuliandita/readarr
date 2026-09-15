using System;
using System.Collections.Generic;
using System.IO;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.MediaCover
{
    public interface IMediaCoverProxy
    {
        string RegisterUrl(string url);

        string GetUrl(string hash);
        byte[] GetImage(string hash);
    }

    public class MediaCoverProxy : IMediaCoverProxy
    {
        private static readonly string[] AllowedHostSuffixes =
        {
            "amazon.com",
            "media-amazon.com",
            "ssl-images-amazon.com",
            "gr-assets.com",
            "goodreads.com",
            "hardcover.app"
        };

        private readonly IHttpClient _httpClient;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly ICached<string> _cache;
        private readonly Logger _logger;

        public MediaCoverProxy(IHttpClient httpClient,
                               IConfigFileProvider configFileProvider,
                               IConfigService configService,
                               ICacheManager cacheManager,
                               Logger logger)
        {
            _httpClient = httpClient;
            _configFileProvider = configFileProvider;
            _configService = configService;
            _cache = cacheManager.GetCache<string>(GetType());
            _logger = logger;
        }

        public string RegisterUrl(string url)
        {
            if (url.IsNullOrWhiteSpace())
            {
                return null;
            }

            var hash = url.SHA256Hash();

            _cache.Set(hash, url, TimeSpan.FromHours(24));

            _cache.ClearExpired();

            var fileName = Path.GetFileName(url);
            return _configFileProvider.UrlBase + @"/MediaCoverProxy/" + hash + "/" + fileName;
        }

        public string GetUrl(string hash)
        {
            var result = _cache.Find(hash);

            if (result == null)
            {
                throw new KeyNotFoundException("Url no longer in cache");
            }

            return result;
        }

        public byte[] GetImage(string hash)
        {
            var url = GetUrl(hash);

            if (!IsAllowedUrl(url))
            {
                _logger.Warn("Refusing to proxy media cover from disallowed url: {0}", url);

                return null;
            }

            var request = new HttpRequest(url);

            return _httpClient.Get(request).ResponseData;
        }

        internal bool IsAllowedUrl(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                return false;
            }

            if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            if (uri.Port != 80 && uri.Port != 443)
            {
                return false;
            }

            return IsAllowedHost(uri.Host);
        }

        private bool IsAllowedHost(string host)
        {
            var metadataSourceHost = GetMetadataSourceHost();

            if (metadataSourceHost != null && IsHostOrSubdomain(host, metadataSourceHost))
            {
                return true;
            }

            return Array.Exists(AllowedHostSuffixes, suffix => IsHostOrSubdomain(host, suffix));
        }

        private string GetMetadataSourceHost()
        {
            if (!Uri.TryCreate(_configService.MetadataSource, UriKind.Absolute, out var uri))
            {
                return null;
            }

            return uri.Host;
        }

        private static bool IsHostOrSubdomain(string host, string domain)
        {
            return host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                   host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);
        }
    }
}
