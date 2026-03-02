# ==============================
# Stage 1: Build
# ==============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files first (for better layer caching)
COPY DreamGuard.BE.sln ./
COPY DreamGuard.BE.API/DreamGuard.BE.API.csproj DreamGuard.BE.API/
COPY DreamGuard.BE.BLL/DreamGuard.BE.BLL.csproj DreamGuard.BE.BLL/
COPY DreamGuard.BE.DAL/DreamGuard.BE.DAL.csproj DreamGuard.BE.DAL/

# Restore NuGet packages
RUN dotnet restore DreamGuard.BE.sln

# Copy the rest of the source code
COPY . .

# Build and publish in Release mode
RUN dotnet publish DreamGuard.BE.API/DreamGuard.BE.API.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# ==============================
# Stage 2: Runtime
# ==============================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create a non-root user for security
RUN adduser --disabled-password --gecos "" appuser

# Copy published output from build stage
COPY --from=build /app/publish .

# Expose the port the app runs on
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Switch to non-root user
USER appuser

# Start the application
ENTRYPOINT ["dotnet", "DreamGuard.BE.API.dll"]
