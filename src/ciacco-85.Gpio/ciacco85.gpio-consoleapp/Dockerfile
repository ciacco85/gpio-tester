# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ciacco85.gpio-consoleapp/ciacco85.gpio-consoleapp.csproj", "ciacco85.gpio-consoleapp/"]
RUN dotnet restore "./ciacco85.gpio-consoleapp/ciacco85.gpio-consoleapp.csproj"
COPY . .
WORKDIR "/src/ciacco85.gpio-consoleapp"
RUN dotnet build "./ciacco85.gpio-consoleapp.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ciacco85.gpio-consoleapp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ciacco85.gpio-consoleapp.dll"]