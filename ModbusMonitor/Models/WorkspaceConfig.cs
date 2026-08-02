using System.Collections.Generic;

namespace ModbusMonitor.Models
{
    public class RegisterItemDto
    {
        public string Name { get; set; } = string.Empty;
        public byte SlaveId { get; set; } = 1;
        public int Address { get; set; } = 0;
        public string DataType { get; set; } = "UInt16 (16-bit)";
        public string Endian { get; set; } = "ABCD (Big Endian)";
        public int Length { get; set; } = 1;
    }

    public class WorkspaceConfig
    {
        public List<RegisterItemDto> Coils { get; set; } = new();
        public List<RegisterItemDto> DiscreteInputs { get; set; } = new();
        public List<RegisterItemDto> InputRegisters { get; set; } = new();
        public List<RegisterItemDto> HoldingRegisters { get; set; } = new();
    }
}
