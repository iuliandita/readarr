using System.Collections.Generic;
using System.Net;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MetadataSource
{
    [TestFixture]
    public class MetadataRequestBuilderFixture : CoreTest<MetadataRequestBuilder>
    {
        private const string Primary = "https://primary.example";
        private const string Secondary = "https://secondary.example";

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.MetadataSource)
                .Returns("");

            Mocker.GetMock<IReadarrCloudRequestBuilder>()
                .Setup(s => s.Metadata)
                .Returns(new HttpRequestBuilder("https://api.bookinfo.club/v1/{route}").CreateFactory());
        }

        private void WithCustomProvider()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(s => s.MetadataSource)
                .Returns("http://api.readarr.com/api/testing/");
        }

        private void WithFailover()
        {
            Mocker.GetMock<IConfigService>().Setup(s => s.MetadataSource).Returns(Primary);
            Mocker.GetMock<IConfigService>().Setup(s => s.MetadataSourceSecondary).Returns(Secondary);
        }

        [TestCase]
        public void should_use_user_definied_if_not_blank()
        {
            WithCustomProvider();

            var details = Subject.GetRequestBuilder().Create();

            details.BaseUrl.ToString().Should().Contain("testing");
        }

        [TestCase]
        public void should_use_default_if_config_blank()
        {
            var details = Subject.GetRequestBuilder().Create();

            details.BaseUrl.ToString().Should().Contain("bookinfo.club/v1");
        }

        [Test]
        public void should_use_primary_when_healthy()
        {
            WithFailover();

            var response = Subject.Execute(Build, r => Response(r, HttpStatusCode.OK));

            response.Request.Url.Host.Should().Be("primary.example");
            Subject.IsUsingSecondary.Should().BeFalse();
        }

        [Test]
        public void should_failover_to_secondary_on_server_error()
        {
            WithFailover();

            var hosts = new List<string>();

            var response = Subject.Execute(Build, r =>
            {
                hosts.Add(r.Url.Host);

                return r.Url.Host == "primary.example"
                    ? Response(r, HttpStatusCode.InternalServerError)
                    : Response(r, HttpStatusCode.OK);
            });

            hosts.Should().Equal("primary.example", "secondary.example");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            Subject.IsUsingSecondary.Should().BeTrue();
        }

        [Test]
        public void should_failover_to_secondary_on_exception()
        {
            WithFailover();

            var hosts = new List<string>();

            var response = Subject.Execute(Build, r =>
            {
                hosts.Add(r.Url.Host);

                if (r.Url.Host == "primary.example")
                {
                    throw new HttpException(r, Response(r, HttpStatusCode.InternalServerError));
                }

                return Response(r, HttpStatusCode.OK);
            });

            hosts.Should().Equal("primary.example", "secondary.example");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void should_not_failover_when_no_secondary_is_configured()
        {
            Mocker.GetMock<IConfigService>().Setup(s => s.MetadataSource).Returns(Primary);

            var calls = 0;

            var response = Subject.Execute(Build, r =>
            {
                calls++;
                return Response(r, HttpStatusCode.InternalServerError);
            });

            calls.Should().Be(1);
            response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        }

        [Test]
        public void should_use_secondary_for_the_next_request_after_a_failure()
        {
            WithFailover();

            Subject.Execute(Build, r => r.Url.Host == "primary.example"
                ? Response(r, HttpStatusCode.InternalServerError)
                : Response(r, HttpStatusCode.OK));

            var hosts = new List<string>();

            Subject.Execute(Build, r =>
            {
                hosts.Add(r.Url.Host);
                return Response(r, HttpStatusCode.OK);
            });

            hosts.Should().Equal("secondary.example");
        }

        private static HttpRequest Build(IHttpRequestBuilderFactory factory)
        {
            return factory.Create().SetSegment("route", "author/1").Build();
        }

        private static HttpResponse Response(HttpRequest request, HttpStatusCode statusCode)
        {
            return new HttpResponse(request, new HttpHeader(), new byte[0], statusCode);
        }
    }
}
