@echo off
echo ========================================
echo    Jenkins Startup Script for FoodBook
echo ========================================
echo.

REM Kiểm tra xem Jenkins đã chạy chưa
echo [INFO] Checking if Jenkins is already running...
netstat -an | findstr :8080 >nul
if %errorlevel% == 0 (
    echo [SUCCESS] Jenkins is already running on port 8080
    echo [INFO] Access Jenkins at: http://localhost:8080
    goto :end
)

REM Chuyển đến thư mục D:\
echo [INFO] Changing to D:\ directory...
cd /d D:\

REM Kiểm tra file jenkins.war
if not exist "jenkins.war" (
    echo [ERROR] jenkins.war not found in D:\
    echo [INFO] Please ensure jenkins.war is in D:\ directory
    pause
    exit /b 1
)

REM Kiểm tra Java
echo [INFO] Checking Java installation...
java -version >nul 2>&1
if %errorlevel% neq 0 (
    echo [ERROR] Java is not installed or not in PATH
    echo [INFO] Please install Java and try again
    pause
    exit /b 1
)

REM Khởi động Jenkins
echo [INFO] Starting Jenkins server...
echo [INFO] Jenkins will be available at: http://localhost:8080
echo [INFO] Press Ctrl+C to stop Jenkins
echo.

java -jar jenkins.war --httpPort=8080

:end
echo.
echo [INFO] Jenkins startup script completed
pause