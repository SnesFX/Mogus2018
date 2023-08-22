using System;

namespace Mono.Unix.Native
{
	[Map]
	[Flags]
	[CLSCompliant(false)]
	public enum MountFlags : ulong
	{
		ST_RDONLY = 1uL,
		ST_NOSUID = 2uL,
		ST_NODEV = 4uL,
		ST_NOEXEC = 8uL,
		ST_SYNCHRONOUS = 0x10uL,
		ST_REMOUNT = 0x20uL,
		ST_MANDLOCK = 0x40uL,
		ST_WRITE = 0x80uL,
		ST_APPEND = 0x100uL,
		ST_IMMUTABLE = 0x200uL,
		ST_NOATIME = 0x400uL,
		ST_NODIRATIME = 0x800uL,
		ST_BIND = 0x1000uL
	}
}
