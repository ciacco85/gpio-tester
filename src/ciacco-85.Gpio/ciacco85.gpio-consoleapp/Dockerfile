# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
#RUN apt-get update \
	#&& apt-get install -y --install-recommends sudo curl \
	#&& curl -SL --output setup-libgpiod.sh https://raw.githubusercontent.com/devdotnetorg/docker-libgpiod/HEAD/setup-libgpiod.sh \
	#&& chmod +x setup-libgpiod.sh \
	#&& ./setup-libgpiod.sh --type binary --version 1.6.3 --canselect no \
	#&& rm setup-libgpiod.sh \
	#&& apt-get -y --purge remove curl unzip
#RUN	apt-get update \
	#&& apt-get install -y sudo curl \
	#&& curl -SL --output setup-libgpiod.sh https://raw.githubusercontent.com/devdotnetorg/docker-libgpiod/HEAD/setup-libgpiod.sh \
	#&& sudo chmod +x setup-libgpiod.sh \
	#&& sudo ./setup-libgpiod.sh
RUN apt-get update \
    && apt-get install -y sudo libgpiod2 gpiod libgpiod-dev libpigpiod-if-dev libpigpiod-if1 libpigpiod-if2-1
#USER $APP_UID
WORKDIR /app


# This stage is used to build the service project
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["ciacco85.gpio-consoleapp/ciacco85.gpio-consoleapp.csproj", "ciacco85.gpio-consoleapp/"]
RUN dotnet restore "./ciacco85.gpio-consoleapp/ciacco85.gpio-consoleapp.csproj" --arch $TARGETARCH
COPY . .
WORKDIR "/src/ciacco85.gpio-consoleapp"
RUN dotnet build "./ciacco85.gpio-consoleapp.csproj" -c $BUILD_CONFIGURATION -o /app/build --arch $TARGETARCH

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ciacco85.gpio-consoleapp.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false --arch $TARGETARCH

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ciacco85.gpio-consoleapp.dll"]