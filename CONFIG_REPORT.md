# Taskmaster Presentation Configuration Report

## 🎯 Configuration Overview

Bu configuration dosyası (`presentation.config.json`) Taskmaster'ın tüm özelliklerini test etmek ve sunumda göstermek için tasarlanmıştır. Subject'teki tüm gereksinimleri kapsar.

---

## 📊 Container Detayları

### 1. **nginx-demo** - Autostart & OnFailure Demo
```json
{
  "Name": "nginx-demo",
  "Command": "/nginx-demo",
  "BinaryPath": "/usr/bin/sleep",
  "StartAtLaunch": true,
  "RestartPolicy": "OnFailure"
}
```

**🎯 Test Amacı:**
- **Autostart özelliği** - Program başladığında otomatik çalışır
- **OnFailure restart policy** - Sadece beklenmeyen çıkışlarda restart
- **Multiple exit codes** - 0 ve 2 exit kodları kabul edilir
- **Environment variables** - STARTED_BY, ANSWER, SERVICE_NAME

**📋 Test Senaryosu:**
1. Program başlatıldığında otomatik başlamalı
2. Manual olarak kill edildiğinde restart etmeli
3. Exit code 0 veya 2 ile çıkarsa restart etmemeli

---

### 2. **worker-always** - Always Restart & Multi-Process
```json
{
  "Name": "worker-always", 
  "NumberOfProcesses": 2,
  "StartAtLaunch": false,
  "RestartPolicy": "Always"
}
```

**🎯 Test Amacı:**
- **Always restart policy** - Her çıkışta restart eder
- **Multiple processes** - 2 process aynı anda çalışır
- **Manual start** - Autostart kapalı, manuel başlatılır
- **Different umask** - 077 umask ayarı

**📋 Test Senaryosu:**
1. Manuel başlatılmalı (`/start worker-always`)
2. Process kill edildiğinde hemen restart etmeli
3. 2 adet process paralel çalışmalı

---

### 3. **echo-service** - Never Restart & Quick Exit
```json
{
  "Name": "echo-service",
  "BinaryPath": "/bin/echo", 
  "RestartPolicy": "Never",
  "ExpectedRunTime": 1
}
```

**🎯 Test Amacı:**
- **Never restart policy** - Hiçbir zaman restart etmez
- **Quick exit process** - Echo komutu hemen çıkar
- **Short runtime** - 1 saniye beklenen çalışma süresi
- **Output capture** - Echo output'u dosyaya yazılır

**📋 Test Senaryosu:**
1. Başlatıldığında hemen çıkmalı
2. Restart etmemeli
3. Output dosyasında "Hello from Taskmaster!" görülmeli

---

### 4. **long-runner** - Long Running Process
```json
{
  "Name": "long-runner",
  "ExpectedRunTime": 5,
  "KillTimeout": 10000
}
```

**🎯 Test Amacı:**
- **Long running process** - 30 saniye sleep
- **Starttime validation** - 5 saniye minimum çalışma süresi
- **Graceful shutdown** - 10 saniye kill timeout
- **Manual control** - Start/stop/restart komutları

**📋 Test Senaryosu:**
1. Manual başlatılmalı
2. 5 saniye sonra "successfully started" log'u gelmeli
3. Stop komutu ile graceful shutdown test edilmeli

---

### 5. **failing-service** - Failure Testing
```json
{
  "Name": "failing-service",
  "MaxRestartAttempts": 2,
  "RestartPolicy": "OnFailure"
}
```

**🎯 Test Amacı:**
- **Restart limits** - Maximum 2 restart denemesi
- **Failure handling** - Başarısız processler için retry logic
- **Unexpected death logging** - Beklenmeyen çıkışları loglar
- **Error scenarios** - Hata durumları testi

**📋 Test Senaryosu:**
1. Process başarısız olduğunda 2 kez restart denemeli
2. 2 denemeden sonra durdurmalı
3. Her başarısızlık loglanmalı

---

### 6. **multi-process** - Scale Testing
```json
{
  "Name": "multi-process",
  "NumberOfProcesses": 3,
  "StartAtLaunch": true
}
```

**🎯 Test Amacı:**
- **Process scaling** - 3 adet process paralel
- **Load testing** - Çoklu process yönetimi
- **Status monitoring** - Tüm processler için status
- **Bulk operations** - Toplu start/stop

