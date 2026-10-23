FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

RUN apt-get update && \
    apt-get upgrade -y && \
    apt-get install -y fontconfig && \
    rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80
EXPOSE 443

ENV SYNCFUSION_LICENSE_KEY=""
ENV LC_ALL=en_US.UTF-8
ENV LANGUAGE=en_US.UTF-8
ENV LANG=en_US.UTF-8

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source
COPY ["src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj", "./ej2-spreadsheet-server/ej2-spreadsheet-server.csproj"]
COPY ["src/ej2-spreadsheet-server/NuGet.Config", "./ej2-spreadsheet-server/"]
RUN dotnet restore "./ej2-spreadsheet-server/ej2-spreadsheet-server.csproj"
COPY . .
WORKDIR "/source/src"
RUN dotnet build -c Release -o /app

FROM build AS publish
RUN dotnet publish -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "ej2-spreadsheet-server.dll"]