#!/bin/bash

# Docker build script for containerized deployment
# Taskmaster Project

set -e

echo "🐳 Building Taskmaster Docker images..."
echo "======================================"

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "❌ Error: Docker not found"
    echo "Please install Docker from https://docker.com/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Error: Docker daemon not running"
    echo "Please start Docker daemon"
    exit 1
fi

echo "✅ Docker is available and running"

# Build main Taskmaster image
echo ""
echo "🔨 Building Taskmaster image..."
docker build -t taskmaster:latest .

# Build development image with additional tools
echo ""
echo "🔨 Building Taskmaster development image..."
cat > Dockerfile.dev << 'EOF'
FROM taskmaster:latest

# Install development tools
RUN apt-get update && apt-get install -y \
    gdb \
    valgrind \
    strace \
    htop \
    vim \
    git \
    && rm -rf /var/lib/apt/lists/*

# Add development configuration
COPY docker-config.json /app/test.config.json

# Set up development environment
WORKDIR /app
USER taskmaster

# Development entrypoint
CMD ["./Taskmaster"]
EOF

docker build -f Dockerfile.dev -t taskmaster:dev .
rm Dockerfile.dev

# Build minimal production image
echo ""
echo "🔨 Building Taskmaster minimal image..."
cat > Dockerfile.minimal << 'EOF'
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

# Create non-root user
RUN adduser -D -s /bin/sh taskmaster

# Create app directory
WORKDIR /app

# Copy only the published application
COPY --from=taskmaster:latest /app/Taskmaster /app/
COPY --from=taskmaster:latest /app/tests/ /app/tests/
COPY --from=taskmaster:latest /app/*.config.json /app/

# Set ownership
RUN chown -R taskmaster:taskmaster /app

# Switch to non-root user
USER taskmaster

# Create necessary directories
RUN mkdir -p /tmp /app/logs

EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD pgrep Taskmaster || exit 1

CMD ["./Taskmaster"]
EOF

docker build -f Dockerfile.minimal -t taskmaster:minimal .
rm Dockerfile.minimal

# Tag images for different purposes
docker tag taskmaster:latest taskmaster:full
docker tag taskmaster:dev taskmaster:development
docker tag taskmaster:minimal taskmaster:production

echo ""
echo "📦 Creating Docker export packages..."

# Export images as tar files
mkdir -p release/docker

echo "Exporting taskmaster:latest..."
docker save taskmaster:latest | gzip > release/docker/taskmaster-latest.tar.gz

echo "Exporting taskmaster:dev..."
docker save taskmaster:dev | gzip > release/docker/taskmaster-dev.tar.gz

echo "Exporting taskmaster:minimal..."
docker save taskmaster:minimal | gzip > release/docker/taskmaster-minimal.tar.gz

# Create docker-compose files for different scenarios
echo ""
echo "📝 Creating Docker Compose files..."

# Development compose
cat > release/docker/docker-compose.dev.yml << 'EOF'
version: '3.8'

services:
  taskmaster-dev:
    image: taskmaster:dev
    container_name: taskmaster-development
    volumes:
      - ./logs:/app/logs
      - ./configs:/app/configs
    environment:
      - TASKMASTER_ENV=development
      - TASKMASTER_LOG_LEVEL=debug
    restart: unless-stopped
    init: true
    security_opt:
      - no-new-privileges:true
    read_only: false
    tmpfs:
      - /tmp
    ports:
      - "8080:8080"
EOF

# Production compose
cat > release/docker/docker-compose.prod.yml << 'EOF'
version: '3.8'

services:
  taskmaster:
    image: taskmaster:minimal
    container_name: taskmaster-production
    volumes:
      - ./logs:/app/logs:rw
      - ./configs:/app/configs:ro
    environment:
      - TASKMASTER_ENV=production
      - TASKMASTER_LOG_LEVEL=info
    restart: always
    init: true
    security_opt:
      - no-new-privileges:true
    read_only: true
    tmpfs:
      - /tmp
      - /app/logs
    healthcheck:
      test: ["CMD", "pgrep", "Taskmaster"]
      interval: 30s
      timeout: 10s
      retries: 3
      start_period: 30s
    ports:
      - "8080:8080"
    deploy:
      resources:
        limits:
          cpus: '1.0'
          memory: 512M
        reservations:
          cpus: '0.5'
          memory: 256M
EOF

# Create usage instructions
cat > release/docker/README.md << 'EOF'
# Taskmaster Docker Deployment

## Available Images

- `taskmaster:latest` - Full image with all features
- `taskmaster:dev` - Development image with debugging tools  
- `taskmaster:minimal` - Minimal production image
- `taskmaster:production` - Alias for minimal

## Quick Start

### Development
```bash
docker run -d --name taskmaster-dev \
  -v $(pwd)/logs:/app/logs \
  taskmaster:dev
```

### Production
```bash
docker run -d --name taskmaster-prod \
  --restart always \
  --read-only \
  --tmpfs /tmp \
  -v $(pwd)/logs:/app/logs \
  taskmaster:minimal
```

## Using Docker Compose

### Development
```bash
docker-compose -f docker-compose.dev.yml up -d
```

### Production
```bash
docker-compose -f docker-compose.prod.yml up -d
```

## Loading from Export

```bash
# Load image from tar.gz
docker load < taskmaster-latest.tar.gz

# Or with gunzip
gunzip -c taskmaster-latest.tar.gz | docker load
```

## Configuration

Mount your configuration files:
```bash
docker run -v $(pwd)/my-config.json:/app/test.config.json taskmaster:latest
```

## Logs and Monitoring

Logs are written to `/app/logs` inside container. Mount this directory:
```bash
-v $(pwd)/logs:/app/logs
```

## Security Notes

- Images run as non-root user `taskmaster`
- Production image is read-only with tmpfs for temporary files
- No new privileges allowed
- Resource limits configured in production compose
EOF

echo ""
echo "🎉 Docker builds completed successfully!"
echo "======================================="
echo ""
echo "📋 Available Docker images:"
docker images | grep taskmaster

echo ""
echo "📁 Docker release files:"
ls -la release/docker/

echo ""
echo "🚀 Usage examples:"
echo "  Development: docker run -d taskmaster:dev"
echo "  Production:  docker run -d taskmaster:minimal"
echo "  Compose:     docker-compose -f release/docker/docker-compose.prod.yml up -d"
echo ""
echo "📖 See release/docker/README.md for detailed instructions"