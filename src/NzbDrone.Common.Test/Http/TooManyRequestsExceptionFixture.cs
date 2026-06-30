using System;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test.Http
{
    [TestFixture]
    public class TooManyRequestsExceptionFixture : TestBase
    {
        private TooManyRequestsException Build(string retryAfter)
        {
            var request = new HttpRequest("https://discord.com/api/webhooks/1/abc");
            var response = new HttpResponse(request, new HttpHeader(), new byte[0], (HttpStatusCode)429);

            if (retryAfter != null)
            {
                response.Headers["Retry-After"] = retryAfter;
            }

            return new TooManyRequestsException(request, response);
        }

        [Test]
        public void should_parse_integer_retry_after_seconds()
        {
            Build("300").RetryAfter.Should().Be(TimeSpan.FromSeconds(300));
        }

        [Test]
        public void should_parse_fractional_retry_after_seconds()
        {
            // Discord can return sub-second values like "0.5"; int parsing dropped these to zero.
            Build("0.5").RetryAfter.Should().Be(TimeSpan.FromMilliseconds(500));
        }

        [Test]
        public void should_default_to_zero_when_header_missing()
        {
            Build(null).RetryAfter.Should().Be(TimeSpan.Zero);
        }
    }
}
