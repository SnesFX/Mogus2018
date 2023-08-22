using System;
using System.Collections.Specialized;
using System.IO;

namespace LipingShare.LCLib.Asn1Processor
{
	internal class Oid
	{
		private static StringDictionary oidDictionary = null;

		public string GetOidName(string inOidStr)
		{
			if (oidDictionary == null)
			{
				oidDictionary = new StringDictionary();
			}
			return oidDictionary[inOidStr];
		}

		public byte[] Encode(string oidStr)
		{
			MemoryStream memoryStream = new MemoryStream();
			Encode(memoryStream, oidStr);
			memoryStream.Position = 0L;
			byte[] array = new byte[memoryStream.Length];
			memoryStream.Read(array, 0, array.Length);
			memoryStream.Close();
			return array;
		}

		public string Decode(byte[] data)
		{
			MemoryStream memoryStream = new MemoryStream(data);
			memoryStream.Position = 0L;
			string result = Decode(memoryStream);
			memoryStream.Close();
			return result;
		}

		public virtual void Encode(Stream bt, string oidStr)
		{
			string[] array = oidStr.Split('.');
			if (array.Length < 2)
			{
				throw new Exception("Invalid OID string.");
			}
			ulong[] array2 = new ulong[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToUInt64(array[i]);
			}
			bt.WriteByte((byte)(array2[0] * 40 + array2[1]));
			for (int j = 2; j < array2.Length; j++)
			{
				EncodeValue(bt, array2[j]);
			}
		}

		public virtual string Decode(Stream bt)
		{
			string text = "";
			ulong v = 0uL;
			byte b = (byte)bt.ReadByte();
			text += Convert.ToString((int)b / 40);
			text = text + "." + Convert.ToString((int)b % 40);
			while (bt.Position < bt.Length)
			{
				try
				{
					DecodeValue(bt, ref v);
					text = text + "." + v;
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to decode OID value: " + ex.Message);
				}
			}
			return text;
		}

		protected void EncodeValue(Stream bt, ulong v)
		{
			for (int num = (Asn1Util.BitPrecision(v) - 1) / 7; num > 0; num--)
			{
				bt.WriteByte((byte)(0x80 | ((v >> num * 7) & 0x7F)));
			}
			bt.WriteByte((byte)(v & 0x7F));
		}

		protected int DecodeValue(Stream bt, ref ulong v)
		{
			int num = 0;
			v = 0uL;
			byte b;
			do
			{
				b = (byte)bt.ReadByte();
				num++;
				v <<= 7;
				v += (ulong)(b & 0x7F);
			}
			while ((b & 0x80u) != 0);
			return num;
		}
	}
}
