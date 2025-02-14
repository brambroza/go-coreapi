

# Stage 1: Base image for runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 6600

ENV ASPNETCORE_URLS=http://+:6600

# Creates a non-root user and gives permissions
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Stage 2: Build and restore dependencies
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy everything (including .sln and project dependencies)
COPY . . 

# Restore dependencies before building
RUN dotnet restore

# Ensure build is working correctly
RUN dotnet build -c Release --no-restore -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Stage 4: Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "goalongapi.dll"]

