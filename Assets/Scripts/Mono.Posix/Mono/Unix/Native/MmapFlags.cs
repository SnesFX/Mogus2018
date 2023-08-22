using System;

namespace Mono.Unix.Native
{
	[Map]
	[Flags]
	[CLSCompliant(false)]
	public enum MmapFlags
	{
		MAP_SHARED = 1,
		MAP_PRIVATE = 2,
		MAP_TYPE = 0xF,
		MAP_FIXED = 0x10,
		MAP_FILE = 0,
		MAP_ANONYMOUS = 0x20,
		MAP_ANON = 0x20,
		MAP_GROWSDOWN = 0x100,
		MAP_DENYWRITE = 0x800,
		MAP_EXECUTABLE = 0x1000,
		MAP_LOCKED = 0x2000,
		MAP_NORESERVE = 0x4000,
		MAP_POPULATE = 0x8000,
		MAP_NONBLOCK = 0x10000
	}
}
