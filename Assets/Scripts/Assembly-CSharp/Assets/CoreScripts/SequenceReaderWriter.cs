using System;
using System.IO;

namespace Assets.CoreScripts
{
	public class SequenceReaderWriter : IDisposable
	{
		private readonly int MaxSize;

		private readonly byte[] streamBuffer;

		private readonly MemoryStream stream;

		private readonly byte[] buffer = new byte[256];

		private byte bufferIdx;

		public long Position
		{
			get
			{
				return stream.Position;
			}
		}

		public SequenceReaderWriter(int maxSize)
		{
			MaxSize = maxSize;
			streamBuffer = new byte[maxSize + 1024];
			stream = new MemoryStream(streamBuffer, true);
		}

		public bool IsFull()
		{
			return stream.Position + (int)bufferIdx >= MaxSize;
		}

		public void Write(byte input)
		{
			if (bufferIdx == byte.MaxValue)
			{
				throw new IndexOutOfRangeException();
			}
			buffer[bufferIdx++] = input;
		}

		public void Write(bool input)
		{
			Write((byte)(input ? 1u : 0u));
		}

		public void Write(string input)
		{
			Write((byte)input.Length);
			for (int i = 0; i < input.Length; i++)
			{
				byte input2 = (byte)input[i];
				Write(input2);
			}
		}

		public void Write(ushort input)
		{
			Write((byte)(input & 0xFFu));
			Write((byte)((uint)(input >> 8) & 0xFFu));
		}

		public void Write(uint input)
		{
			Write((byte)(input & 0xFFu));
			Write((byte)((input >> 8) & 0xFFu));
			Write((byte)((input >> 16) & 0xFFu));
			Write((byte)((input >> 24) & 0xFFu));
		}

		public void Reset()
		{
			stream.Position = 0L;
		}

		public void Flush()
		{
			stream.WriteByte(bufferIdx);
			stream.Write(buffer, 0, bufferIdx);
			bufferIdx = 0;
		}

		public byte[] ReadSequence()
		{
			int num = stream.ReadByte();
			if (num < 0)
			{
				return null;
			}
			byte[] result = new byte[num];
			stream.Read(result, 0, num);
			return result;
		}

		public void Write(byte[] input)
		{
			if (input.Length + bufferIdx > 255)
			{
				throw new IndexOutOfRangeException();
			}
			Array.Copy(input, 0, buffer, bufferIdx, input.Length);
			bufferIdx = (byte)(bufferIdx + input.Length);
		}

		public void Write(Guid currentGuid)
		{
			Write(currentGuid.ToByteArray());
		}

		public void Write(float input)
		{
			byte[] bytes = BitConverter.GetBytes(input);
			Write(bytes);
		}

		public string GetDataString()
		{
			stream.Flush();
			return Convert.ToBase64String(streamBuffer, 0, (int)stream.Position);
		}

		public void Dispose()
		{
			stream.Dispose();
		}
	}
}
