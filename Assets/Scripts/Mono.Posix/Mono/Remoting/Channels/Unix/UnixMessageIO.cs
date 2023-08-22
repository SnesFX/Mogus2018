using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixMessageIO
	{
		private static byte[][] _msgHeaders = new byte[2][]
		{
			new byte[6] { 46, 78, 69, 84, 1, 0 },
			new byte[6] { 255, 255, 255, 255, 255, 255 }
		};

		public static int DefaultStreamBufferSize = 1000;

		private static byte[] msgUriTransportKey = new byte[4] { 4, 0, 1, 1 };

		private static byte[] msgContentTypeTransportKey = new byte[4] { 6, 0, 1, 1 };

		private static byte[] msgDefaultTransportKey = new byte[3] { 1, 0, 1 };

		private static byte[] msgHeaderTerminator = new byte[2];

		public static MessageStatus ReceiveMessageStatus(Stream networkStream, byte[] buffer)
		{
			try
			{
				StreamRead(networkStream, buffer, 6);
			}
			catch (Exception innerException)
			{
				throw new RemotingException("Unix transport error.", innerException);
			}
			try
			{
				bool[] array = new bool[_msgHeaders.Length];
				bool flag = true;
				int num = 0;
				while (flag)
				{
					flag = false;
					byte b = buffer[num];
					for (int i = 0; i < _msgHeaders.Length; i++)
					{
						if (num <= 0 || array[i])
						{
							array[i] = b == _msgHeaders[i][num];
							if (array[i] && num == _msgHeaders[i].Length - 1)
							{
								return (MessageStatus)i;
							}
							flag = flag || array[i];
						}
					}
					num++;
				}
				return MessageStatus.Unknown;
			}
			catch (Exception innerException2)
			{
				throw new RemotingException("Unix transport error.", innerException2);
			}
		}

		private static bool StreamRead(Stream networkStream, byte[] buffer, int count)
		{
			int num = 0;
			do
			{
				int num2 = networkStream.Read(buffer, num, count - num);
				if (num2 == 0)
				{
					throw new RemotingException("Connection closed");
				}
				num += num2;
			}
			while (num < count);
			return true;
		}

		public static void SendMessageStream(Stream networkStream, Stream data, ITransportHeaders requestHeaders, byte[] buffer)
		{
			if (buffer == null)
			{
				buffer = new byte[DefaultStreamBufferSize];
			}
			byte[] array = _msgHeaders[0];
			networkStream.Write(array, 0, array.Length);
			if (requestHeaders["__RequestUri"] != null)
			{
				buffer[0] = 0;
			}
			else
			{
				buffer[0] = 2;
			}
			buffer[1] = 0;
			buffer[2] = 0;
			buffer[3] = 0;
			int num = (int)data.Length;
			buffer[4] = (byte)num;
			buffer[5] = (byte)(num >> 8);
			buffer[6] = (byte)(num >> 16);
			buffer[7] = (byte)(num >> 24);
			networkStream.Write(buffer, 0, 8);
			SendHeaders(networkStream, requestHeaders, buffer);
			if (data is MemoryStream)
			{
				MemoryStream memoryStream = (MemoryStream)data;
				networkStream.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
				return;
			}
			for (int num2 = data.Read(buffer, 0, buffer.Length); num2 > 0; num2 = data.Read(buffer, 0, buffer.Length))
			{
				networkStream.Write(buffer, 0, num2);
			}
		}

		private static void SendHeaders(Stream networkStream, ITransportHeaders requestHeaders, byte[] buffer)
		{
			if (networkStream != null)
			{
				IEnumerator enumerator = requestHeaders.GetEnumerator();
				while (enumerator.MoveNext())
				{
					DictionaryEntry dictionaryEntry = (DictionaryEntry)enumerator.Current;
					string text = dictionaryEntry.Key.ToString();
					if (!(text == "__RequestUri"))
					{
						if (text == "Content-Type")
						{
							networkStream.Write(msgContentTypeTransportKey, 0, 4);
						}
						else
						{
							networkStream.Write(msgDefaultTransportKey, 0, 3);
							SendString(networkStream, dictionaryEntry.Key.ToString(), buffer);
							networkStream.WriteByte(1);
						}
					}
					else
					{
						networkStream.Write(msgUriTransportKey, 0, 4);
					}
					SendString(networkStream, dictionaryEntry.Value.ToString(), buffer);
				}
			}
			networkStream.Write(msgHeaderTerminator, 0, 2);
		}

		public static ITransportHeaders ReceiveHeaders(Stream networkStream, byte[] buffer)
		{
			StreamRead(networkStream, buffer, 2);
			byte b = buffer[0];
			TransportHeaders transportHeaders = new TransportHeaders();
			while (b != 0)
			{
				StreamRead(networkStream, buffer, 1);
				string key;
				switch (b)
				{
				case 4:
					key = "__RequestUri";
					break;
				case 6:
					key = "Content-Type";
					break;
				case 1:
					key = ReceiveString(networkStream, buffer);
					break;
				default:
					throw new NotSupportedException("Unknown header code: " + b);
				}
				StreamRead(networkStream, buffer, 1);
				transportHeaders[key] = ReceiveString(networkStream, buffer);
				StreamRead(networkStream, buffer, 2);
				b = buffer[0];
			}
			return transportHeaders;
		}

		public static Stream ReceiveMessageStream(Stream networkStream, out ITransportHeaders headers, byte[] buffer)
		{
			headers = null;
			if (buffer == null)
			{
				buffer = new byte[DefaultStreamBufferSize];
			}
			StreamRead(networkStream, buffer, 8);
			int num = buffer[4] | (buffer[5] << 8) | (buffer[6] << 16) | (buffer[7] << 24);
			headers = ReceiveHeaders(networkStream, buffer);
			byte[] buffer2 = new byte[num];
			StreamRead(networkStream, buffer2, num);
			return new MemoryStream(buffer2);
		}

		private static void SendString(Stream networkStream, string str, byte[] buffer)
		{
			int num = Encoding.UTF8.GetMaxByteCount(str.Length) + 4;
			if (num > buffer.Length)
			{
				buffer = new byte[num];
			}
			int bytes = Encoding.UTF8.GetBytes(str, 0, str.Length, buffer, 4);
			buffer[0] = (byte)bytes;
			buffer[1] = (byte)(bytes >> 8);
			buffer[2] = (byte)(bytes >> 16);
			buffer[3] = (byte)(bytes >> 24);
			networkStream.Write(buffer, 0, bytes + 4);
		}

		private static string ReceiveString(Stream networkStream, byte[] buffer)
		{
			StreamRead(networkStream, buffer, 4);
			int num = buffer[0] | (buffer[1] << 8) | (buffer[2] << 16) | (buffer[3] << 24);
			if (num == 0)
			{
				return string.Empty;
			}
			if (num > buffer.Length)
			{
				buffer = new byte[num];
			}
			StreamRead(networkStream, buffer, num);
			return new string(Encoding.UTF8.GetChars(buffer, 0, num));
		}
	}
}
