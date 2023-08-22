using System;
using System.IO;
using Microsoft.Win32;

namespace LipingShare.LCLib.Asn1Processor
{
	internal class Asn1Util
	{
		private static char[] hexDigits = new char[16]
		{
			'0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
			'A', 'B', 'C', 'D', 'E', 'F'
		};

		private const string PemStartStr = "-----BEGIN";

		private const string PemEndStr = "-----END";

		public static bool IsAsn1EncodedHexStr(string dataStr)
		{
			bool result = false;
			try
			{
				byte[] array = HexStrToBytes(dataStr);
				if (array.Length != 0)
				{
					Asn1Node asn1Node = new Asn1Node();
					result = asn1Node.LoadData(array);
				}
			}
			catch
			{
				result = false;
			}
			return result;
		}

		public static string FormatString(string inStr, int lineLen, int groupLen)
		{
			char[] array = new char[inStr.Length * 2];
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < inStr.Length; i++)
			{
				array[num++] = inStr[i];
				num3++;
				num2++;
				if (num3 >= groupLen && groupLen > 0)
				{
					array[num++] = ' ';
					num3 = 0;
				}
				if (num2 >= lineLen)
				{
					array[num++] = '\r';
					array[num++] = '\n';
					num2 = 0;
				}
			}
			string text = new string(array);
			text = text.TrimEnd(default(char));
			text = text.TrimEnd('\n');
			return text.TrimEnd('\r');
		}

		public static string GenStr(int len, char xch)
		{
			char[] array = new char[len];
			for (int i = 0; i < len; i++)
			{
				array[i] = xch;
			}
			return new string(array);
		}

		public static long BytesToLong(byte[] bytes)
		{
			long num = 0L;
			for (int i = 0; i < bytes.Length; i++)
			{
				num = (num << 8) | bytes[i];
			}
			return num;
		}

		public static string BytesToString(byte[] bytes)
		{
			string result = "";
			if (bytes == null || bytes.Length < 1)
			{
				return result;
			}
			char[] array = new char[bytes.Length];
			int i = 0;
			int num = 0;
			for (; i < bytes.Length; i++)
			{
				if (bytes[i] != 0)
				{
					array[num++] = (char)bytes[i];
				}
			}
			result = new string(array);
			return result.TrimEnd(default(char));
		}

		public static byte[] StringToBytes(string msg)
		{
			byte[] array = new byte[msg.Length];
			for (int i = 0; i < msg.Length; i++)
			{
				array[i] = (byte)msg[i];
			}
			return array;
		}

		public static bool IsEqual(byte[] source, byte[] target)
		{
			if (source == null)
			{
				return false;
			}
			if (target == null)
			{
				return false;
			}
			if (source.Length != target.Length)
			{
				return false;
			}
			for (int i = 0; i < source.Length; i++)
			{
				if (source[i] != target[i])
				{
					return false;
				}
			}
			return true;
		}

		public static string ToHexString(byte[] bytes)
		{
			if (bytes == null)
			{
				return "";
			}
			char[] array = new char[bytes.Length * 2];
			for (int i = 0; i < bytes.Length; i++)
			{
				int num = bytes[i];
				array[i * 2] = hexDigits[num >> 4];
				array[i * 2 + 1] = hexDigits[num & 0xF];
			}
			return new string(array);
		}

