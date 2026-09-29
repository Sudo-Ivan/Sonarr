using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Http;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Core.Update
{
    public interface IUpdatePackageProvider
    {
        UpdatePackage GetLatestUpdate(string branch, Version currentVersion);
        List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion = null);
    }

    public class UpdatePackageProvider : IUpdatePackageProvider
    {
        private const int MaxReleases = 10;

        private readonly IHttpClient _httpClient;
        private readonly IConfigFileProvider _configFileProvider;

        public UpdatePackageProvider(IHttpClient httpClient, IConfigFileProvider configFileProvider)
        {
            _httpClient = httpClient;
            _configFileProvider = configFileProvider;
        }

        public UpdatePackage GetLatestUpdate(string branch, Version currentVersion)
        {
            var releases = GetReleases();

            var latest = releases
                .Select(r => new { Release = r, Version = ParseVersion(r.TagName) })
                .Where(r => r.Version != null)
                .OrderByDescending(r => r.Version)
                .FirstOrDefault();

            if (latest == null || latest.Version <= currentVersion)
            {
                return null;
            }

            return MapRelease(latest.Release, latest.Version, branch);
        }

        public List<UpdatePackage> GetRecentUpdates(string branch, Version currentVersion, Version previousVersion = null)
        {
            return GetReleases()
                .Select(r => new { Release = r, Version = ParseVersion(r.TagName) })
                .Where(r => r.Version != null)
                .OrderByDescending(r => r.Version)
                .Take(MaxReleases)
                .Select(r => MapRelease(r.Release, r.Version, branch))
                .ToList();
        }

        private List<GitHubRelease> GetReleases()
        {
            var request = new HttpRequestBuilder(_configFileProvider.UpdateFeedUrl)
                .Resource("/releases")
                .AddQueryParam("per_page", MaxReleases)
                .Build();

            var response = _httpClient.Get<List<GitHubRelease>>(request);

            return response.Resource?.Where(r => !r.Draft && !r.Prerelease).ToList() ?? new List<GitHubRelease>();
        }

        private static Version ParseVersion(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return null;
            }

            var versionString = tagName.TrimStart('v', 'V');

            return Version.TryParse(versionString, out var version) ? version : null;
        }

        private static UpdatePackage MapRelease(GitHubRelease release, Version version, string branch)
        {
            return new UpdatePackage
            {
                Version = version,
                ReleaseDate = release.PublishedAt,
                Url = release.HtmlUrl,
                Branch = branch
            };
        }
    }
}
