# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Depending on the operating system of the host machines(s) that will build or run the containers, the image specified in the FROM statement may need to be changed.
# For more information, please see https://aka.ms/containercompat

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0-nanoserver-ltsc2022 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081
EXPOSE 443

ENV SYNCFUSION_LICENSE_KEY=""

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0-nanoserver-ltsc2022 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/ej2-spreadsheet-server/ej2-spreadsheet-server.csproj", "ej2-spreadsheet-server/"]
COPY ["src/ej2-spreadsheet-server/NuGet.Config", "ej2-spreadsheet-server/"]
RUN dotnet restore "./ej2-spreadsheet-server/ej2-spreadsheet-server.csproj"
COPY . .
WORKDIR "/src/ej2-spreadsheet-server"
RUN dotnet build "./ej2-spreadsheet-server.csproj" -c %BUILD_CONFIGURATION% -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ej2-spreadsheet-server.csproj" -c %BUILD_CONFIGURATION% -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app

# Copy system fonts from SDK to runtime for SkiaSharp font resolution
# This ensures fonts are available in the nanoserver container for proper text measurement
COPY --from=build /Windows/Fonts /Windows/Fonts

# Copy published application
COPY --from=publish /app/publish .

# Set environment variable for custom fonts path (optional, if additional fonts are mounted)
ENV CUSTOM_FONTS_PATH=""

ENTRYPOINT ["dotnet", "ej2-spreadsheet-server.dll"]
