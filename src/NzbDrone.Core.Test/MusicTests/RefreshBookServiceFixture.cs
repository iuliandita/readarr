using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class RefreshBookServiceFixture : CoreTest<RefreshBookService>
    {
        private Author _author;
        private Book _localBook;
        private List<Book> _remoteBooks;

        [SetUp]
        public void Setup()
        {
            var metadata = Builder<AuthorMetadata>.CreateNew().Build();

            _author = Builder<Author>.CreateNew()
                .With(a => a.Metadata = metadata)
                .Build();

            // A book the user still owns locally (e.g. an omnibus) that the partial
            // metadata provider no longer returns for the author.
            _localBook = Builder<Book>.CreateNew()
                .With(b => b.Id = 5)
                .With(b => b.ForeignBookId = "99")
                .With(b => b.Author = _author)
                .With(b => b.AuthorMetadata = metadata)
                .With(b => b.AddOptions = new AddBookOptions { AddType = BookAddType.Automatic })
                .Build();

            // Author payload omits the local book (narrow remote coverage).
            _remoteBooks = new List<Book>
            {
                Builder<Book>.CreateNew().With(b => b.ForeignBookId = "1").Build()
            };
        }

        private void GivenBookHasFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByBook(_localBook.Id))
                .Returns(Builder<BookFile>.CreateListOfSize(1).BuildList());
        }

        private void GivenNoBookFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                .Setup(x => x.GetFilesByBook(It.IsAny<int>()))
                .Returns(new List<BookFile>());
        }

        private void GivenProviderLacksBook()
        {
            Mocker.GetMock<IProvideBookInfo>()
                .Setup(x => x.GetBookInfo(_localBook.ForeignBookId))
                .Throws(new BookNotFoundException(_localBook.ForeignBookId));
        }

        [Test]
        public void should_skip_book_and_not_throw_when_provider_lacks_book_but_book_has_files()
        {
            // Partial remote data: author resolves, this book is missing from the
            // author payload AND the provider 404s the book directly. Previously
            // this dereferenced a null Author and crashed the whole author refresh.
            GivenBookHasFiles();
            GivenProviderLacksBook();

            var updated = Subject.RefreshBookInfo(_localBook, _remoteBooks, _author, false);

            updated.Should().BeFalse();

            // Book is retained because it still has files on disk.
            Mocker.GetMock<IBookService>()
                .Verify(x => x.DeleteBook(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never());

            // The book is skipped with an error rather than crashing the refresh.
            ExceptionVerification.IgnoreErrors();
        }

        [Test]
        public void should_delete_book_when_missing_from_metadata_and_no_files()
        {
            // Clean, already-handled path: book gone from metadata and nothing on
            // disk, so it is safe to delete. Locks in that behaviour is unchanged.
            GivenNoBookFiles();

            Mocker.GetMock<IBookService>()
                .Setup(x => x.DeleteBook(_localBook.Id, It.IsAny<bool>(), It.IsAny<bool>()));

            var updated = Subject.RefreshBookInfo(_localBook, _remoteBooks, _author, false);

            updated.Should().BeFalse();

            Mocker.GetMock<IBookService>()
                .Verify(x => x.DeleteBook(_localBook.Id, false, It.IsAny<bool>()), Times.Once());

            ExceptionVerification.IgnoreWarns();
        }
    }
}
