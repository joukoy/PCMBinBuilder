using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Crc16
{
    const ushort polynomial = 0xA001;
    ushort[] table = new ushort[256];

    public ushort ComputeChecksum(byte[] bytes)
    {
        ushort crc = 0;
        for (int i = 0; i < bytes.Length; ++i)
        {
            byte index = (byte)(crc ^ bytes[i]);
            crc = (ushort)((crc >> 8) ^ table[index]);
        }
        return crc;
    }

    public byte[] ComputeChecksumBytes(byte[] bytes)
    {
        ushort crc = ComputeChecksum(bytes);
        return BitConverter.GetBytes(crc);
    }

    public Crc16()
    {
        ushort value;
        ushort temp;
        for (ushort i = 0; i < table.Length; ++i)
        {
            value = 0;
            temp = i;
            for (byte j = 0; j < 8; ++j)
            {
                if (((value ^ temp) & 0x0001) != 0)
                {
                    value = (ushort)((value >> 1) ^ polynomial);
                }
                else
                {
                    value >>= 1;
                }
                temp >>= 1;
            }
            table[i] = value;
        }
    }
}
public class Crc
{
    private static UInt32[] crcTable;
    private const int WIDTH = 8 * 4;
    private const UInt32 TOPBIT = 0x80000000;
    private const UInt32 POLYNOMIAL = 0x04C11DB7;

    public Crc()
    {
        if (crcTable == null)
        {
            crcTable = new UInt32[256];
            UInt32 remainder;

            /*
                * Compute the remainder of each possible dividend.
                */
            for (int dividend = 0; dividend < 256; ++dividend)
            {
                /*
                    * Start with the dividend followed by zeros.
                    */
                remainder = (UInt32)(dividend << (WIDTH - 8));

                /*
                    * Perform modulo-2 division, a bit at a time.
                    */
                for (int bit = 8; bit > 0; --bit)
                {
                    /*
                        * Try to divide the current data bit.
                        */
                    if ((remainder & TOPBIT) != 0)
                    {
                        remainder = (remainder << 1) ^ POLYNOMIAL;
                    }
                    else
                    {
                        remainder = (remainder << 1);
                    }
                }

                /*
                    * Store the result into the table.
                    */
                crcTable[dividend] = remainder;
            }
        }
    }

    public UInt32 GetCrc(byte[] buffer, UInt32 start, UInt32 length)
    {
        byte data;
        UInt32 remainder = 0;

        for (UInt32 index = start; index < start + length; index++)
        {
            /*
                * Divide the message by the polynomial, a byte at a time.
                */
            data = (byte)(buffer[index] ^ (remainder >> (WIDTH - 8)));
            remainder = crcTable[data] ^ (remainder << 8);
        }

        /*
            * The final remainder is the CRC.
            */
        return (remainder);
    }
}
