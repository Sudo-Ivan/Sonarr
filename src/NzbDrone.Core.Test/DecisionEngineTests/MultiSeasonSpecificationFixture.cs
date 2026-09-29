using System;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class MultiSeasonSpecificationFixture : CoreTest<MultiSeasonSpecification>
    {
        private RemoteEpisode _remoteEpisode;

        [SetUp]
        public void Setup()
        {
            var series = Builder<Series>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteEpisode = new RemoteEpisode
            {
                ParsedEpisodeInfo = new ParsedEpisodeInfo
                {
                    FullSeason = true,
                    SeasonNumbers = new[] { 1, 2, 3, 4, 5 }
                },
                Episodes = Builder<Episode>.CreateListOfSize(3)
                                           .All()
                                           .With(s => s.SeriesId = series.Id)
                                           .BuildList(),
                Series = series,
                ReleaseSource = ReleaseSourceType.InteractiveSearch,
                Release = new ReleaseInfo
                {
                    Title = "Series.Title.S01-05.720p.BluRay.X264-RlsGrp"
                }
            };
        }

        [Test]
        public void should_return_true_if_is_not_a_multi_season_release()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 1 };
            _remoteEpisode.Episodes.Last().AirDateUtc = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_is_a_multi_season_release_with_resolved_episodes()
        {
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_for_multi_season_release_from_user_invoked_search()
        {
            _remoteEpisode.ReleaseSource = ReleaseSourceType.UserInvokedSearch;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_for_multi_season_release_from_rss()
        {
            _remoteEpisode.ReleaseSource = ReleaseSourceType.Rss;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_for_multi_season_release_from_automatic_search()
        {
            _remoteEpisode.ReleaseSource = ReleaseSourceType.Search;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_for_multi_season_release_from_release_push()
        {
            _remoteEpisode.ReleaseSource = ReleaseSourceType.ReleasePush;
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_true_if_episodes_span_only_some_covered_seasons()
        {
            _remoteEpisode.MappedSeasonNumbers = new[] { 1, 2 };
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_no_episodes_were_resolved()
        {
            _remoteEpisode.Episodes.Clear();
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_covered_seasons_cannot_be_determined()
        {
            _remoteEpisode.ParsedEpisodeInfo.SeasonNumbers = new[] { 0, 0 };
            _remoteEpisode.MappedSeasonNumbers = Array.Empty<int>();
            Subject.IsSatisfiedBy(_remoteEpisode, new()).Accepted.Should().BeFalse();
        }
    }
}
