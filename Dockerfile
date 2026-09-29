# syntax=docker/dockerfile:1

FROM node:24.19.0-bookworm-slim@sha256:a9f5f7c91a432850b2a8a7797adf5eadb6c733ceed61167806cee7ea7fbc29df AS ui
WORKDIR /build
COPY package.json yarn.lock ./
RUN corepack enable && corepack yarn install --frozen-lockfile
COPY . .
RUN corepack yarn build

FROM mcr.microsoft.com/dotnet/sdk:10.0.401@sha256:35d40304542c8689331f8cab17c65926cdf48fe711e289321d71924b230a7d29 AS backend
ARG TARGETARCH
WORKDIR /build
COPY . .
RUN rid="linux-$( [ "${TARGETARCH}" = "amd64" ] && echo x64 || echo "${TARGETARCH}" )" && \
    dotnet publish src/NzbDrone.Console/Sonarr.Console.csproj \
    -c Release \
    -f net10.0 \
    -r "${rid}" \
    --self-contained \
    -p:SolutionDir=/build/src/ \
    -o /app/bin && \
    dotnet publish src/NzbDrone.Mono/Sonarr.Mono.csproj \
    -c Release \
    -f net10.0 \
    -r "${rid}" \
    --self-contained \
    -p:SolutionDir=/build/src/ \
    -o /app/bin && \
    mkdir -p /config && chown 1000:1000 /config

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled-extra@sha256:9a3e4e315a3eae3be20b73739ae84fa7e4ffb2a47a234f3bef30825f0543fe1b AS runtime
ARG VERSION="local"
ARG REVISION=""
LABEL org.opencontainers.image.title="Sonarr" \
      org.opencontainers.image.description="PVR for Usenet and BitTorrent users. Multi-season support fork with telemetry removed." \
      org.opencontainers.image.url="https://github.com/Sudo-Ivan/Sonarr" \
      org.opencontainers.image.source="https://github.com/Sudo-Ivan/Sonarr" \
      org.opencontainers.image.documentation="https://github.com/Sudo-Ivan/Sonarr" \
      org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.revision="${REVISION}" \
      org.opencontainers.image.vendor="Sudo-Ivan" \
      org.opencontainers.image.licenses="GPL-3.0" \
      org.opencontainers.image.base.name="mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled-extra"

ENV XDG_CONFIG_HOME=/config/.config \
    SONARR__BRANCH__NAME=master \
    SONARR__AUTH__REQUIRED=DisabledForLocalAddresses \
    COMPlus_EnableDiagnostics=0

WORKDIR /app/sonarr/bin
COPY --from=backend /app/bin/ /app/sonarr/bin/
COPY --from=backend --chown=1000:1000 /config /config
COPY --from=ui /build/_output/UI/ /app/sonarr/bin/UI/

USER 1000
VOLUME /config
EXPOSE 8989

ENTRYPOINT ["/app/sonarr/bin/Sonarr", "-nobrowser", "-data=/config"]
