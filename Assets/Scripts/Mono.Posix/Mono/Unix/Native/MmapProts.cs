using System;

namespace Mono.Unix.Native
{
	[Map]
	[Flags]
	[CLSCompliant(false)]
	public enum MmapProts
	{
		PROT_READ = 1,
		PROT_WRITE = 2,
		PROT_EXEC = 4,
		PROT_NONE = 0,
		PROT_GROWSDOWN = 0x1000000,
		PROT_GROWSUP = 0x2000000
	}
}
