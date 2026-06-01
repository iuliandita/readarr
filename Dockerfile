# syntax=docker/dockerfile:1.7

FROM node:24-trixie-slim AS frontend-build

WORKDIR /src

COPY package.json yarn.lock .yarnrc tsconfig.json ./
COPY frontend ./frontend

RUN yarn install --frozen-lockfile --network-timeout 120000 \
    && yarn build --env production


FROM mcr.microsoft.com/dotnet/sdk:10.0-noble AS backend-build

ARG TARGETARCH

WORKDIR /src

COPY .editorconfig ./
COPY Logo ./Logo
COPY src ./src

RUN case "${TARGETARCH}" in \
        amd64) runtime_id="linux-musl-x64" ;; \
        arm64) runtime_id="linux-musl-arm64" ;; \
        *) echo "Unsupported arch: ${TARGETARCH}" >&2; exit 1 ;; \
    esac \
    && dotnet publish src/NzbDrone.Console/Readarr.Console.csproj \
        -c Release \
        -f net10.0 \
        -r "${runtime_id}" \
        --self-contained true \
        -p:EnableAnalyzers=false \
        -p:EnforceCodeStyleInBuild=false \
        -o /out/app \
    && dotnet publish src/NzbDrone.Mono/Readarr.Mono.csproj \
        -c Release \
        -f net10.0 \
        -r "${runtime_id}" \
        --self-contained true \
        -p:EnableAnalyzers=false \
        -p:EnforceCodeStyleInBuild=false \
        -o /out/mono \
    && cp -a /out/mono/Readarr.Mono* /out/app/ \
    && cp -a /out/mono/Mono.Posix.NETStandard* /out/app/ \
    && cp -a /out/mono/libMonoPosixHelper* /out/app/


FROM ghcr.io/linuxserver/baseimage-alpine:3.23 AS runtime-alpine

ARG BUILD_DATE
ARG VERSION=dev
ARG READARR_BRANCH=develop
ARG REPOSITORY_URL=https://github.com/iuliandita/readarr

LABEL build_version="Readarr version:- ${VERSION} Build-date:- ${BUILD_DATE}" \
      maintainer="Readarr maintenance fork" \
      org.opencontainers.image.authors="Readarr maintenance fork" \
      org.opencontainers.image.created="${BUILD_DATE}" \
      org.opencontainers.image.description="Readarr container image with LinuxServer-compatible runtime layout" \
      org.opencontainers.image.documentation="${REPOSITORY_URL}" \
      org.opencontainers.image.source="${REPOSITORY_URL}" \
      org.opencontainers.image.title="Readarr" \
      org.opencontainers.image.url="${REPOSITORY_URL}" \
      org.opencontainers.image.vendor="Readarr maintenance fork" \
      org.opencontainers.image.version="${VERSION}"

ENV XDG_CONFIG_HOME="/config/xdg" \
    COMPlus_EnableDiagnostics=0 \
    TMPDIR=/run/readarr-temp

RUN apk add --no-cache \
      icu-libs \
      sqlite-libs \
      xmlstarlet \
    && mkdir -p /app/readarr/bin \
    && printf 'UpdateMethod=docker\nBranch=%s\nPackageVersion=%s\nPackageAuthor=Readarr maintenance fork\n' "${READARR_BRANCH}" "${VERSION}" > /app/readarr/package_info \
    && printf 'Readarr version: %s\nBuild-date: %s\n' "${VERSION}" "${BUILD_DATE}" > /build_version

COPY --from=backend-build /out/app/ /app/readarr/bin/
COPY --from=frontend-build /src/_output/UI /app/readarr/bin/UI
COPY LICENSE.md /app/readarr/bin/
COPY root/ /

RUN chmod +x \
      /etc/s6-overlay/s6-rc.d/init-readarr-config/run \
      /etc/s6-overlay/s6-rc.d/svc-readarr/run \
      /etc/s6-overlay/s6-rc.d/svc-readarr/data/check

EXPOSE 8787
VOLUME ["/config"]
