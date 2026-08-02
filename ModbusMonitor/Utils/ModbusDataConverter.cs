using System;
using System.Linq;

namespace ModbusMonitor.Utils
{
    public static class ModbusDataConverter
    {
        public static string ConvertRegistersToString(ushort[] registers, string dataType, string endian)
        {
            if (registers == null || registers.Length == 0) return "0";

            if (dataType.Contains("UInt16"))
            {
                return registers[0].ToString();
            }
            else if (dataType.Contains("Int16"))
            {
                return ((short)registers[0]).ToString();
            }

            if (dataType.Contains("64-bit"))
            {
                if (registers.Length < 4) return "0";
                byte[] bytes = new byte[8];
                byte[] reg0Bytes = BitConverter.GetBytes(registers[0]);
                byte[] reg1Bytes = BitConverter.GetBytes(registers[1]);
                byte[] reg2Bytes = BitConverter.GetBytes(registers[2]);
                byte[] reg3Bytes = BitConverter.GetBytes(registers[3]);

                if (endian.Contains("ABCD")) // Big Endian (ABCDEFGH)
                {
                    bytes[0] = reg3Bytes[1]; bytes[1] = reg3Bytes[0]; // A B
                    bytes[2] = reg2Bytes[1]; bytes[3] = reg2Bytes[0]; // C D
                    bytes[4] = reg1Bytes[1]; bytes[5] = reg1Bytes[0]; // E F
                    bytes[6] = reg0Bytes[1]; bytes[7] = reg0Bytes[0]; // G H
                }
                else if (endian.Contains("DCBA")) // Little Endian (HGFEDCBA)
                {
                    bytes[0] = reg0Bytes[0]; bytes[1] = reg0Bytes[1]; // H G
                    bytes[2] = reg1Bytes[0]; bytes[3] = reg1Bytes[1]; // F E
                    bytes[4] = reg2Bytes[0]; bytes[5] = reg2Bytes[1]; // D C
                    bytes[6] = reg3Bytes[0]; bytes[7] = reg3Bytes[1]; // B A
                }
                else if (endian.Contains("BADC")) // Byte Swap (BADCFEHG)
                {
                    bytes[0] = reg3Bytes[0]; bytes[1] = reg3Bytes[1];
                    bytes[2] = reg2Bytes[0]; bytes[3] = reg2Bytes[1];
                    bytes[4] = reg1Bytes[0]; bytes[5] = reg1Bytes[1];
                    bytes[6] = reg0Bytes[0]; bytes[7] = reg0Bytes[1];
                }
                else if (endian.Contains("CDAB")) // Word Swap (GHEFCDAB)
                {
                    bytes[0] = reg0Bytes[1]; bytes[1] = reg0Bytes[0];
                    bytes[2] = reg1Bytes[1]; bytes[3] = reg1Bytes[0];
                    bytes[4] = reg2Bytes[1]; bytes[5] = reg2Bytes[0];
                    bytes[6] = reg3Bytes[1]; bytes[7] = reg3Bytes[0];
                }
                else
                {
                    bytes[0] = reg3Bytes[1]; bytes[1] = reg3Bytes[0];
                    bytes[2] = reg2Bytes[1]; bytes[3] = reg2Bytes[0];
                    bytes[4] = reg1Bytes[1]; bytes[5] = reg1Bytes[0];
                    bytes[6] = reg0Bytes[1]; bytes[7] = reg0Bytes[0];
                }

                if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

                if (dataType.Contains("UInt64")) return BitConverter.ToUInt64(bytes, 0).ToString();
                else if (dataType.Contains("Int64")) return BitConverter.ToInt64(bytes, 0).ToString();
                else if (dataType.Contains("Double")) return BitConverter.ToDouble(bytes, 0).ToString("F4");
            }
            
            if (registers.Length < 2) return "0";

            byte[] b32 = new byte[4];
            byte[] r0b = BitConverter.GetBytes(registers[0]);
            byte[] r1b = BitConverter.GetBytes(registers[1]);

            if (endian.Contains("ABCD")) // Big Endian
            {
                b32[0] = r1b[1]; b32[1] = r1b[0];
                b32[2] = r0b[1]; b32[3] = r0b[0];
            }
            else if (endian.Contains("DCBA")) // Little Endian
            {
                b32[0] = r0b[0]; b32[1] = r0b[1];
                b32[2] = r1b[0]; b32[3] = r1b[1];
            }
            else if (endian.Contains("BADC")) // Byte Swap
            {
                b32[0] = r1b[0]; b32[1] = r1b[1];
                b32[2] = r0b[0]; b32[3] = r0b[1];
            }
            else if (endian.Contains("CDAB")) // Word Swap
            {
                b32[0] = r0b[1]; b32[1] = r0b[0];
                b32[2] = r1b[1]; b32[3] = r1b[0];
            }
            else
            {
                b32[0] = r1b[1]; b32[1] = r1b[0];
                b32[2] = r0b[1]; b32[3] = r0b[0];
            }

            if (BitConverter.IsLittleEndian) Array.Reverse(b32);

            if (dataType.Contains("UInt32")) return BitConverter.ToUInt32(b32, 0).ToString();
            else if (dataType.Contains("Int32")) return BitConverter.ToInt32(b32, 0).ToString();
            else if (dataType.Contains("Float32")) return BitConverter.ToSingle(b32, 0).ToString("F4");

            return registers[0].ToString();
        }

