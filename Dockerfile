# Taskmaster Dockerfile
# Multi-stage build for optimized production image

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy source code and build
COPY . ./
RUN dotnet publish -c Release -o out --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS runtime

# Install required packages for process management and signal handling
RUN apt-get update && apt-get install -y \
    procps \
    psmisc \
    bash \
    && rm -rf /var/lib/apt/lists/*

# Create taskmaster user and group
RUN groupadd -r taskmaster && useradd -r -g taskmaster taskmaster

# Create required directories
RUN mkdir -p /app /tmp/taskmaster /var/log/taskmaster && \
    chown -R taskmaster:taskmaster /app /tmp/taskmaster /var/log/taskmaster

WORKDIR /app

# Copy built application
COPY --from=build /app/out .

# Copy configuration files
COPY presentation.config.json ./task.config.json
COPY presentation.config.json ./

# Set permissions
RUN chown -R taskmaster:taskmaster /app

# Switch to taskmaster user
USER taskmaster

# Expose port for socket communication (if needed)
EXPOSE 8080

# Set environment variables
ENV DOTNET_ENVIRONMENT=Production
ENV TASKMASTER_CONFIG_PATH=/app/task.config.json
ENV TASKMASTER_LOG_PATH=/var/log/taskmaster/taskmaster.log

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD pgrep -f Taskmaster || exit 1

# Entry point
ENTRYPOINT ["dotnet", "Taskmaster.dll"]