using System.IO.Ports;

namespace ModbusMonitor.Models
{
    public enum ModbusProtocol
    {
        Tcp,
        RtuOverTcp,
        SerialRtu,
        SerialAscii
    }

    public class ConnectionConfig
    {
        public ModbusProtocol Protocol { get; set; } = ModbusProtocol.Tcp;
        public int Timeout { get; set; } = 2000;

        // TCP Settings
        public string IpAddress { get; set; } = "127.0.0.1";
        public int TcpPort { get; set; } = 502;

        // Serial Settings
        public string ComPort { get; set; } = "COM1";
        public int BaudRate { get; set; } = 9600;
        public int DataBits { get; set; } = 8;
        public Parity Parity { get; set; } = Parity.None;
        public StopBits StopBits { get; set; } = StopBits.One;

        public override string ToString()
        {
            if (Protocol == ModbusProtocol.Tcp || Protocol == ModbusProtocol.RtuOverTcp)
            {
                return $"{Protocol}: {IpAddress}:{TcpPort}";
            }
            else
            {
                return $"{Protocol}: {ComPort} ({BaudRate}, {Parity}, {StopBits})";
            }
        }
    }
}
