using System;
using System.Collections.Generic;
using System.Text;

namespace Hazel
{
	public class MessageWriter : IRecyclable
	{
		public static int BufferSize = 64000;

		public static readonly ObjectPool<MessageWriter> WriterPool = new ObjectPool<MessageWriter>(() => new MessageWriter(BufferSize));

		internal byte[] Buffer;

		public int Length;

		public int Position;

		private Stack<int> messageStarts = new Stack<int>();

		public SendOption SendOption { get; private set; }

		public MessageWriter(int bufferSize)
		{
			Buffer = new byte[bufferSize];
		}

		public byte[] ToByteArray(bool includeHeader)
		{
			if (includeHeader)
			{
				byte[] array = new byte[Length];
				System.Buffer.BlockCopy(Buffer, 0, array, 0, Length);
				return array;
			}
			switch (SendOption)
			{
			case SendOption.Reliable:
			{
				byte[] array3 = new byte[Length - 3];
				System.Buffer.BlockCopy(Buffer, 3, array3, 0, Length - 3);
				return array3;
			}
			case SendOption.None:
			{
				byte[] array2 = new byte[Length - 1];
				System.Buffer.BlockCopy(Buffer, 1, array2, 0, Length - 1);
				return array2;
			}
			default:
				throw new NotImplementedException();
			}
		}

		public static MessageWriter Get(SendOption sendOption = SendOption.None)
		{
			MessageWriter @object = WriterPool.GetObject();
			@object.Clear(sendOption);
			return @object;
		}

		public bool HasBytes(int expected)
		{
			if (SendOption == SendOption.None)
			{
				return Length > 1 + expected;
			}
			return Length > 3 + expected;
		}

		public void StartMessage(byte typeFlag)
		{
			messageStarts.Push(Position);
			Position += 2;
			Write(typeFlag);
		}

		public void EndMessage()
		{
			int num = messageStarts.Pop();
			ushort num2 = (ushort)(Position - num - 3);
			Buffer[num] = (byte)num2;
			Buffer[num + 1] = (byte)(num2 >> 8);
		}

		public void CancelMessage()
		{
			Position = messageStarts.Pop();
			Length = Position;
		}

		public void Clear(SendOption sendOption)
		{
			Position = (Length = 0);
			SendOption = sendOption;
			Buffer[0] = (byte)sendOption;
			switch (sendOption)
			{
			case SendOption.None:
				Length = (Position = 1);
				break;
			case SendOption.Reliable:
				Length = (Position = 3);
				break;
			case SendOption.FragmentedReliable:
				throw new NotImplementedException("Sry bruh");
			}
		}

		public void Recycle()
		{
			Position = (Length = 0);
			WriterPool.PutObject(this);
		}

		public void Write(bool value)
		{
			Buffer[Position++] = (byte)(value ? 1u : 0u);
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(sbyte value)
		{
			Buffer[Position++] = (byte)value;
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(byte value)
		{
			Buffer[Position++] = value;
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(short value)
		{
			Buffer[Position++] = (byte)value;
			Buffer[Position++] = (byte)(value >> 8);
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(ushort value)
		{
			Buffer[Position++] = (byte)value;
			Buffer[Position++] = (byte)(value >> 8);
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(int value)
		{
			Buffer[Position++] = (byte)value;
			Buffer[Position++] = (byte)(value >> 8);
			Buffer[Position++] = (byte)(value >> 16);
			Buffer[Position++] = (byte)(value >> 24);
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public unsafe void Write(float value)
		{
			fixed (byte* ptr2 = &Buffer[Position])
			{
				byte* ptr = (byte*)(&value);
				*ptr2 = *ptr;
				ptr2[1] = ptr[1];
				ptr2[2] = ptr[2];
				ptr2[3] = ptr[3];
			}
			Position += 4;
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value);
			WritePacked(bytes.Length);
			Write(bytes);
		}

		public void WriteBytesAndSize(byte[] bytes)
		{
			WritePacked((uint)bytes.Length);
			Write(bytes);
		}

		public void WriteBytesAndSize(byte[] bytes, int length)
		{
			WritePacked((uint)length);
			Write(bytes, length);
		}

		public void Write(byte[] bytes)
		{
			Array.Copy(bytes, 0, Buffer, Position, bytes.Length);
			Position += bytes.Length;
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void Write(byte[] bytes, int length)
		{
			Array.Copy(bytes, 0, Buffer, Position, length);
			Position += length;
			if (Position > Length)
			{
				Length = Position;
			}
		}

		public void WritePacked(int value)
		{
			WritePacked((uint)value);
		}

		public void WritePacked(uint value)
		{
			do
			{
				byte b = (byte)(value & 0xFFu);
				if (value >= 128)
				{
					b = (byte)(b | 0x80u);
				}
				Write(b);
				value >>= 7;
			}
			while (value != 0);
		}

		public unsafe static bool IsLittleEndian()
		{
			int num = 1;
			byte* ptr = (byte*)(&num);
			return *ptr == 1;
		}
	}
}
