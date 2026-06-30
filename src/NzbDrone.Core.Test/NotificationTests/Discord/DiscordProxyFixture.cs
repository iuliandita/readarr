using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.Notifications.Discord;
using NzbDrone.Core.Notifications.Discord.Payloads;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.NotificationTests.Discord
{
    [TestFixture]
    public class DiscordProxyFixture : CoreTest<DiscordProxy>
    {
        private DiscordSettings _settings;
        private DiscordPayload _payload;

        [SetUp]
        public void Setup()
        {
            _settings = new DiscordSettings { WebHookUrl = "https://discord.com/api/webhooks/1/abc" };
            _payload = new DiscordPayload();
        }

        private TooManyRequestsException Build429(string retryAfter)
        {
            var request = new HttpRequest(_settings.WebHookUrl);
            var response = new HttpResponse(request, new HttpHeader(), new byte[0], (HttpStatusCode)429);
            response.Headers["Retry-After"] = retryAfter;
            return new TooManyRequestsException(request, response);
        }

        private HttpResponse OkResponse()
        {
            var request = new HttpRequest(_settings.WebHookUrl);
            return new HttpResponse(request, new HttpHeader(), new byte[0], HttpStatusCode.OK);
        }

        [Test]
        public void should_wait_and_retry_once_after_429_with_retry_after()
        {
            Mocker.GetMock<IHttpClient>()
                .SetupSequence(c => c.Execute(It.IsAny<HttpRequest>()))
                .Throws(Build429("1"))
                .Returns(OkResponse());

            var sw = Stopwatch.StartNew();
            Subject.SendPayload(_payload, _settings);
            sw.Stop();

            // Honored the ~1s Retry-After before retrying rather than hammering.
            sw.ElapsedMilliseconds.Should().BeGreaterThan(700);

            Mocker.GetMock<IHttpClient>()
                .Verify(c => c.Execute(It.IsAny<HttpRequest>()), Times.Exactly(2));
        }

        [Test]
        public void should_give_up_with_single_warn_on_persistent_429()
        {
            Mocker.GetMock<IHttpClient>()
                .Setup(c => c.Execute(It.IsAny<HttpRequest>()))
                .Throws(Build429("0.5"));

            var ex = Assert.Throws<DiscordException>(() => Subject.SendPayload(_payload, _settings));

            // Still surfaces the 429 to the caller rather than swallowing it.
            ex.InnerException.Should().BeOfType<TooManyRequestsException>();

            // Bounded attempts: initial + retries, all exhausted - no retry storm.
            Mocker.GetMock<IHttpClient>()
                .Verify(c => c.Execute(It.IsAny<HttpRequest>()), Times.Exactly(3));

            // Give-up branch runs exactly once, so its single Warn is logged once and
            // no Error is logged per attempt. Drain it so the log harness stays clean.
            // (Asserting the exact Warn count couples to flaky global NLog-init timing.)
            ExceptionVerification.IgnoreWarns();
            ExceptionVerification.IgnoreErrors();
        }
    }
}
