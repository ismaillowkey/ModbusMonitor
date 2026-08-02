# Modbus Monitor

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
This project uses NSIS (Nullsoft Scriptable Install System) to generate a professional setup executable.

1. **Publish the application:**
   Open a terminal in the `ModbusMonitor` directory and run:
   ```bash
   dotnet publish -c Release -o publish
   ```
2. **Compile the Installer:**
   - Install NSIS from [https://nsis.sourceforge.io/Download](https://nsis.sourceforge.io/Download).
   - Right-click `installer.nsi` in the `ModbusMonitor` directory and select **"Compile NSIS Script"**.
   - The setup file `setup_modbusMonitor.exe` will be generated in the same directory.

## Publisher
Developed by **Ismail Lowkey**

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
