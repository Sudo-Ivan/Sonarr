using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.DecisionEngine.Specifications.Search;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Tv;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.DecisionEngineTests.Search
{
    [TestFixture]
    public class SeasonMatchSpecificationFixture : TestBase<SeasonMatchSpecification>
    {
        private Series _series;
        private RemoteEpisode _remoteEpisode;
        private ReleaseDecisionInformation _information;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Series>.CreateNew().With(s => s.Id = 1).Build();

            _remoteEpisode = new RemoteEpisode
            {
                Series = _series,
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true,
                    SeasonNumber = 2
                },
                Release = new ReleaseInfo()
            };

            _information = new ReleaseDecisionInformation(false, new SeasonSearchCriteria
            {
                Series = _series,
                SeasonNumber = 2
            });
        }

        [Test]
        public void should_return_true_when_search_criteria_is_null()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 1;
            Subject.IsSatisfiedBy(_remoteEpisode, new ReleaseDecisionInformation()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_when_season_matches()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_season_does_not_match()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumber = 1;
            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_when_multi_season_release_covers_searched_season()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 1, 2, 3 };
            _remoteEpisode.MappedSeasonNumbers = new[] { 1, 2, 3 };

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_when_multi_season_release_does_not_cover_searched_season()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 1, 3 };
            _remoteEpisode.MappedSeasonNumbers = new[] { 1, 3 };

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_use_mapped_season_numbers_when_available()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 1, 2, 3 };
            _remoteEpisode.MappedSeasonNumbers = new[] { 2 };

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_apply_scene_mapping_offset_when_mapped_season_numbers_are_unavailable()
        {
            // Scene seasons 3-5 map to tvdb seasons 2-4
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 3, 4, 5 };
            _remoteEpisode.MappedSeasonNumber = 2;
            _remoteEpisode.MappedSeasonNumbers = [];

            Subject.IsSatisfiedBy(_remoteEpisode, _information).Accepted.Should().BeTrue();
        }
    }
}
