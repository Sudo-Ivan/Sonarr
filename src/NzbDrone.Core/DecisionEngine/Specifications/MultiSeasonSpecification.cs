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
