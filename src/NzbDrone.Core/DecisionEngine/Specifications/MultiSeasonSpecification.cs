using System.Linq;
using NLog;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications
{
    public class MultiSeasonSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;

        public MultiSeasonSpecification(Logger logger)
        {
            _logger = logger;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public virtual DownloadSpecDecision IsSatisfiedBy(RemoteEpisode subject, ReleaseDecisionInformation information)
        {
            if (!subject.ParsedEpisodeInfo.IsMultiSeason)
            {
                return DownloadSpecDecision.Accept();
            }

            // Multi-season packs are only supported for user initiated grabs and downloads
            // added to the client outside of Sonarr, keep rejecting them for automatic grabs
            if (subject.ReleaseSource is ReleaseSourceType.Rss or ReleaseSourceType.ReleasePush or ReleaseSourceType.Search)
            {
                _logger.Debug("Multi-season release {0} rejected. Not supported for automatic grabs", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiSeason, "Multi-season releases are not supported for automatic grabs");
            }

            var coveredSeasonNumbers = (subject.MappedSeasonNumbers.Any()
                                            ? subject.MappedSeasonNumbers
                                            : subject.ParsedEpisodeInfo.SeasonNumbers)
                                       .Where(n => n > 0)
                                       .Distinct()
                                       .ToList();

            if (!coveredSeasonNumbers.Any())
            {
                _logger.Debug("Multi-season release {0} rejected. Unable to determine covered seasons", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiSeason, "Multi-season release rejected. Unable to determine covered seasons.");
            }

            if (!subject.Episodes.Any())
            {
                _logger.Debug("Multi-season release {0} rejected. No episodes could be resolved for the covered seasons", subject.Release.Title);
                return DownloadSpecDecision.Reject(DownloadRejectionReason.MultiSeason, "Multi-season release rejected. No episodes could be resolved for the covered seasons.");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
