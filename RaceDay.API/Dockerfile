
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY RaceDay.slnx ./
COPY RaceDay.API/RaceDay.API.csproj RaceDay.API/
COPY RaceDay.Web/RaceDay.Web.csproj RaceDay.Web/

# Restore API dependencies
RUN dotnet restore RaceDay.API/RaceDay.API.csproj

# Copy source code
COPY . .

# Publish API
WORKDIR /src/RaceDay.API
RUN dotnet publish RaceDay.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.API.dll"]
