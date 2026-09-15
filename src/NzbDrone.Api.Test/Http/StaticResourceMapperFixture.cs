using System.IO;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NLog;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Test.Common;
using Readarr.Http.Frontend;
using Readarr.Http.Frontend.Mappers;

namespace NzbDrone.Api.Test.Http
{
    [TestFixture]
    public class StaticResourceMapperFixture : TestBase
    {
        private string _folder;

        [SetUp]
        public void Setup()
        {
            _folder = Path.Combine(TestContext.CurrentContext.TestDirectory, "ui");
        }

        [Test]
        public void should_map_path_within_folder()
        {
            var mapper = new TestMapper(_folder, TestLogger);

            mapper.Map("Content/index.html")
                .Should().Be(Path.GetFullPath(Path.Combine(_folder, "Content", "index.html")));
        }

        [TestCase("../secrets.txt")]
        [TestCase("../../../../etc/passwd")]
        [TestCase("Content/../../secrets.txt")]
        public void should_reject_path_escaping_folder(string resourceUrl)
        {
            var mapper = new TestMapper(_folder, TestLogger);

            mapper.Map(resourceUrl).Should().BeNull();
        }

        [TestCase("../secrets.txt")]
        [TestCase("..%2fsecrets.txt")]
        [TestCase("Content/../../etc/passwd")]
        [TestCase("Content/..%5c..%5csecrets.txt")]
        public void controller_should_reject_invalid_paths(string path)
        {
            var controller = new StaticResourceController(new[] { new TestMapper(_folder, TestLogger) }, TestLogger);

            controller.Index(path).Should().BeOfType<NotFoundResult>();
        }

        [TestCase("../secrets.txt")]
        [TestCase("..%2fsecrets.txt")]
        public void controller_should_reject_invalid_content_paths(string path)
        {
            var controller = new StaticResourceController(new[] { new TestMapper(_folder, TestLogger) }, TestLogger);

            controller.IndexContent(path).Should().BeOfType<NotFoundResult>();
        }

        private class TestMapper : StaticResourceMapperBase
        {
            private readonly string _folder;

            public TestMapper(string folder, Logger logger)
                : base(new Mock<IDiskProvider>().Object, logger)
            {
                _folder = folder;
            }

            protected override string FolderPath => _folder;

            protected override string MapPath(string resourceUrl)
            {
                return Path.Combine(_folder, resourceUrl.Replace('/', Path.DirectorySeparatorChar).Trim(Path.DirectorySeparatorChar));
            }

            public override bool CanHandle(string resourceUrl)
            {
                return true;
            }
        }
    }
}
