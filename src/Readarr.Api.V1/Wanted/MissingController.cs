using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.AuthorStats;
using NzbDrone.Core.Books;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.MediaCover;
using NzbDrone.SignalR;
using Readarr.Api.V1.Books;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Wanted
{
    [V1ApiController("wanted/missing")]
    public class MissingController : BookControllerWithSignalR
    {
        public MissingController(IBookService bookService,
                             ISeriesBookLinkService seriesBookLinkService,
                             IAuthorStatisticsService authorStatisticsService,
                             IMapCoversToLocal coverMapper,
                             IUpgradableSpecification upgradableSpecification,
                             IBroadcastSignalRMessage signalRBroadcaster)
        : base(bookService, seriesBookLinkService, authorStatisticsService, coverMapper, upgradableSpecification, signalRBroadcaster)
        {
        }

        [HttpGet]
        public PagingResource<BookResource> GetMissingBooks([FromQuery] PagingRequestResource paging, bool includeAuthor = false, bool monitored = true)
        {
            var pagingResource = new PagingResource<BookResource>(paging);
            var pagingSpec = pagingResource.MapToPagingSpec<BookResource, Book>(
                new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    "authorMetadata.sortName",
                    "books.title",
                    "releaseDate",
                    "books.lastSearchTime"
                },
                "id",
                SortDirection.Descending);

            if (monitored)
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == true && v.Author.Value.Monitored == true);
            }
            else
            {
                pagingSpec.FilterExpressions.Add(v => v.Monitored == false || v.Author.Value.Monitored == false);
            }

            return pagingSpec.ApplyToPage(_bookService.BooksWithoutFiles, v => MapToResource(v, includeAuthor));
        }
    }
}