**📋 Test Senaryosu:**
1. 3 process otomatik başlamalı
2. Status komutunda 3 process görünmeli
3. Stop komutu tüm processler durdurmalı

---

## 🧪 Test Matrisi

| Container | Autostart | Processes | Restart Policy | Test Focus |
|-----------|-----------|-----------|----------------|------------|
| **nginx-demo** | ✅ Yes | 1 | OnFailure | Autostart, Multiple exit codes |
| **worker-always** | ❌ No | 2 | Always | Multi-process, Always restart |
| **echo-service** | ✅ Yes | 1 | Never | Quick exit, No restart |
| **long-runner** | ❌ No | 1 | OnFailure | Long running, Graceful stop |
| **failing-service** | ❌ No | 1 | OnFailure | Failure handling, Retry limits |
| **multi-process** | ✅ Yes | 3 | Always | Scaling, Bulk operations |

---

## 🎬 Sunum Test Senaryoları

### **Senaryo 1: Program Başlatma**
```bash
# Config dosyasını kopyala
cp presentation.config.json task.config.json

# Programı başlat  
dotnet run

# Beklenen: nginx-demo, echo-service, multi-process otomatik başlamalı
> /status
```

### **Senaryo 2: Manuel Container Yönetimi**
```bash
# Manual start
> /start worker-always
> /start long-runner

# Status kontrolü
> /status

# Stop testi
> /stop long-runner

# Restart testi  
> /restart nginx-demo
```

### **Senaryo 3: Restart Policy Testi**
```bash
# Terminal'de process ID bul ve kill et
ps aux | grep sleep
kill -9 <pid>

# Beklenen: 
# - worker-always: Hemen restart (Always policy)
# - nginx-demo: Restart (OnFailure policy)  
# - echo-service: Restart etmez (Never policy)
```

### **Senaryo 4: Configuration Reload**
```bash
# Manual reload
> /reloadconfig

# SIGHUP ile reload (başka terminal)
kill -HUP <taskmaster_pid>

# Log kontrolü
cat taskmaster.log
```

### **Senaryo 5: Output Redirection Testi**
```bash
# Output dosyalarını kontrol et
ls -la /tmp/*.stdout /tmp/*.stderr
cat /tmp/echo-service.stdout
cat /tmp/nginx-demo.stdout
```

### **Senaryo 6: Graceful Shutdown**
```bash
# Program çıkışı
> /quit

# Beklenen: Tüm processler temiz bir şekilde durdurulmalı
```

---

## 📝 Subject Requirement Coverage

### ✅ **Shell Commands** (6/6)
- `/status` - Container durumları
- `/start <name>` - Manuel başlatma  
- `/stop <name>` - Manuel durdurma
- `/restart <name>` - Yeniden başlatma
- `/reloadconfig` - Config yenileme
- `/quit` - Program çıkışı

### ✅ **Configuration Properties** (12/12)
- **Command/BinaryPath** - Process komutları
- **NumberOfProcesses** - 1-3 arası değişken
- **StartAtLaunch** - True/False kombinasyonu
- **RestartPolicy** - Always/Never/OnFailure
- **ExpectedExitCodes** - Tek ve çoklu değerler  
- **ExpectedRunTime** - 1-5 saniye aralığı
- **MaxRestartAttempts** - 0-5 aralığı
- **StopSignal** - SIGTERM (15)
- **KillTimeout** - 1-10 saniye
- **StdOut/StdErr** - Tüm containerlar için
- **EnvironmentVariables** - Her container farklı
- **WorkingDirectory/Umask** - Çeşitli ayarlar

### ✅ **Advanced Features** (5/5)
- **Process monitoring** - Real-time status
- **Smart config reload** - Değişen containerlar restart
- **Output redirection** - File'lara gerçek zamanlı yazma
- **Comprehensive logging** - Tüm eventler loglanır
- **Signal handling** - SIGHUP ve SIGINT

---

## 🏆 Sunum Sonucu

Bu configuration ile Taskmaster'ın **tüm subject gereksinimlerini** karşıladığını gösterebilirsiniz:

1. **Job control daemon** ✅
2. **Process management** ✅  
3. **Configuration system** ✅
4. **Control shell** ✅
5. **Logging system** ✅
6. **All required properties** ✅
7. **All shell commands** ✅

**Defense session için tamamen hazır! 🎯**