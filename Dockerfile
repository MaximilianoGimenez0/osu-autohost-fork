FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy pre-compiled local library dependencies from lib/
COPY ["lib/", "lib/"]

# Copy project files and restore dependencies
COPY ["BanchoMultiplayerBot.sln", "./"]
COPY ["BanchoMultiplayerBot/BanchoMultiplayerBot.csproj", "BanchoMultiplayerBot/"]
COPY ["BanchoMultiplayerBot.Bancho/BanchoMultiplayerBot.Bancho.csproj", "BanchoMultiplayerBot.Bancho/"]
COPY ["BanchoMultiplayerBot.Database/BanchoMultiplayerBot.Database.csproj", "BanchoMultiplayerBot.Database/"]
COPY ["BanchoMultiplayerBot.Osu/BanchoMultiplayerBot.Osu.csproj", "BanchoMultiplayerBot.Osu/"]
COPY ["BanchoMultiplayerBot.Host.WebApi/BanchoMultiplayerBot.Host.WebApi.csproj", "BanchoMultiplayerBot.Host.WebApi/"]

RUN dotnet restore "BanchoMultiplayerBot.Host.WebApi/BanchoMultiplayerBot.Host.WebApi.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "BanchoMultiplayerBot.Host.WebApi/BanchoMultiplayerBot.Host.WebApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 5215
ENV ASPNETCORE_URLS=http://+:5215

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BanchoMultiplayerBot.Host.WebApi.dll"]

