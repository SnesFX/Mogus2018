using System;

namespace Mono.Unix.Native
{
	[Map]
	[CLSCompliant(false)]
	public enum FcntlCommand
	{
		F_DUPFD = 0,
		F_GETFD = 1,
		F_SETFD = 2,
		F_GETFL = 3,
		F_SETFL = 4,
		F_GETLK = 12,
		F_SETLK = 13,
		F_SETLKW = 14,
		F_SETOWN = 8,
		F_GETOWN = 9,
		F_SETSIG = 10,
		F_GETSIG = 11,
		F_NOCACHE = 48,
		F_SETLEASE = 1024,
		F_GETLEASE = 1025,
		F_NOTIFY = 1026
	}
}
