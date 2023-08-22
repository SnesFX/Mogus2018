using System;

namespace Mono.Posix
{
	[Flags]
	[CLSCompliant(false)]
	[Obsolete("Use Mono.Unix.Native.OpenFlags")]
	public enum OpenFlags
	{
		O_RDONLY = 0,
		O_WRONLY = 1,
		O_RDWR = 2,
		O_CREAT = 4,
		O_EXCL = 8,
		O_NOCTTY = 0x10,
		O_TRUNC = 0x20,
		O_APPEND = 0x40,
		O_NONBLOCK = 0x80,
		O_SYNC = 0x100
	}
}