		public static bool IsValidHexDigits(char ch)
		{
			bool result = false;
			for (int i = 0; i < hexDigits.Length; i++)
			{
				if (hexDigits[i] == ch)
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public static byte GetHexDigitsVal(char ch)
		{
			byte result = 0;
			for (int i = 0; i < hexDigits.Length; i++)
			{
				if (hexDigits[i] == ch)
				{
					result = (byte)i;
					break;
				}
			}
			return result;
		}

		public static byte[] HexStrToBytes(string hexStr)
		{
			hexStr = hexStr.Replace(" ", "");
			hexStr = hexStr.Replace("\r", "");
			hexStr = hexStr.Replace("\n", "");
			hexStr = hexStr.ToUpper();
			if (hexStr.Length % 2 != 0)
			{
				throw new Exception("Invalid Hex string: odd length.");
			}
			for (int i = 0; i < hexStr.Length; i++)
			{
				if (!IsValidHexDigits(hexStr[i]))
				{
					throw new Exception("Invalid Hex string: included invalid character [" + hexStr[i] + "]");
				}
			}
			int num = hexStr.Length / 2;
			byte[] array = new byte[num];
			for (int i = 0; i < num; i++)
			{
				int hexDigitsVal = GetHexDigitsVal(hexStr[i * 2]);
				int hexDigitsVal2 = GetHexDigitsVal(hexStr[i * 2 + 1]);
				int num2 = (hexDigitsVal << 4) | hexDigitsVal2;
				array[i] = (byte)num2;
			}
			return array;
		}

		public static bool IsHexStr(string hexStr)
		{
			byte[] array = null;
			try
			{
				array = HexStrToBytes(hexStr);
			}
			catch
			{
				return false;
			}
			if (array == null || array.Length < 0)
			{
				return false;
			}
			return true;
		}

		public static bool IsPemFormated(string pemStr)
		{
			byte[] array = null;
			try
			{
				array = PemToBytes(pemStr);
			}
			catch
			{
				return false;
			}
			return array.Length != 0;
		}

		public static bool IsPemFormatedFile(string fileName)
		{
			bool flag = false;
			try
			{
				FileStream fileStream = new FileStream(fileName, FileMode.Open);
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				fileStream.Close();
				string pemStr = BytesToString(array);
				return IsPemFormated(pemStr);
			}
			catch
			{
				return false;
			}
		}

		public static Stream PemToStream(string pemStr)
		{
			byte[] buffer = PemToBytes(pemStr);
			MemoryStream memoryStream = new MemoryStream(buffer);
			memoryStream.Position = 0L;
			return memoryStream;
		}

		public static byte[] PemToBytes(string pemStr)
		{
			byte[] array = null;
			string[] array2 = pemStr.Split('\n');
			string text = "";
			bool flag = false;
			bool flag2 = false;
			string text2 = "";
			for (int i = 0; i < array2.Length; i++)
			{
				text2 = array2[i].ToUpper();
				if (text2 == "")
				{
					continue;
				}
				if (text2.Length > "-----BEGIN".Length && !flag && text2.Substring(0, "-----BEGIN".Length) == "-----BEGIN")
				{
					flag = true;
					continue;
				}
				if (text2.Length > "-----END".Length && text2.Substring(0, "-----END".Length) == "-----END")
				{
					flag2 = true;
					break;
				}
				if (flag)
				{
					text += array2[i];
				}
			}
			if (!(flag && flag2))
			{
				throw new Exception("'BEGIN'/'END' line is missing.");
			}
			text = text.Replace("\r", "");
			text = text.Replace("\n", "");
			text = text.Replace("\n", " ");
			return Convert.FromBase64String(text);
		}

		public static string BytesToPem(byte[] data)
		{
			return BytesToPem(data, "");
		}

		public static string GetPemFileHeader(string fileName)
		{
			try
			{
				FileStream fileStream = new FileStream(fileName, FileMode.Open);
				byte[] array = new byte[fileStream.Length];
				fileStream.Read(array, 0, array.Length);
				fileStream.Close();
				string pemStr = BytesToString(array);
				return GetPemHeader(pemStr);
			}
			catch
			{
				return "";
			}
		}

		public static string GetPemHeader(string pemStr)
		{
			string[] array = pemStr.Split('\n');
			bool flag = false;
			string text = "";
			for (int i = 0; i < array.Length; i++)
			{
				text = array[i].ToUpper().Replace("\r", "");
				if (!(text == "") && text.Length > "-----BEGIN".Length && !flag && text.Substring(0, "-----BEGIN".Length) == "-----BEGIN")
				{
					flag = true;
					string text2 = array[i].Substring("-----BEGIN".Length, array[i].Length - "-----BEGIN".Length).Replace("-----", "");
					return text2.Replace("\r", "");
				}
			}
			return "";
		}

		public static string BytesToPem(byte[] data, string pemHeader)
		{
			if (pemHeader == null || pemHeader.Length < 1)
			{
				pemHeader = "ASN.1 Editor Generated PEM File";
			}
			string text = "";
			if (pemHeader.Length > 0 && pemHeader[0] != ' ')
			{
				pemHeader = " " + pemHeader;
			}
			text = Convert.ToBase64String(data);
			text = FormatString(text, 64, 0);
			return "-----BEGIN" + pemHeader + "-----\r\n" + text + "\r\n-----END" + pemHeader + "-----\r\n";
		}

		public static int BitPrecision(ulong ivalue)
		{
			if (ivalue == 0)
			{
				return 0;
			}
			int num = 0;
			int num2 = 32;
			while (num2 - num > 1)
			{
				int num3 = (num + num2) / 2;
				if (ivalue >> num3 != 0)
				{
					num = num3;
				}
				else
				{
					num2 = num3;
				}
			}
			return num2;
		}

		public static int BytePrecision(ulong value)
		{
			int num = 4;
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

		public static long DerLengthDecode(Stream bt, ref bool isIndefiniteLength)
		{
			isIndefiniteLength = false;
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
					isIndefiniteLength = true;
					return -2L;
				}
				num = 0L;
				while (num2-- > 0)
				{
					if (num >> 24 > 0)
					{
						return -1L;
					}
					b = (byte)bt.ReadByte();
					num = (num << 8) | b;
				}
			}
			return num;
		}

		public static string GetTagName(byte tag)
		{
			string text = "";
			if ((tag & 0xC0u) != 0)
			{
				switch (tag & 0xC0)
				{
				case 128:
					text = text + "CONTEXT SPECIFIC (" + (tag & 0x1F) + ")";
					break;
				case 64:
					text = text + "APPLICATION (" + (tag & 0x1F) + ")";
					break;
				case 192:
					text = text + "PRIVATE (" + (tag & 0x1F) + ")";
					break;
				case 32:
					text = text + "CONSTRUCTED (" + (tag & 0x1F) + ")";
					break;
				case 0:
					text = text + "UNIVERSAL (" + (tag & 0x1F) + ")";
					break;
				}
			}
			else
			{
				switch (tag & 0x1F)
				{
				case 1:
					text += "BOOLEAN";
					break;
				case 2:
					text += "INTEGER";
					break;
				case 3:
					text += "BIT STRING";
					break;
				case 4:
					text += "OCTET STRING";
					break;
				case 5:
					text += "NULL";
					break;
				case 6:
					text += "OBJECT IDENTIFIER";
					break;
				case 7:
					text += "OBJECT DESCRIPTOR";
					break;
				case 13:
					text += "RELATIVE-OID";
					break;
				case 8:
					text += "EXTERNAL";
					break;
				case 9:
					text += "REAL";
					break;
				case 10:
					text += "ENUMERATED";
					break;
				case 12:
					text += "UTF8 STRING";
					break;
				case 16:
					text += "SEQUENCE";
					break;
				case 17:
					text += "SET";
					break;
				case 18:
					text += "NUMERIC STRING";
					break;
				case 19:
					text += "PRINTABLE STRING";
					break;
				case 20:
					text += "T61 STRING";
					break;
				case 21:
					text += "VIDEOTEXT STRING";
					break;
				case 22:
					text += "IA5 STRING";
					break;
				case 23:
					text += "UTC TIME";
					break;
				case 24:
					text += "GENERALIZED TIME";
					break;
				case 25:
					text += "GRAPHIC STRING";
					break;
				case 26:
					text += "VISIBLE STRING";
					break;
				case 27:
					text += "GENERAL STRING";
					break;
				case 28:
					text += "UNIVERSAL STRING";
					break;
				case 30:
					text += "BMP STRING";
					break;
				default:
					text += "UNKNOWN TAG";
					break;
				}
			}
			return text;
		}

		public static object ReadRegInfo(string path, string name)
		{
			object result = null;
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path, false);
			if (registryKey != null)
			{
				result = registryKey.GetValue(name);
			}
			return result;
		}

		public static void WriteRegInfo(string path, string name, object data)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(path, true);
			if (registryKey == null)
			{
				registryKey = Registry.LocalMachine.CreateSubKey(path);
			}
			if (registryKey != null)
			{
				registryKey.SetValue(name, data);
			}
		}

		private Asn1Util()
		{
		}
	}
}
