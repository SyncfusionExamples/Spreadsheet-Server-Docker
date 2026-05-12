ARG ASPNET_IMAGE=mcr.microsoft.com/dotnet/aspnet:8.0.27-noble@sha256:cdd262e8545b4313dad7e375ec0ce9703bc973d6384e275f7c7d42be9be93de7
ARG SDK_IMAGE=mcr.microsoft.com/dotnet/sdk:8.0.421-noble@sha256:16411e23fa6602381d54c1926a44d934bbe0c7fb680319c89bcd8f0e360a12e6

FROM ${ASPNET_IMAGE} AS base

RUN apt-get update -y \
    && apt-get install -y --no-install-recommends fontconfig \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080

ENV SYNCFUSION_LICENSE_KEY=""
ENV LC_ALL=en_US.UTF-8
ENV LANGUAGE=en_US.UTF-8
ENV LANG=en_US.UTF-8

FROM ${SDK_IMAGE} AS build

WORKDIR /source
COPY ["src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj", "src/ej2-spreadsheet-server/"]
COPY ["src/ej2-spreadsheet-server/packages.lock.json", "src/ej2-spreadsheet-server/"]
COPY ["src/ej2-spreadsheet-server/NuGet.Config", "src/ej2-spreadsheet-server/"]
RUN dotnet restore "src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj" --locked-mode
COPY . .
RUN dotnet build "src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj" -c Release --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ej2-spreadsheet-server.dll"]