        public static ushort[] ConvertStringToRegisters(string value, string dataType, string endian)
        {
            if (dataType.Contains("UInt16"))
            {
                if (ushort.TryParse(value, out ushort u16)) return new ushort[] { u16 };
                throw new ArgumentException("Invalid UInt16");
            }
            else if (dataType.Contains("Int16"))
            {
                if (short.TryParse(value, out short i16)) return new ushort[] { (ushort)i16 };
                throw new ArgumentException("Invalid Int16");
            }

            if (dataType.Contains("64-bit"))
            {
                byte[] bytes;
                if (dataType.Contains("UInt64"))
                {
                    if (!ulong.TryParse(value, out ulong u64)) throw new ArgumentException("Invalid UInt64");
                    bytes = BitConverter.GetBytes(u64);
                }
                else if (dataType.Contains("Int64"))
                {
                    if (!long.TryParse(value, out long i64)) throw new ArgumentException("Invalid Int64");
                    bytes = BitConverter.GetBytes(i64);
                }
                else if (dataType.Contains("Double"))
                {
                    if (!double.TryParse(value, out double d64)) throw new ArgumentException("Invalid Double");
                    bytes = BitConverter.GetBytes(d64);
                }
                else
                {
                    throw new ArgumentException("Unsupported 64-bit DataType");
                }

                if (BitConverter.IsLittleEndian) Array.Reverse(bytes);

                byte[] r0b = new byte[2];
                byte[] r1b = new byte[2];
                byte[] r2b = new byte[2];
                byte[] r3b = new byte[2];

                if (endian.Contains("ABCD")) // ABCDEFGH
                {
                    r3b[1] = bytes[0]; r3b[0] = bytes[1];
                    r2b[1] = bytes[2]; r2b[0] = bytes[3];
                    r1b[1] = bytes[4]; r1b[0] = bytes[5];
                    r0b[1] = bytes[6]; r0b[0] = bytes[7];
                }
                else if (endian.Contains("DCBA")) // HGFEDCBA
                {
                    r0b[0] = bytes[0]; r0b[1] = bytes[1];
                    r1b[0] = bytes[2]; r1b[1] = bytes[3];
                    r2b[0] = bytes[4]; r2b[1] = bytes[5];
                    r3b[0] = bytes[6]; r3b[1] = bytes[7];
                }
                else if (endian.Contains("BADC")) // BADCFEHG
                {
                    r3b[0] = bytes[0]; r3b[1] = bytes[1];
                    r2b[0] = bytes[2]; r2b[1] = bytes[3];
                    r1b[0] = bytes[4]; r1b[1] = bytes[5];
                    r0b[0] = bytes[6]; r0b[1] = bytes[7];
                }
                else if (endian.Contains("CDAB")) // GHEFCDAB
                {
                    r0b[1] = bytes[0]; r0b[0] = bytes[1];
                    r1b[1] = bytes[2]; r1b[0] = bytes[3];
                    r2b[1] = bytes[4]; r2b[0] = bytes[5];
                    r3b[1] = bytes[6]; r3b[0] = bytes[7];
                }
                else
                {
                    r3b[1] = bytes[0]; r3b[0] = bytes[1];
                    r2b[1] = bytes[2]; r2b[0] = bytes[3];
                    r1b[1] = bytes[4]; r1b[0] = bytes[5];
                    r0b[1] = bytes[6]; r0b[0] = bytes[7];
                }

                return new ushort[] { BitConverter.ToUInt16(r0b, 0), BitConverter.ToUInt16(r1b, 0), BitConverter.ToUInt16(r2b, 0), BitConverter.ToUInt16(r3b, 0) };
            }

            byte[] b32;
            if (dataType.Contains("UInt32"))
            {
                if (!uint.TryParse(value, out uint u32)) throw new ArgumentException("Invalid UInt32");
                b32 = BitConverter.GetBytes(u32);
            }
            else if (dataType.Contains("Int32"))
            {
                if (!int.TryParse(value, out int i32)) throw new ArgumentException("Invalid Int32");
                b32 = BitConverter.GetBytes(i32);
            }
            else if (dataType.Contains("Float32"))
            {
                if (!float.TryParse(value, out float f32)) throw new ArgumentException("Invalid Float32");
                b32 = BitConverter.GetBytes(f32);
            }
            else
            {
                throw new ArgumentException("Unsupported DataType");
            }

            if (BitConverter.IsLittleEndian) Array.Reverse(b32);

            byte[] r0 = new byte[2];
            byte[] r1 = new byte[2];

            if (endian.Contains("ABCD"))
            {
                r1[1] = b32[0]; r1[0] = b32[1];
                r0[1] = b32[2]; r0[0] = b32[3];
            }
            else if (endian.Contains("DCBA"))
            {
                r0[0] = b32[0]; r0[1] = b32[1];
                r1[0] = b32[2]; r1[1] = b32[3];
            }
            else if (endian.Contains("BADC"))
            {
                r1[0] = b32[0]; r1[1] = b32[1];
                r0[0] = b32[2]; r0[1] = b32[3];
            }
            else if (endian.Contains("CDAB"))
            {
                r0[1] = b32[0]; r0[0] = b32[1];
                r1[1] = b32[2]; r1[0] = b32[3];
            }
            else
            {
                r1[1] = b32[0]; r1[0] = b32[1];
                r0[1] = b32[2]; r0[0] = b32[3];
            }

            return new ushort[] { BitConverter.ToUInt16(r0, 0), BitConverter.ToUInt16(r1, 0) };
        }
    }
}
