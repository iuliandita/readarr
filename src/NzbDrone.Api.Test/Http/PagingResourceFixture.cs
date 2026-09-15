using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Datastore;
using Readarr.Http;

namespace NzbDrone.Api.Test.Http
{
    [TestFixture]
    public class PagingResourceFixture
    {
        private static readonly HashSet<string> AllowedSortKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "title",
            "authorMetadata.sortName"
        };

        [Test]
        public void should_keep_allowed_sort_key()
        {
            var pagingSpec = Map("title");

            pagingSpec.SortKey.Should().Be("title");
        }

        [Test]
        public void should_ignore_sort_key_not_in_allowlist()
        {
            var pagingSpec = Map("title\"; DROP TABLE \"Books\"; --");

            pagingSpec.SortKey.Should().Be("id");
        }

        [Test]
        public void should_use_default_when_sort_key_is_null()
        {
            var pagingSpec = Map(null);

            pagingSpec.SortKey.Should().Be("id");
        }

        [Test]
        public void should_use_default_when_no_allowlist_is_configured()
        {
            var pagingSpec = new PagingResource<string>(new PagingRequestResource { SortKey = "anything" })
                .MapToPagingSpec<string, string>(null, "timeleft", SortDirection.Ascending);

            pagingSpec.SortKey.Should().Be("timeleft");
        }

        [Test]
        public void should_ignore_sort_key_case()
        {
            var pagingSpec = Map("AuthorMetadata.SortName");

            pagingSpec.SortKey.Should().Be("AuthorMetadata.SortName");
        }

        [Test]
        public void should_apply_default_direction_when_not_set()
        {
            var pagingSpec = new PagingResource<string>(new PagingRequestResource
            {
                SortKey = "title",
                SortDirection = SortDirection.Default
            }).MapToPagingSpec<string, string>(AllowedSortKeys, "id", SortDirection.Descending);

            pagingSpec.SortDirection.Should().Be(SortDirection.Descending);
        }

        [Test]
        public void should_keep_explicit_direction()
        {
            var pagingSpec = new PagingResource<string>(new PagingRequestResource
            {
                SortKey = "title",
                SortDirection = SortDirection.Ascending
            }).MapToPagingSpec<string, string>(AllowedSortKeys, "id", SortDirection.Descending);

            pagingSpec.SortDirection.Should().Be(SortDirection.Ascending);
        }

        private static PagingSpec<string> Map(string sortKey)
        {
            var request = new PagingRequestResource
            {
                SortKey = sortKey,
                SortDirection = SortDirection.Ascending
            };

            return new PagingResource<string>(request).MapToPagingSpec<string, string>(AllowedSortKeys, "id", SortDirection.Descending);
        }
    }
}
