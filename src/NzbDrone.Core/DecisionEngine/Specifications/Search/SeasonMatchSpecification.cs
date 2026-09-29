using System.Linq;
using NLog;
using NzbDrone.Core.DataAugmentation.Scene;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.DecisionEngine.Specifications.Search
{
    public class SeasonMatchSpecification : IDownloadDecisionEngineSpecification
    {
        private readonly Logger _logger;
        private readonly ISceneMappingService _sceneMappingService;

        public SeasonMatchSpecification(ISceneMappingService sceneMappingService, Logger logger)
        {
            _logger = logger;
            _sceneMappingService = sceneMappingService;
        }

        public SpecificationPriority Priority => SpecificationPriority.Default;
        public RejectionType Type => RejectionType.Permanent;

        public DownloadSpecDecision IsSatisfiedBy(RemoteEpisode remoteEpisode, ReleaseDecisionInformation information)
        {
            if (information.SearchCriteria == null)
            {
                return DownloadSpecDecision.Accept();
            }

            var singleEpisodeSpec = information.SearchCriteria as SeasonSearchCriteria;

            if (singleEpisodeSpec == null)
            {
                return DownloadSpecDecision.Accept();
            }

            if (remoteEpisode.ParsedEpisodeInfo.IsMultiSeason)
            {
                var coveredSeasonNumbers = remoteEpisode.MappedSeasonNumbers;

                if (coveredSeasonNumbers.Length == 0)
                {
                    var offset = (remoteEpisode.MappedSeasonNumber ?? remoteEpisode.ParsedEpisodeInfo.SeasonNumber ?? 0) -
                                 (remoteEpisode.ParsedEpisodeInfo.SeasonNumber ?? 0);

                    coveredSeasonNumbers = remoteEpisode.ParsedEpisodeInfo.SeasonNumbers.Select(s => s + offset).ToArray();
                }

                if (!coveredSeasonNumbers.Contains(singleEpisodeSpec.SeasonNumber))
                {
                    _logger.Debug("Searched season is not covered by the multi-season release, skipping.");
                    return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeason, "Wrong season");
                }

                return DownloadSpecDecision.Accept();
            }

            var seasonNumber = remoteEpisode.ParsedEpisodeInfo.SeasonNumber ?? remoteEpisode.MappedSeasonNumber;

            if (singleEpisodeSpec.SeasonNumber != seasonNumber)
            {
                _logger.Debug("Season number does not match searched season number, skipping.");
                return DownloadSpecDecision.Reject(DownloadRejectionReason.WrongSeason, "Wrong season");
            }

            return DownloadSpecDecision.Accept();
        }
    }
}
