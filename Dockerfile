# Stage 1: Build & Publish (.NET 10 SDK)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY CoreDeck.csproj ./
RUN dotnet restore CoreDeck.csproj

COPY . .
RUN dotnet publish CoreDeck.csproj -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Hermetic Runtime (.NET 10 ASP.NET Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV UseInMemoryDatabase=true

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "CoreDeck.dll"]
