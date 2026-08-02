@echo off
rem ------------------------------------------------------------
rem Build + Package ModbusMonitor (x86 & x64)
rem ------------------------------------------------------------

rem ---- Publish 32-bit (x86) ----
echo [1/4] Publishing x86...
dotnet publish -c Release -r win-x86 -o bin\Release\net10.0-windows\win-x86
if %errorlevel% neq 0 (
    echo ERROR: dotnet publish x86 failed.
    pause
    exit /b %errorlevel%
)

rem ---- Publish 64-bit (x64) ----
echo [2/4] Publishing x64...
dotnet publish -c Release -r win-x64 -o bin\Release\net10.0-windows\win-x64
if %errorlevel% neq 0 (
    echo ERROR: dotnet publish x64 failed.
    pause
    exit /b %errorlevel%
)

rem ---- Check NSIS ----
set MAKENSIS="C:\Program Files (x86)\NSIS\makensis.exe"
if not exist %MAKENSIS% (
    echo ERROR: makensis.exe tidak ditemukan di %MAKENSIS%
    pause
    exit /b 1
)

rem ---- Build installer x86 ----
echo [3/4] Building installer x86...
%MAKENSIS% /DARCH=x86 installer.nsi
if %errorlevel% neq 0 (
    echo ERROR: NSIS compile x86 failed.
    pause
    exit /b %errorlevel%
)

rem ---- Build installer x64 ----
echo [4/4] Building installer x64...
%MAKENSIS% /DARCH=x64 installer.nsi
if %errorlevel% neq 0 (
    echo ERROR: NSIS compile x64 failed.
    pause
    exit /b %errorlevel%
)

echo.
echo ============================================
echo  DONE! Kedua installer berhasil dibuat.
echo ============================================
pause
