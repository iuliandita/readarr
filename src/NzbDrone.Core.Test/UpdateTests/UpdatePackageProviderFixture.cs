using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Cloud;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Http;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Analytics;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        private HttpRequest _request;

        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IPlatformInfo>().SetupGet(c => c.Version).Returns(new Version("9.9.9"));
            Mocker.SetConstant<IReadarrCloudRequestBuilder>(new ReadarrCloudRequestBuilder());
        }

        [Test]
        public void should_return_null_when_no_update_is_available()
        {
            GivenLatestUpdate(new UpdatePackageAvailable { Available = false });

            Subject.GetLatestUpdate("develop", new Version(10, 0)).Should().BeNull();

            _request.Url.Path.Should().Be("/v1/update/develop");
            _request.Url.Query.Should().Contain("version=10.0");
            _request.Url.Query.Should().NotContain("active=");
        }

        [Test]
        public void should_return_available_update_and_include_active_analytics_state()
        {
            var update = new UpdatePackage
            {
                Version = new Version(1, 2, 3),
                FileName = "Readarr.develop.1.2.3.linux-core-x64.tar.gz",
                Hash = "update-hash",
                Branch = "develop"
            };

            Mocker.GetMock<IAnalyticsService>().SetupGet(c => c.IsEnabled).Returns(true);
            Mocker.GetMock<IAnalyticsService>().SetupGet(c => c.InstallIsActive).Returns(true);
            GivenLatestUpdate(new UpdatePackageAvailable { Available = true, UpdatePackage = update });

            var result = Subject.GetLatestUpdate("develop", new Version(0, 1));

            result.Should().BeEquivalentTo(update);
            _request.Url.Path.Should().Be("/v1/update/develop");
            _request.Url.Query.Should().Contain("version=0.1");
            _request.Url.Query.Should().Contain("active=true");
        }

        [Test]
        public void should_return_recent_updates_and_include_previous_version()
        {
            var updates = new List<UpdatePackage>
            {
                new UpdatePackage { Version = new Version(1, 2, 3), FileName = "Readarr.develop.1.2.3.linux-core-x64.tar.gz", Hash = "first", Branch = "develop" },
                new UpdatePackage { Version = new Version(1, 2, 2), FileName = "Readarr.develop.1.2.2.linux-core-x64.tar.gz", Hash = "second", Branch = "develop" }
            };

            GivenRecentUpdates(updates);

            var recent = Subject.GetRecentUpdates("develop", new Version(1, 2, 3), new Version(1, 2, 2));

            recent.Should().BeEquivalentTo(updates, options => options.WithStrictOrdering());
            _request.Url.Path.Should().Be("/v1/update/develop/changes");
            _request.Url.Query.Should().Contain("version=1.2.3");
            _request.Url.Query.Should().Contain("prevVersion=1.2.2");
            _request.Url.Query.Should().NotContain("active=");
        }

        private void GivenLatestUpdate(UpdatePackageAvailable update)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<UpdatePackageAvailable>(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(request => GivenResponse(request, update));
        }

        private void GivenRecentUpdates(List<UpdatePackage> updates)
        {
            Mocker.GetMock<IHttpClient>()
                  .Setup(c => c.Get<List<UpdatePackage>>(It.IsAny<HttpRequest>()))
                  .Returns<HttpRequest>(request => GivenResponse(request, updates));
        }

        private HttpResponse<T> GivenResponse<T>(HttpRequest request, T resource)
            where T : new()
        {
            _request = request;

            return new HttpResponse<T>(new HttpResponse(request, new HttpHeader(), resource.ToJson()));
        }
    }
}
