using System;

namespace Mono.Posix
{
	[Flags]
	[CLSCompliant(false)]
	[Obsolete("Use Mono.Unix.Native.FilePermissions")]
	public enum FileMode
	{
		S_ISUID = 0x800,
		S_ISGID = 0x400,
		S_ISVTX = 0x200,
		S_IRUSR = 0x100,
		S_IWUSR = 0x80,
		S_IXUSR = 0x40,
		S_IRGRP = 0x20,
		S_IWGRP = 0x10,
		S_IXGRP = 8,
		S_IROTH = 4,
		S_IWOTH = 2,
		S_IXOTH = 1
	}
}
