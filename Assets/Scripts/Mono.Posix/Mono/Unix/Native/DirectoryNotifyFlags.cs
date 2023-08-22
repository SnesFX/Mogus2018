using System;

namespace Mono.Unix.Native
{
	[Map]
	[Flags]
	[CLSCompliant(false)]
	public enum DirectoryNotifyFlags
	{
		DN_ACCESS = 1,
		DN_MODIFY = 2,
		DN_CREATE = 4,
		DN_DELETE = 8,
		DN_RENAME = 0x10,
		DN_ATTRIB = 0x20,
		DN_MULTISHOT = int.MinValue
	}
}
