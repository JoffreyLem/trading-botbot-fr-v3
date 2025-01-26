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
COPY ["robot-project-v3.client/nuget.config", "frontv2.client/"]
COPY ["robot-project-v3.Server/robot-project-v3.Server.csproj", "Robot.Server/"]
COPY ["robot-project-v3.client/robot-project-v3.client.esproj", "frontv2.client/"]
COPY . .
RUN dotnet restore "./robot-project-v3.Server/./robot-project-v3.Server.csproj"

WORKDIR "/src/robot-project-v3.client"
RUN npm i @rollup/rollup-linux-x64-gnu

WORKDIR "/src/robot-project-v3.Server"
RUN dotnet build "./robot-project-v3.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ENV DOTNET_DEV_CERTS__DISABLE=1
RUN dotnet publish "./robot-project-v3.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS="http://*:7500;"
ENV API_URL=https://robot.botbot.fr/
ENV SECURE=true
EXPOSE 7500

ENTRYPOINT ["dotnet", "robot-project-v3.Server.dll"]