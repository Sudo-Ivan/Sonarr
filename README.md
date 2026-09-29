# Sonarr

Sonarr is a PVR for Usenet and BitTorrent users. It monitors multiple RSS feeds for new episodes of your favorite shows and will grab, sort and rename them. It can also be configured to automatically upgrade the quality of files already downloaded when a better quality format becomes available.

This is a hard fork of the upstream Sonarr project with telemetry and Sentry removed, plus additional features listed below.

## Fork changes

- Multi-season release support for user-initiated grabs (interactive and user-invoked search, plus externally added download client items). RSS and automatic search releases stay rejected
- Per-season queue counts and a multi-season indicator in interactive search results
- Sentry error reporting, analytics and piwik tracking removed, including the anonymous user hash sent on startup
- Self-contained multi-arch Docker image published to ghcr.io, cosign signed and trivy scanned

## Features

- Support for major platforms: Windows, Linux, macOS, Raspberry Pi, etc.
- Automatically detects new episodes
- Scans your existing library and downloads missing episodes
- Automatic quality upgrades when better releases appear
- Automatic failed download handling
- Manual search
- Fully configurable episode renaming
- Full integration with SABnzbd, NZBGet, qBittorrent, Deluge, rTorrent, Transmission, uTorrent and other clients

## Docker

```sh
docker run -d \
  -p 8989:8989 \
  -v sonarr-config:/config \
  -v /media:/media \
  ghcr.io/sudo-ivan/sonarr:latest
```

Multi-arch image: linux/amd64 and linux/arm64, zstd compressed, non-root, cosign keyless signed.

## Building

```sh
dotnet build src/NzbDrone.Console/Sonarr.Console.csproj
yarn install --frozen-lockfile && yarn build
```
