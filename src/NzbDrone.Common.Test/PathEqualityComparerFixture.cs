using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class PathEqualityComparerFixture : TestBase
    {
        private const string MalformedPath = @"/data/media/books/Percy Jackson and the Olympians #5 /Book #5 .pdf";

        [Test]
        public void GetHashCode_should_not_throw_for_a_path_with_a_whitespace_padded_component()
        {
            Action action = () => PathEqualityComparer.Instance.GetHashCode(MalformedPath);

            action.Should().NotThrow();
        }

        [Test]
        public void Equals_should_not_throw_for_a_path_with_a_whitespace_padded_component()
        {
            Action action = () => PathEqualityComparer.Instance.Equals(MalformedPath, "/data/media/books/Other.pdf");

            action.Should().NotThrow();
        }

        [Test]
        public void Equals_should_be_true_for_the_same_malformed_path()
        {
            PathEqualityComparer.Instance.Equals(MalformedPath, MalformedPath).Should().BeTrue();
        }

        [Test]
        public void Equals_should_be_false_for_different_malformed_paths()
        {
            PathEqualityComparer.Instance.Equals(MalformedPath, "/data/media/books/Percy Jackson and the Olympians #5 /Other.pdf")
                .Should().BeFalse();
        }

        [Test]
        public void GetHashCode_should_be_equal_for_equal_paths()
        {
            var copy = new string(MalformedPath.ToCharArray());

            PathEqualityComparer.Instance.GetHashCode(MalformedPath)
                .Should().Be(PathEqualityComparer.Instance.GetHashCode(copy));
        }
    }
}
