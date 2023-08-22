using System;
using System.IO;

namespace LCLib.Asn1Processor
{
	internal class Asn1Util
	{
		public static int BytePrecision(ulong value)
		{
			int num = 8;
			while (num > 0 && value >> (num - 1) * 8 == 0)
			{
				num--;
			}
			return num;
		}

		public static int DERLengthEncode(Stream xdata, ulong length)
		{
			int num = 0;
			if (length <= 127)
			{
				xdata.WriteByte((byte)length);
				num++;
			}
			else
			{
				xdata.WriteByte((byte)((uint)BytePrecision(length) | 0x80u));
				num++;
				for (int num2 = BytePrecision(length); num2 > 0; num2--)
				{
					xdata.WriteByte((byte)(length >> (num2 - 1) * 8));
					num++;
				}
			}
			return num;
		}

		public static long DerLengthDecode(Stream bt)
		{
			long num = 0L;
			byte b = (byte)bt.ReadByte();
			if ((b & 0x80) == 0)
			{
				num = b;
			}
			else
			{
				long num2 = b & 0x7F;
				if (num2 == 0)
				{
					throw new Exception("Indefinite length.");
				}
				num = 0L;
				while (num2-- > 0)
				{
					if (num >> 56 > 0)
					{
						throw new Exception("Length overflow.");
					}
					b = (byte)bt.ReadByte();
					num = (num << 8) | b;
				}
			}
			return num;
		}

		private Asn1Util()
		{
		}
	}
}
