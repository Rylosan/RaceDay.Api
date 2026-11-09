# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file
COPY RaceDay.Web.csproj ./

# Restore dependencies
RUN dotnet restore RaceDay.Web.csproj

# Copy source code
COPY . .

# Publish MVC application
RUN dotnet publish RaceDay.Web.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]