public class BufferReader
{
	public int Position;

	public int Length;

	public byte[] Buffer;

	public BufferReader(byte[] buffer)
	{
		Buffer = buffer;
		Length = Buffer.Length;
	}

	public float ReadFloat()
	{
		byte b = Buffer[Position++];
		byte b2 = Buffer[Position++];
		short num = (short)((b << 8) | b2);
		return (float)num / 32600f;
	}
}
