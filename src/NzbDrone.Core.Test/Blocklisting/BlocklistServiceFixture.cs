using System;
using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Blocklisting;
using NzbDrone.Core.Download;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Blocklisting
{
    [TestFixture]
    public class BlocklistServiceFixture : CoreTest<BlocklistService>
    {
        private DownloadFailedEvent _event;

        [SetUp]
        public void Setup()
        {
            _event = new DownloadFailedEvent
            {
                AuthorId = 12345,
                BookIds = new List<int> { 1 },
                Quality = new QualityModel(Quality.MP3),
                SourceTitle = "author.name.book.title",
                DownloadClient = "SabnzbdClient",
                DownloadId = "Sabnzbd_nzo_2dfh73k"
            };

            _event.Data.Add("publishedDate", DateTime.UtcNow.ToString("s") + "Z");
            _event.Data.Add("size", "1000");
            _event.Data.Add("indexer", "nzbs.org");
            _event.Data.Add("protocol", "1");
            _event.Data.Add("message", "Marked as failed");
        }

        [Test]
        public void should_add_to_repository()
        {
            Subject.Handle(_event);

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.BookIds == _event.BookIds)), Times.Once());
        }

        [Test]
        public void should_add_to_repository_missing_size_and_protocol()
        {
            Subject.Handle(_event);

            _event.Data.Remove("size");
            _event.Data.Remove("protocol");

            Mocker.GetMock<IBlocklistRepository>()
                .Verify(v => v.Insert(It.Is<Blocklist>(b => b.BookIds == _event.BookIds)), Times.Once());
        }

        [Test]
        public void should_block_a_usenet_release_when_only_the_published_timestamp_changed()
        {
            var release = new ReleaseInfo
            {
                Title = "author.name.book.title",
                DownloadProtocol = DownloadProtocol.Usenet,
                Indexer = "nzbs.org",
                PublishDate = new DateTime(2022, 3, 31, 10, 18, 0),
                Size = 361219000
            };

            Mocker.GetMock<IBlocklistRepository>()
                .Setup(v => v.BlocklistedByTitle(12345, release.Title))
                .Returns(new List<Blocklist>
                {
                    new Blocklist
                    {
                        AuthorId = 12345,
                        SourceTitle = release.Title,
                        Protocol = DownloadProtocol.Usenet,
                        Indexer = null,
                        PublishedDate = new DateTime(2022, 4, 30, 10, 26, 30),
                        Size = 361219000
                    }
                });

            Subject.Blocklisted(12345, release).Should().BeTrue();
        }

        [Test]
        public void should_not_block_a_usenet_release_with_a_different_title()
        {
            var release = new ReleaseInfo
            {
                Title = "author.name.book.title",
                DownloadProtocol = DownloadProtocol.Usenet,
                Indexer = "nzbs.org",
                PublishDate = new DateTime(2022, 3, 31, 10, 18, 0),
                Size = 361219000
            };

            Mocker.GetMock<IBlocklistRepository>()
                .Setup(v => v.BlocklistedByTitle(12345, release.Title))
                .Returns(new List<Blocklist>
                {
                    new Blocklist
                    {
                        AuthorId = 12345,
                        SourceTitle = "a.different.release",
                        Protocol = DownloadProtocol.Usenet,
                        Indexer = null,
                        PublishedDate = new DateTime(2022, 4, 30, 10, 26, 30),
                        Size = 361219000
                    }
                });

            Subject.Blocklisted(12345, release).Should().BeFalse();
        }
    }
}
