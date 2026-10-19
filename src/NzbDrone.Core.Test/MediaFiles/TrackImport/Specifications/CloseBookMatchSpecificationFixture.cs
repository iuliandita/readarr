using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.MediaFiles.BookImport.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles.BookImport.Specifications
{
    [TestFixture]
    public class CloseBookMatchSpecificationFixture : CoreTest<CloseBookMatchSpecification>
    {
        // A fresh LocalEdition carries a dummy distance of 1.0, far past the 0.20 threshold.
        [TestCase(false, false)]
        [TestCase(true, true)]
        public void should_only_accept_a_distant_new_download_when_the_grabbed_book_matches(bool trusted, bool accepted)
        {
            var edition = new LocalEdition { NewDownload = true, BookOverrideTrusted = trusted };

            Subject.IsSatisfiedBy(edition, null).Accepted.Should().Be(accepted);
        }
    }
}
