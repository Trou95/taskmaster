# 🎬 Taskmaster Presentation Script

## 📋 Sunum Öncesi Hazırlık

```bash
# 1. Config dosyasını hazırla
cp presentation.config.json task.config.json

# 2. Output dosyalarını temizle  
rm -f /tmp/*.stdout /tmp/*.stderr

# 3. Log dosyasını temizle
rm -f taskmaster.log

# 4. Projeyi build et
dotnet build
```

---

## 🎯 Demo 1: Program Başlatma ve Autostart

### **Gösterilecek Özellik:** Autostart functionality

```bash
# Programı başlat
dotnet run

# Beklenen çıktı: Autostart containerlar başlamalı
```

**📢 Anlatım:**
> "Taskmaster başlatıldığında, `StartAtLaunch: true` olan containerlar otomatik başlar. Şu anda nginx-demo, echo-service ve multi-process başladı."

```bash
# Status kontrolü
> /status

# Beklenen: 4 container running, 2 container waiting
```

**📊 Gösterilecek:**
- nginx-demo: Running (autostart)
- echo-service: Running veya Waiting (hemen çıktığı için)  
- multi-process: Running (3 process)
- worker-always: Waiting (manual start)
- long-runner: Waiting (manual start)
- failing-service: Waiting (manual start)

---

## 🎯 Demo 2: Shell Commands

### **Gösterilecek Özellik:** Complete shell interface

```bash
# Help komutu
> /help

# Manual start
> /start worker-always
> /start long-runner

# Status check
> /status

# Stop testi
> /stop nginx-demo

# Restart testi
> /restart nginx-demo
```

**📢 Anlatım:**
> "Shell interface tüm gerekli komutları destekler. ReadLine ile history, autocompletion ve line editing var."

---

## 🎯 Demo 3: Restart Policies

### **Gösterilecek Özellik:** Different restart behaviors

```bash
# Terminal'de process ID'leri bul
ps aux | grep sleep

# worker-always process'ini kill et (Always policy)
kill -9 <worker-always-pid>

# nginx-demo process'ini kill et (OnFailure policy)  
kill -9 <nginx-demo-pid>
```

**📢 Anlatım:**
> "Restart policy'lere göre farklı davranışlar gösteriyoruz:"
> - Always: Her çıkışta restart
> - OnFailure: Sadece beklenmeyen çıkışlarda restart
> - Never: Hiç restart etmez

```bash
# Status kontrol et
> /status

# Log dosyasını kontrol et
cat taskmaster.log | tail -10
```

---

## 🎯 Demo 4: Configuration Reload

### **Gösterilecek Özellik:** Smart configuration reload

```bash
# Manuel reload
> /reloadconfig
```

**📢 Anlatım:**
> "Configuration reload sırasında değişmeyen processler durmaz, sadece değişenler restart eder."

```bash
# Başka terminal'de SIGHUP gönder
ps aux | grep Taskmaster
kill -HUP <taskmaster-pid>
```

**📢 Anlatım:**
> "SIGHUP sinyali ile de configuration reload edilebilir. Bu subject requirement'ı."

---

## 🎯 Demo 5: Output Redirection

### **Gösterilecek Özellik:** Stdout/stderr redirection

```bash
# Output dosyalarını listele
ls -la /tmp/*.stdout /tmp/*.stderr

# Echo service output'unu kontrol et  
cat /tmp/echo-service.stdout

# Diğer output'ları göster
head /tmp/nginx-demo.stdout
head /tmp/multi-process.stdout
```

**📢 Anlatım:**
> "Her container'ın stdout/stderr'ı ayrı dosyalara real-time yazılıyor. LogOutput özelliği ile kontrol edilebilir."

---

## 🎯 Demo 6: Multi-Process Management

### **Gösterilecek Özellik:** Multiple processes per container

```bash
# Multi-process container'ı kontrol et
> /status

# Process'leri sistem seviyesinde göster
ps aux | grep sleep | grep -v grep
```

**📢 Anlatım:**
> "multi-process container'ı 3 adet process çalıştırıyor. NumberOfProcesses özelliği ile kontrol edilir."

