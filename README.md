# Modbus Monitor

📥 **[Download Latest Version (Installer)](https://github.com/ismaillowkey/ModbusMonitor/releases)**

A modern, WPF-based Modbus TCP and Serial Client for monitoring and writing to Modbus devices.

## Features
- **Multiple Protocols**: Supports Modbus TCP, Modbus RTU over TCP, Modbus RTU (Serial), and Modbus ASCII (Serial).
- **Comprehensive Data Types**: Supports 16-bit, 32-bit, and 64-bit data types including Int, UInt, Float, and Double.
- **Endianness Configuration**: Easily swap byte orders (Big Endian, Little Endian, Byte Swap, Word Swap).
- **Dynamic Addressing**: Base address configuration (Start from 0 or Start from 1).
- **Interactive UI**: Clean, modern interface with interactive toggle switches for Coils and real-time polling.

## How to Run Locally
1. Ensure you have the .NET 10.0 SDK installed.
2. Open the solution in Visual Studio or run from the command line:
   ```bash
   cd ModbusMonitor
   dotnet run
   ```

## How to Build the Installer
This project uses NSIS (Nullsoft Scriptable Install System) to generate a professional setup executable for both **x86** and **x64** architectures.

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download)
- [NSIS](https://nsis.sourceforge.io/Download) — Install NSIS terlebih dahulu sebelum menjalankan skrip build.

### Build & Package (Otomatis)
Jalankan satu skrip untuk publish + compile installer sekaligus:
```bat
cd ModbusMonitor
build_release.bat
```

Skrip ini akan:
1. `dotnet publish` untuk **win-x86** → `bin\Release\net10.0-windows\win-x86`
2. `dotnet publish` untuk **win-x64** → `bin\Release\net10.0-windows\win-x64`
3. Compile installer NSIS untuk **x86** → `setup_Modbus Monitor_v0.6.0_x86.exe`
4. Compile installer NSIS untuk **x64** → `setup_Modbus Monitor_v0.6.0_x64.exe`

### Manual (Opsional)
Jika ingin compile installer secara manual:
```bat
"C:\Program Files (x86)\NSIS\makensis.exe" /DARCH=x86 installer.nsi
"C:\Program Files (x86)\NSIS\makensis.exe" /DARCH=x64 installer.nsi
```

## Publisher
Developed by **Ismail Lowkey**

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
