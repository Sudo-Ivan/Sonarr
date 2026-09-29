# syntax=docker/dockerfile:1

FROM node:24.19.0-bookworm-slim@sha256:a9f5f7c91a432850b2a8a7797adf5eadb6c733ceed61167806cee7ea7fbc29df AS ui
WORKDIR /build
COPY package.json yarn.lock ./
RUN corepack enable && corepack yarn install --frozen-lockfile
COPY . .
RUN corepack yarn build

FROM mcr.microsoft.com/dotnet/sdk:10.0.401@sha256:35d40304542c8689331f8cab17c65926cdf48fe711e289321d71924b230a7d29 AS backend
WORKDIR /build
COPY . .
RUN dotnet publish src/NzbDrone.Console/Sonarr.Console.csproj \
    -c Release \
    -f net10.0 \
    -r linux-x64 \
    --self-contained \
    -p:SolutionDir=/build/src/ \
    -o /app/bin && \
    dotnet publish src/NzbDrone.Mono/Sonarr.Mono.csproj \
    -c Release \
    -f net10.0 \
    -r linux-x64 \
    --self-contained \
    -p:SolutionDir=/build/src/ \
    -o /app/bin

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble@sha256:099f6f87ed745377dd27bd722f0d1a352bca71b4fddaabfd75e7c064bcaa82da AS runtime
LABEL org.opencontainers.image.source="https://github.com/Sudo-Ivan/Sonarr" \
      org.opencontainers.image.licenses="GPL-3.0"

ENV XDG_CONFIG_HOME=/config/.config \
    SONARR__BRANCH__NAME=multi-season-support \
    SONARR__AUTH__REQUIRED=DisabledForLocalAddresses \
    COMPlus_EnableDiagnostics=0

RUN mkdir -p /config && chown app:app /config

WORKDIR /app/sonarr/bin
COPY --from=backend /app/bin/ /app/sonarr/bin/
COPY --from=ui /build/_output/UI/ /app/sonarr/bin/UI/

USER app
VOLUME /config
EXPOSE 8989

ENTRYPOINT ["/app/sonarr/bin/Sonarr", "-nobrowser", "-data=/config"]
