using System;

namespace Mono.Unix
{
	[Flags]
	public enum FileAccessPermissions
	{
		UserReadWriteExecute = 0x1C0,
		UserRead = 0x100,
		UserWrite = 0x80,
		UserExecute = 0x40,
		GroupReadWriteExecute = 0x38,
		GroupRead = 0x20,
		GroupWrite = 0x10,
		GroupExecute = 8,
		OtherReadWriteExecute = 7,
		OtherRead = 4,
		OtherWrite = 2,
		OtherExecute = 1,
		DefaultPermissions = 0x1B6,
		AllPermissions = 0x1FF
	}
}
