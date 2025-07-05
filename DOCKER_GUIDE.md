# 🐳 Taskmaster Docker Guide

## 📋 Dosya Açıklaması

### 📁 **Oluşturulan Docker Dosyaları:**

1. **`Dockerfile`** - Multi-stage build ile optimize edilmiş container image
2. **`.dockerignore`** - Docker build context'inden dışlanacak dosyalar
3. **`docker-compose.yml`** - Orkestrasyon ve monitoring ile complete setup
4. **`docker-config.json`** - Docker environment için özel configuration
5. **`DOCKER_GUIDE.md`** - Bu guide dosyası

---

## 🚀 Docker Çalıştırma Talimatları

### **Seçenek 1: Docker Build & Run (Basit)**

```bash
# 1. Image'ı build et
docker build -t taskmaster:latest .

# 2. Container'ı çalıştır
docker run -it --name taskmaster-container taskmaster:latest

# 3. Interactive shell için (başka terminal)
docker exec -it taskmaster-container /bin/bash
```

### **Seçenek 2: Docker Compose (Önerilen)**

```bash
# 1. Tüm servisleri başlat
docker-compose up -d

# 2. Logları takip et
docker-compose logs -f taskmaster

# 3. Container'a bağlan
docker-compose exec taskmaster /bin/bash

# 4. Servisleri durdur
docker-compose down
```

### **Seçenek 3: Custom Config ile**

```bash
# Docker config kullanarak çalıştır
docker run -it \
  -v $(pwd)/docker-config.json:/app/task.config.json \
  -v taskmaster-logs:/var/log/taskmaster \
  taskmaster:latest
```

---

## 🔧 Docker Container Özellikleri

### **Image Detayları:**
- **Base Image**: `mcr.microsoft.com/dotnet/runtime:8.0`
- **Build Image**: `mcr.microsoft.com/dotnet/sdk:8.0`
- **User**: `taskmaster` (non-root güvenlik)
- **Working Directory**: `/app`

### **Installed Packages:**
- `procps` - Process monitoring tools
- `psmisc` - Process utilities (pgrep, pkill)
- `bash` - Shell environment

### **Volumes:**
- `/var/log/taskmaster` - Log dosyaları
- `/tmp/taskmaster` - Geçici dosyalar ve process output
- `/app/task.config.json` - Configuration file

### **Ports:**
- `8080` - Socket communication (eğer kullanılırsa)

### **Environment Variables:**
```bash
DOTNET_ENVIRONMENT=Production
TASKMASTER_CONFIG_PATH=/app/task.config.json
TASKMASTER_LOG_PATH=/var/log/taskmaster/taskmaster.log
```

---

## 🧪 Docker'da Test Senaryoları

### **Test 1: Container Başlatma**
```bash
# Container'ı başlat
docker-compose up -d taskmaster

# Autostart container'ların çalıştığını kontrol et
docker-compose exec taskmaster dotnet Taskmaster.dll
```

### **Test 2: Interactive Shell**
```bash
# Container'a bağlan
docker-compose exec taskmaster /bin/bash

# Taskmaster'ı interactive modda çalıştır
dotnet Taskmaster.dll

# Commands test et
> /status
> /help
> /start docker-date
```

### **Test 3: Signal Testing**
```bash
# Container PID bul
docker-compose exec taskmaster pgrep -f Taskmaster

# SIGHUP gönder (config reload)
docker-compose exec taskmaster kill -HUP <pid>

# Logları kontrol et
docker-compose logs taskmaster
```

### **Test 4: Volume Persistence**
```bash
# Log dosyalarını kontrol et
docker-compose exec taskmaster ls -la /var/log/taskmaster/
docker-compose exec taskmaster cat /var/log/taskmaster/taskmaster.log

# Output dosyalarını kontrol et
docker-compose exec taskmaster ls -la /tmp/taskmaster/
docker-compose exec taskmaster cat /tmp/taskmaster/docker-echo.stdout
```

### **Test 5: Health Check**
```bash
# Container health status
docker-compose ps

# Health check detayları
docker inspect --format='{{.State.Health.Status}}' taskmaster-daemon
```

---

## 📊 Docker Config Özellikleri

`docker-config.json` dosyası Docker environment için optimize edilmiştir:

### **Container 1: docker-echo**
- **Purpose**: Quick exit test, output redirection
- **AutoStart**: Yes
- **Restart**: Never
- **Output**: `/tmp/taskmaster/docker-echo.stdout`

### **Container 2: docker-sleep** 
- **Purpose**: Long running service, always restart
- **AutoStart**: Yes  
- **Restart**: Always
- **Duration**: 10 seconds

### **Container 3: docker-date**
- **Purpose**: Multi-process, manual start
- **AutoStart**: No
- **Restart**: OnFailure
- **Processes**: 2

---

## 🐛 Troubleshooting

### **Problem: Container çalışmıyor**
```bash
# Container status kontrol et
docker-compose ps

# Logları kontrol et
docker-compose logs taskmaster

# Container'a bağlan
docker-compose exec taskmaster /bin/bash
```

### **Problem: Permission errors**
```bash
# User kontrol et
docker-compose exec taskmaster whoami

# File permissions kontrol et
docker-compose exec taskmaster ls -la /app
```

### **Problem: Process monitoring çalışmıyor**
```bash
# Process tools test et
docker-compose exec taskmaster pgrep -f sleep
docker-compose exec taskmaster ps aux
```

### **Problem: Signal handling çalışmıyor**
```bash
# Init system kontrol et (docker-compose.yml'de init: true)
docker-compose exec taskmaster ps -p 1

# Signal test et
docker-compose exec taskmaster kill -l
```

---

## 🚀 Production Deployment

### **Resource Limits**
```yaml
deploy:
  resources:
    limits:
      memory: 512M
      cpus: '1.0'
```

### **Logging Configuration**
```yaml
logging:
  driver: "json-file"
  options:
    max-size: "10m"
    max-file: "3"
```

### **Health Monitoring**
```yaml
healthcheck:
  test: ["CMD", "pgrep", "-f", "Taskmaster"]
  interval: 30s
  timeout: 10s
  retries: 3
```

---

## 📝 Docker Best Practices

### ✅ **Implemented:**
- Multi-stage build (smaller image)
- Non-root user (security)
- Proper signal handling (`init: true`)
- Health checks
- Resource limits
- Log rotation
- Volume persistence

### 🔒 **Security:**
- Non-root execution
- Minimal base image
- No sensitive data in image
- Read-only config mount

### 📈 **Performance:**
- Optimized build context (`.dockerignore`)
- Layer caching
- Resource constraints
- Health monitoring

---

## 🎯 Sonuç

Docker setup'ı ile Taskmaster:
- ✅ **Containerized environment'da çalışır**
- ✅ **Production-ready configuration**
- ✅ **Complete monitoring ve logging**
- ✅ **Subject requirements'ları karşılar**
- ✅ **Easy deployment ve scaling**

**Docker'da çalışan Taskmaster artık production-ready! 🐳**