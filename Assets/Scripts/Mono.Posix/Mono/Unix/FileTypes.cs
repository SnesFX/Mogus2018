namespace Mono.Unix
{
	public enum FileTypes
	{
		Directory = 16384,
		CharacterDevice = 8192,
		BlockDevice = 24576,
		RegularFile = 32768,
		Fifo = 4096,
		SymbolicLink = 40960,
		Socket = 49152
	}
}
