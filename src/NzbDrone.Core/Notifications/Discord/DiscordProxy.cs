using System;
using System.Net.Http;
using System.Threading;
using NLog;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Notifications.Discord.Payloads;

namespace NzbDrone.Core.Notifications.Discord
{
    public interface IDiscordProxy
    {
        void SendPayload(DiscordPayload payload, DiscordSettings settings);
    }

    public class DiscordProxy : IDiscordProxy
    {
        // Discord rate-limits webhooks (~30 req/min). Honor 429 Retry-After with a
        // few bounded retries so a burst degrades gracefully instead of log-spamming.
        private const int MaxAttempts = 3;
        private static readonly TimeSpan MinRetryWait = TimeSpan.FromMilliseconds(500);
        private static readonly TimeSpan MaxRetryWait = TimeSpan.FromSeconds(10);

        private readonly IHttpClient _httpClient;
        private readonly Logger _logger;

        public DiscordProxy(IHttpClient httpClient, Logger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public void SendPayload(DiscordPayload payload, DiscordSettings settings)
        {
            for (var attempt = 1; ; attempt++)
            {
                try
                {
                    var request = new HttpRequestBuilder(settings.WebHookUrl)
                        .Accept(HttpAccept.Json)
                        .Build();

                    request.Method = HttpMethod.Post;
                    request.Headers.ContentType = "application/json";
                    request.SetContent(payload.ToJson());

                    _httpClient.Execute(request);
                    return;
                }
                catch (TooManyRequestsException ex)
                {
                    if (attempt >= MaxAttempts)
                    {
                        _logger.Warn("Discord rate limited the webhook (429); gave up after {0} attempts", attempt);
                        throw new DiscordException("Unable to post payload", ex);
                    }

                    var wait = ClampRetryAfter(ex.RetryAfter);
                    _logger.Debug("Discord rate limited the webhook (429); waiting {0:0.###}s before retry", wait.TotalSeconds);
                    Thread.Sleep(wait);
                }
                catch (HttpException ex)
                {
                    _logger.Error(ex, "Unable to post payload {0}", payload);
                    throw new DiscordException("Unable to post payload", ex);
                }
            }
        }

        private static TimeSpan ClampRetryAfter(TimeSpan retryAfter)
        {
            if (retryAfter < MinRetryWait)
            {
                return MinRetryWait;
            }

            return retryAfter > MaxRetryWait ? MaxRetryWait : retryAfter;
        }
    }
}
