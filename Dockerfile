FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 443

ENV SYNCFUSION_LICENSE_KEY=""

FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj", "./ej2-spreadsheet-server/"]
COPY ["src/ej2-spreadsheet-server/NuGet.Config", "./ej2-spreadsheet-server/"]
RUN dotnet restore "./ej2-spreadsheet-server/ej2-spreadsheet-server.csproj"
COPY . .
WORKDIR "/src/ej2-spreadsheet-server"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ej2-spreadsheet-server.csproj" -c Release -o /app/publish /p:UseAppHost=false /p:IncludeNativeLibrariesForSelfExtract=false --runtime win-x64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ej2-spreadsheet-server.dll"]