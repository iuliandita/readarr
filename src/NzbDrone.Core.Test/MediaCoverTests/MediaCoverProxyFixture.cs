using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaCoverTests
{
    [TestFixture]
    public class MediaCoverProxyFixture : CoreTest<MediaCover.MediaCoverProxy>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .SetupGet(s => s.MetadataSource)
                .Returns("https://api.bookinfo.pro");
        }

        [TestCase("https://m.media-amazon.com/images/I/1.jpg")]
        [TestCase("https://images-na.ssl-images-amazon.com/images/I/1.jpg")]
        [TestCase("https://i.gr-assets.com/images/S/compressed.photo.goodreads.com/books/1.jpg")]
        [TestCase("https://assets.hardcover.app/cover.jpg")]
        [TestCase("https://api.bookinfo.pro/images/cover.jpg")]
        public void should_allow_expected_cover_hosts(string url)
        {
            Subject.IsAllowedUrl(url).Should().BeTrue();
        }

        [TestCase("http://localhost:8989/api/v1/system/status")]
        [TestCase("http://127.0.0.1/")]
        [TestCase("http://169.254.169.254/latest/meta-data/")]
        [TestCase("http://10.0.0.5/")]
        [TestCase("http://192.168.1.10/admin")]
        [TestCase("http://evil.example.com/cover.jpg")]
        [TestCase("http://m.media-amazon.com.evil.example.com/cover.jpg")]
        [TestCase("https://m.media-amazon.com:8443/cover.jpg")]
        [TestCase("file:///etc/passwd")]
        [TestCase("ftp://m.media-amazon.com/cover.jpg")]
        public void should_reject_other_urls(string url)
        {
            Subject.IsAllowedUrl(url).Should().BeFalse();
        }
    }
}
