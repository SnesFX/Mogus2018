using System;

namespace Mono.Posix
{
	[Flags]
	[CLSCompliant(false)]
	[Obsolete("Use Mono.Unix.Native.AccessModes")]
	public enum AccessMode
	{
		R_OK = 1,
		W_OK = 2,
		X_OK = 4,
		F_OK = 8
	}
}
