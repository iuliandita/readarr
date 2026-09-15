using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class DateTimeExtensionsFixture
    {
        [Test]
        public void should_remove_sub_second_ticks()
        {
            var dateTime = new DateTime(2024, 1, 1, 12, 30, 45).AddMilliseconds(123);

            dateTime.WithoutTicks().Should().Be(new DateTime(2024, 1, 1, 12, 30, 45));
        }

        [Test]
        public void should_keep_whole_seconds()
        {
            var dateTime = new DateTime(2024, 1, 1, 12, 30, 45);

            dateTime.WithoutTicks().Should().Be(dateTime);
        }
    }
}
