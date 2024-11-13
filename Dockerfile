FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0-alpine as buildnode

ARG TARGETPLATFORM
ARG VERSION

LABEL description="This image builds the Sensor Server"
LABEL vendor="ser.soft GmbH"
LABEL maintainer="florian.friedrich@sersoft.de"
LABEL version="${VERSION}"

ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

WORKDIR /sensor-server

RUN <<EOC
set -eux
case "${TARGETPLATFORM}" in
    'linux/amd64') export RUNTIME_ID='linux-musl-x64' ;;
    'linux/arm64') export RUNTIME_ID='linux-musl-arm64' ;;
    *)
      echo "Unsupported platform: ${TARGETPLATFORM}"
      exit 1
    ;;
esac
echo -n "${RUNTIME_ID}" > /tmp/runtime-id
EOC

COPY server/*.sln .
WORKDIR /sensor-server/SensorServer
COPY server/SensorServer/*.csproj .
WORKDIR /sensor-server
RUN unset VERSION; \
    dotnet restore --runtime "$(cat /tmp/runtime-id)"

COPY server/ ./

RUN <<EOC
set -eux
RUNTIME_ID="$(cat /tmp/runtime-id)"
if echo "${VERSION}" | grep -Eq '^[0-9]+.[0-9]+.[0-9]+'; then
  PROJECT_VERSION="${VERSION}"
else
  PROJECT_VERSION="0.0.1-${VERSION}"
fi
rm -f appsettings.Development.json
unset VERSION
dotnet publish --no-restore \
  --configuration release \
  --runtime "${RUNTIME_ID}" \
  --no-self-contained \
  --output ./dist \
  -p:Version="${PROJECT_VERSION}"
EOC


FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine

ARG VERSION

LABEL description="This image runs the Sensor Server"
LABEL vendor="ser.soft GmbH"
LABEL maintainer="florian.friedrich@sersoft.de"
LABEL version="${VERSION}"

ENV TZ=UTC
ENV SERVER_PORT=8080
ENV ASPNETCORE_URLS=http://+:${SERVER_PORT}
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

RUN <<EOC
set -eux
apk add --update --no-cache curl icu-libs icu-data-full
mkdir -p /sensor-server/data
addgroup --system --gid 1000 sensor-server
adduser --system --uid 1000 --ingroup sensor-server --home /sensor-server sensor-server
chown -R sensor-server:sensor-server /sensor-server
EOC

WORKDIR /sensor-server

USER sensor-server:sensor-server

EXPOSE ${SERVER_PORT}

COPY bin/docker-healthcheck.sh /usr/local/bin/docker-healthcheck.sh
COPY --from=buildnode /sensor-server/dist/ ./

VOLUME [ "/sensor-server/data" ]

HEALTHCHECK --interval=10s --timeout=3s --start-period=30s \
    CMD ["/usr/local/bin/docker-healthcheck.sh"]

ENTRYPOINT ["/sensor-server/SensorServer"]
