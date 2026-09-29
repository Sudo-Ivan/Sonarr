using System.ComponentModel;
using System.Linq;
using ModelContextProtocol.Server;
using NzbDrone.Core.HealthCheck;
using NzbDrone.Core.Queue;
using NzbDrone.Core.Tv;

namespace Sonarr.Http.Mcp
{
    [McpServerToolType]
    public class SonarrMcpTools
    {
        private readonly ISeriesService _seriesService;
        private readonly IEpisodeService _episodeService;
        private readonly IQueueService _queueService;
        private readonly IHealthCheckService _healthCheckService;

        public SonarrMcpTools(ISeriesService seriesService,
                              IEpisodeService episodeService,
                              IQueueService queueService,
                              IHealthCheckService healthCheckService)
        {
            _seriesService = seriesService;
            _episodeService = episodeService;
            _queueService = queueService;
            _healthCheckService = healthCheckService;
        }

        [McpServerTool(Name = "get_series")]
        [Description("List all series in the library")]
        public object GetSeries()
        {
            return _seriesService.GetAllSeries()
                .OrderBy(s => s.SortTitle)
                .Select(s => new { s.Id, s.Title, s.Year, s.Status, Seasons = s.Seasons?.Count ?? 0 })
                .ToList();
        }

        [McpServerTool(Name = "get_episodes")]
        [Description("List episodes for a series, optionally filtered to one season")]
        public object GetEpisodes([Description("Series id")] int seriesId,
                                  [Description("Optional season number filter")] int? seasonNumber = null)
        {
            var episodes = seasonNumber.HasValue
                ? _episodeService.GetEpisodesBySeason(seriesId, seasonNumber.Value)
                : _episodeService.GetEpisodeBySeries(seriesId);

            return episodes
                .Select(e => new { e.Id, e.SeasonNumber, e.EpisodeNumber, e.Title, e.HasFile, e.AirDateUtc })
                .OrderBy(e => e.SeasonNumber)
                .ThenBy(e => e.EpisodeNumber)
                .ToList();
        }

        [McpServerTool(Name = "get_queue")]
        [Description("List the active download queue")]
        public object GetQueue()
        {
            return _queueService.GetQueue()
                .Select(q => new
                {
                    q.Id,
                    q.Title,
                    Status = q.Status.ToString(),
                    q.Size,
                    q.SizeLeft,
                    q.TimeLeft,
                    Protocol = q.Protocol.ToString()
                })
                .ToList();
        }

        [McpServerTool(Name = "get_health")]
        [Description("List current health check results")]
        public object GetHealth()
        {
            return _healthCheckService.Results()
                .Select(h => new { Type = h.Type.ToString(), Reason = h.Reason.ToString(), h.Message })
                .ToList();
        }
    }
}
