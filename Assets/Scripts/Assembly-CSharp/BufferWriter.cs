public class BufferWriter
{
	public int Length;

	public byte[] Buffer;

	public BufferWriter(byte[] buffer)
	{
		Buffer = buffer;
	}

	public void Write(float value)
	{
		short num = (short)(32600f * value);
		Buffer[Length++] = (byte)(num >> 8);
		Buffer[Length++] = (byte)((uint)num & 0xFFu);
	}
}
