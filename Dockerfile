FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
RUN apt-get update
RUN apt-get install -y curl
RUN apt-get install -y libpng-dev libjpeg-dev curl libxi6 build-essential libgl1-mesa-glx
RUN curl -sL https://deb.nodesource.com/setup_lts.x | bash -
RUN apt-get install -y nodejs
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["frontv2.client/nuget.config", "frontv2.client/"]
COPY ["Robot.Server/Robot.Server.csproj", "Robot.Server/"]
COPY ["frontv2.client/frontv2.client.esproj", "frontv2.client/"]
COPY . .
RUN dotnet restore "./Robot.Server/./Robot.Server.csproj"

WORKDIR "/src/frontv2.client"
RUN npm i @rollup/rollup-linux-x64-gnu

WORKDIR "/src/Robot.Server"
RUN dotnet build "./Robot.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "./Robot.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://*:7000;"
ENV API_URL=https://robot.botbot.fr/
ENV SECURE=true
EXPOSE 7000

ENTRYPOINT ["dotnet", "Robot.Server.dll"]