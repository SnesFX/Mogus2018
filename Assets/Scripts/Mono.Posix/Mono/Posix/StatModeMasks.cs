using System;

namespace Mono.Posix
{
	[Obsolete("Use Mono.Unix.Native.FilePermissions")]
	public enum StatModeMasks
	{
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_IFMT")]
		TypeMask = 61440,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_RWXU")]
		OwnerMask = 448,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_RWXG")]
		GroupMask = 56,
		[Obsolete("Use Mono.Unix.Native.FilePermissions.S_RWXO")]
		OthersMask = 7
	}
}
