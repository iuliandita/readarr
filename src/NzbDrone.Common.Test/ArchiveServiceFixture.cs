using System.IO;
using System.Text;
using FluentAssertions;
using ICSharpCode.SharpZipLib.Zip;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class ArchiveServiceFixture : TestBase<ArchiveService>
    {
        [Test]
        public void should_extract_zip()
        {
            var destination = Path.Combine(TempFolder, "destination");
            var archive = CreateZip("book.epub", "content");

            Directory.CreateDirectory(destination);

            Subject.Extract(archive, destination);

            File.ReadAllText(Path.Combine(destination, "book.epub")).Should().Be("content");
        }

        [Test]
        public void should_reject_zip_entry_outside_destination()
        {
            var destination = Path.Combine(TempFolder, "destination");
            var archive = CreateZip("../escaped.epub", "content");

            Directory.CreateDirectory(destination);

            Subject.Invoking(s => s.Extract(archive, destination))
                .Should().Throw<IOException>();

            File.Exists(Path.Combine(TempFolder, "escaped.epub")).Should().BeFalse();
        }

        private string CreateZip(string entryName, string content)
        {
            var path = GetTempFilePath() + ".zip";

            using (var fileStream = File.Create(path))
            using (var zipStream = new ZipOutputStream(fileStream))
            {
                var entry = new ZipEntry(entryName);
                zipStream.PutNextEntry(entry);

                var data = Encoding.UTF8.GetBytes(content);
                zipStream.Write(data, 0, data.Length);
                zipStream.CloseEntry();
            }

            return path;
        }
    }
}
