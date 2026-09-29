using System;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Update;

namespace NzbDrone.Core.Test.UpdateTests
{
    [TestFixture]
    public class UpdatePackageProviderFixture : CoreTest<UpdatePackageProvider>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigFileProvider>()
                .SetupGet(c => c.UpdateFeedUrl)
                .Returns("https://api.github.com/repos/Radarr/Radarr");
        }

        [Test]
        public void no_update_when_version_higher()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(999, 0)).Should().BeNull();
        }

        [Test]
        public void finds_update_when_version_lower()
        {
            UseRealHttp();
            Subject.GetLatestUpdate("master", new Version(1, 0)).Should().NotBeNull();
        }

        [Test]
        public void should_get_recent_updates()
        {
            UseRealHttp();
            var recent = Subject.GetRecentUpdates("master", new Version(1, 0), null);

            recent.Should().NotBeEmpty();
            recent.Should().OnlyContain(c => c.Version != null);
            recent.Should().OnlyContain(c => c.Url.StartsWith("https://github.com/"));
            recent.Should().OnlyContain(c => c.ReleaseDate.Year >= 2024);
        }
    }
}
