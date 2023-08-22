using System;

namespace Mono.Posix
{
	[Flags]
	[Obsolete("Use Mono.Unix.Native.FilePermissions")]
	public enum StatMode
	{
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFSOCK")]
		Socket = 0xC000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFLNK")]
		SymLink = 0xA000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFREG")]
		Regular = 0x8000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFBLK")]
		BlockDevice = 0x6000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFDIR")]
		Directory = 0x4000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFCHR")]
		CharDevice = 0x2000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFIFO")]
		FIFO = 0x1000,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_ISUID")]
		SUid = 0x800,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_ISGID")]
		SGid = 0x400,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_ISVTX")]
		Sticky = 0x200,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IRUSR")]
		OwnerRead = 0x100,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IWUSR")]
		OwnerWrite = 0x80,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IXUSR")]
		OwnerExecute = 0x40,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IRGRP")]
		GroupRead = 0x20,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IWGRP")]
		GroupWrite = 0x10,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IXGRP")]
		GroupExecute = 8,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IROTH")]
		OthersRead = 4,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IWOTH")]
		OthersWrite = 2,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IXOTH")]
		OthersExecute = 1
	}
}
