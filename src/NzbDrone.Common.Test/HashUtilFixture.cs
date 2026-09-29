using FluentAssertions;
using NUnit.Framework;

namespace NzbDrone.Common.Test
{
    [TestFixture]
    public class HashUtilFixture
    {
        [Test]
        public void should_create_the_same_crc()
        {
            HashUtil.CalculateCrc("test").Should().Be(HashUtil.CalculateCrc("test"));
        }
    }
}