```bash
# Bir process'i kill et
kill -9 <one-of-multi-process-pids>

# Status kontrol et - kaç tane restart ettiğini göster
> /status
```

---

## 🎯 Demo 7: Failure Handling

### **Gösterilecek Özellik:** Restart attempts and failure logging

```bash
# Failing service'i başlat  
> /start failing-service

# Başarısızlığa zorla (bash script ile exit 1)
# Bu container bash çalıştırdığı için hemen çıkacak

# Log dosyasını kontrol et
tail -f taskmaster.log
```

**📢 Anlatım:**
> "MaxRestartAttempts ile restart limitini kontrol ediyoruz. 2 denemeden sonra durur ve unexpected death loglanır."

---

## 🎯 Demo 8: Comprehensive Logging

### **Gösterilecek Özellik:** Complete event logging

```bash
# Log dosyasının tamamını göster
cat taskmaster.log

# Specific event'leri grep'le
grep "Container.*starting" taskmaster.log
grep "Configuration reload" taskmaster.log  
grep "died unexpectedly" taskmaster.log
grep "successfully started" taskmaster.log
```

**📢 Anlatım:**
> "Tüm subject requirement'larındaki eventler loglanıyor:"
> - Container starting/stopping  
> - Configuration reload
> - Unexpected deaths
> - Successful starts
> - Restart attempts

---

## 🎯 Demo 9: Environment & Working Directory

### **Gösterilecek Özellik:** Process environment setup

```bash
# Process environment'ını kontrol et (eğer mümkünse)
# Bu demo için environment variable'ları configuration'da göster

cat task.config.json | grep -A5 -B5 "EnvironmentVariables"
```

**📢 Anlatım:**
> "Her container farklı environment variable'lar, working directory ve umask ayarları ile çalışır."

---

## 🎯 Demo 10: Graceful Shutdown

### **Gösterilecek Özellik:** Proper program termination

```bash
# Programı graceful şekilde kapat
> /quit
```

**📢 Anlatım:**
> "Quit komutu ile program graceful olarak kapanır ve tüm child processler temizlenir."

```bash
# Hiç process kalmadığını kontrol et
ps aux | grep sleep | grep -v grep
```

---

## 📊 Subject Compliance Checklist

### ✅ **Mandatory Features Demonstrated**

1. **Job Control Daemon** ✅
   - Foreground operation
   - Control shell interface
   - Process management

2. **Configuration System** ✅  
   - JSON configuration loading
   - SIGHUP reload capability
   - Smart reload (unchanged processes preserved)

3. **Process Management** ✅
   - Child process spawning
   - Accurate process monitoring  
   - Restart policies (Always/Never/OnFailure)

4. **Logging System** ✅
   - All required events logged
   - File-based logging
   - Real-time event tracking

5. **Shell Commands** ✅
   - status, start, stop, restart
   - Configuration reload  
   - Program termination

### ✅ **Configuration Properties Demonstrated**

- ✅ Command specification
- ✅ Number of processes (1-3)
- ✅ Autostart functionality  
- ✅ Restart policies (all 3 types)
- ✅ Expected exit codes
- ✅ Runtime validation (starttime)
- ✅ Restart attempt limits
- ✅ Stop signal handling
- ✅ Kill timeout
- ✅ Output redirection
- ✅ Environment variables
- ✅ Working directory
- ✅ Umask settings

### ✅ **Advanced Features Demonstrated**

- ✅ Real-time process monitoring
- ✅ Smart configuration reload
- ✅ Comprehensive logging
- ✅ Signal handling (SIGHUP/SIGINT)
- ✅ Output redirection to files
- ✅ Multi-process containers
- ✅ Failure handling and retry logic

---

## 🏆 Sunum Sonrası

**"Taskmaster subject'teki tüm mandatory gereksinimleri karşılıyor:"**

1. ✅ **20/20 required features implemented**
2. ✅ **All shell commands working**  
3. ✅ **Complete configuration support**
4. ✅ **Proper logging and monitoring**
5. ✅ **Ready for any defense test scenario**

**Defense session için %100 hazır! 🎯**