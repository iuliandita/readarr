using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Authentication;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.AuthenticationTests
{
    [TestFixture]
    public class UserServiceFixture : TestBase<UserService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IUserRepository>()
                .Setup(r => r.Insert(It.IsAny<User>()))
                .Returns((User u) => u);

            Mocker.GetMock<IUserRepository>()
                .Setup(r => r.Update(It.IsAny<User>()))
                .Returns((User u) => u);
        }

        [Test]
        public void add_should_store_a_salted_hash()
        {
            var user = Subject.Add("user", "password");

            user.Salt.Should().NotBeNullOrWhiteSpace();
            user.Iterations.Should().BeGreaterThan(0);
            user.Password.Should().NotBe("password");
        }

        [Test]
        public void find_user_should_verify_the_password()
        {
            var stored = Subject.Add("user", "password");

            Mocker.GetMock<IUserRepository>().Setup(r => r.FindUser("user")).Returns(stored);

            Subject.FindUser("user", "password").Should().BeSameAs(stored);
            Subject.FindUser("user", "wrong").Should().BeNull();
        }

        [Test]
        public void legacy_sha256_password_should_be_upgraded_on_login()
        {
            var stored = new User
            {
                Username = "user",
                Password = Hashing.SHA256Hash("password")
            };

            Mocker.GetMock<IUserRepository>().Setup(r => r.FindUser("user")).Returns(stored);

            Subject.FindUser("user", "password").Should().BeSameAs(stored);

            stored.Salt.Should().NotBeNullOrWhiteSpace();
            stored.Iterations.Should().BeGreaterThan(0);
            stored.Password.Should().NotBe(Hashing.SHA256Hash("password"));
        }

        [Test]
        public void legacy_sha256_password_should_reject_a_wrong_password()
        {
            var stored = new User
            {
                Username = "user",
                Password = Hashing.SHA256Hash("password")
            };

            Mocker.GetMock<IUserRepository>().Setup(r => r.FindUser("user")).Returns(stored);

            Subject.FindUser("user", "wrong").Should().BeNull();
            stored.Salt.Should().BeNullOrWhiteSpace();
        }

        [Test]
        public void upsert_should_keep_the_hash_when_the_password_is_empty()
        {
            var stored = Subject.Add("user", "password");
            var hash = stored.Password;
            var salt = stored.Salt;

            Mocker.GetMock<IUserRepository>().Setup(r => r.SingleOrDefault()).Returns(stored);

            Subject.Upsert("user", null);

            stored.Password.Should().Be(hash);
            stored.Salt.Should().Be(salt);
        }

        [Test]
        public void upsert_should_rehash_when_a_password_is_provided()
        {
            var stored = Subject.Add("user", "password");
            var hash = stored.Password;

            Mocker.GetMock<IUserRepository>().Setup(r => r.SingleOrDefault()).Returns(stored);

            Subject.Upsert("user", "newpassword");

            stored.Password.Should().NotBe(hash);
            stored.Salt.Should().NotBeNullOrWhiteSpace();
        }
    }
}
