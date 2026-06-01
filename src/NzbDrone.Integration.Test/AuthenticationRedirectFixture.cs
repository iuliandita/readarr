using System.Net;
using System.Net.Http;
using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Integration.Test
{
    [TestFixture]
    public class AuthenticationRedirectFixture : IntegrationTest
    {
        protected override bool EnableAuth => true;

        public override string AuthorRootFolder => GetTempDirectory("AuthorRootFolder");

        // The base initialisation performs authenticated API writes (e.g. HostConfig.Put) that fail
        // validation when Forms auth is enabled without configured credentials. These tests only need
        // the server running to observe the unauthenticated challenge response, so skip that setup.
        protected override void InitializeTestTarget()
        {
        }

        private static HttpClient BuildClient()
        {
            // Don't follow redirects: we want to observe the challenge response itself.
            return new HttpClient(new HttpClientHandler { AllowAutoRedirect = false });
        }

        [Test]
        public void unauthenticated_ui_request_should_redirect_to_login()
        {
            using var client = BuildClient();
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl);

            var response = client.Send(request);

            response.StatusCode.Should().Be(HttpStatusCode.Found);
            response.Headers.Location.Should().NotBeNull();
            response.Headers.Location.ToString().Should().Contain("/login");
        }

        [Test]
        public void unauthenticated_api_request_should_return_401()
        {
            using var client = BuildClient();
            var request = new HttpRequestMessage(HttpMethod.Get, RootUrl + "api/v1/system/status");

            var response = client.Send(request);

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
