using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace Hazel
{
	public class MessageReader : IRecyclable
	{
		public static readonly ObjectPool<MessageReader> ReaderPool = new ObjectPool<MessageReader>(() => new MessageReader());

		public byte[] Buffer;

		public byte Tag;

		public int Length;

		private int _position;

		private int readHead;

		public int Offset { get; private set; }

		public int Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				readHead = _position + Offset;
			}
		}

		public static MessageReader Get(MessageReader srcMsg)
		{
			MessageReader @object = ReaderPool.GetObject();
			@object.Buffer = srcMsg.Buffer;
			@object.Offset = srcMsg.Offset;
			@object.Position = srcMsg.Position;
			@object.Length = srcMsg.Length;
			@object.Tag = srcMsg.Tag;
			return @object;
		}

		public static MessageReader Get(byte[] buffer)
		{
			MessageReader @object = ReaderPool.GetObject();
			@object.Buffer = buffer;
			@object.Offset = 0;
			@object.Position = 0;
			@object.Length = buffer.Length;
			@object.Tag = byte.MaxValue;
			return @object;
		}

		public static MessageReader Get(byte[] buffer, int offset)
		{
			MessageReader @object = ReaderPool.GetObject();
			@object.Buffer = buffer;
			@object.Offset = offset;
			@object.Position = 0;
			if (@object.readHead + 3 > @object.Buffer.Length)
			{
				return null;
			}
			@object.Length = @object.ReadUInt16();
			@object.Tag = @object.ReadByte();
			@object.Offset += 3;
			@object.Position = 0;
			return @object;
		}

		public MessageReader ReadMessage()
		{
			MessageReader messageReader = Get(Buffer, readHead);
			if (messageReader == null)
			{
				return null;
			}
			Position += messageReader.Length + 3;
			return messageReader;
		}

		public void Recycle()
		{
			Position = (Length = 0);
			ReaderPool.PutObject(this);
		}

		public bool ReadBoolean()
		{
			return FastByte() != 0;
		}

		public sbyte ReadSByte()
		{
			return (sbyte)FastByte();
		}

		public byte ReadByte()
		{
			return FastByte();
		}

		public ushort ReadUInt16()
		{
			return (ushort)(FastByte() | (FastByte() << 8));
		}

		public short ReadInt16()
		{
			return (short)(FastByte() | (FastByte() << 8));
		}

		public int ReadInt32()
		{
			return FastByte() | (FastByte() << 8) | (FastByte() << 16) | (FastByte() << 24);
		}

		public unsafe float ReadSingle()
		{
			float result = 0f;
			fixed (byte* ptr2 = &Buffer[readHead])
			{
				byte* ptr = (byte*)(&result);
				*ptr = *ptr2;
				ptr[1] = ptr2[1];
				ptr[2] = ptr2[2];
				ptr[3] = ptr2[3];
			}
			Position += 4;
			return result;
		}

		public string ReadString()
		{
			int num = ReadPackedInt32();
			string @string = Encoding.UTF8.GetString(Buffer, readHead, num);
			Position += num;
			return @string;
		}

		public byte[] ReadBytesAndSize()
		{
			int length = ReadPackedInt32();
			return ReadBytes(length);
		}

		public byte[] ReadBytes(int length)
		{
			byte[] array = new byte[length];
			Array.Copy(Buffer, readHead, array, 0, array.Length);
			Position += array.Length;
			return array;
		}

		public int ReadPackedInt32()
		{
			return (int)ReadPackedUInt32();
		}

		public uint ReadPackedUInt32()
		{
			bool flag = true;
			int num = 0;
			uint num2 = 0u;
			while (flag)
			{
				byte b = ReadByte();
				if (b >= 128)
				{
					flag = true;
					b = (byte)(b ^ 0x80u);
				}
				else
				{
					flag = false;
				}
				num2 |= (uint)(b << num);
				num += 7;
			}
			return num2;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private byte FastByte()
		{
			_position++;
			return Buffer[readHead++];
		}

		public unsafe static bool IsLittleEndian()
		{
			int num = 1;
			byte* ptr = (byte*)(&num);
			return *ptr == 1;
		}
	}
}
