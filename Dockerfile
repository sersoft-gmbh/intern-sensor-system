FROM mcr.microsoft.com/dotnet/sdk:8.0-jammy as buildnode

ARG VERSION

LABEL description="This image builds the Sensor Server"
LABEL vendor="ser.soft GmbH"
LABEL maintainer="florian.friedrich@sersoft.de"
LABEL version="${VERSION}"

RUN apt-get update --quiet \
    && apt-get install --quiet --yes --no-install-recommends openssl ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /sensor-server

COPY server/*.sln .
WORKDIR /sensor-server/SensorServer
COPY server/SensorServer/*.csproj .
WORKDIR /sensor-server
RUN unset VERSION; \
    dotnet restore

COPY server/ ./
RUN unset VERSION; \
	dotnet publish --no-restore -c release -o /dist



FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy

ARG VERSION

LABEL description="This image runs the Sensor Server"
LABEL vendor="ser.soft GmbH"
LABEL maintainer="florian.friedrich@sersoft.de"
LABEL version="${VERSION}"

RUN export DEBIAN_FRONTEND=noninteractive DEBCONF_NONINTERACTIVE_SEEN=true \
    && apt-get -qq update && apt-get -q dist-upgrade -y && apt-get -q install -y curl \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /sensor-server \
    && mkdir -p /sensor-server/data \
    && groupadd --gid 1000 sensor-server \
    && useradd --uid 1000 --gid 1000 -m sensor-server \
    && chown -R sensor-server:sensor-server /sensor-server

WORKDIR /sensor-server

USER sensor-server:sensor-server

ENV SERVER_PORT=8080
ENV ASPNETCORE_URLS=http://0.0.0.0:${SERVER_PORT}
ENV ASPNETCORE_ENVIRONMENT Production

EXPOSE ${SERVER_PORT}

COPY bin/docker-healthcheck.sh /usr/local/bin/docker-healthcheck.sh
COPY --from=buildnode /dist/ ./

VOLUME [ "/sensor-server/data" ]

HEALTHCHECK --interval=10s --timeout=3s --start-period=30s \
    CMD ["/usr/local/bin/docker-healthcheck.sh"]

ENTRYPOINT ["dotnet", "/sensor-server/SensorServer.dll"]
