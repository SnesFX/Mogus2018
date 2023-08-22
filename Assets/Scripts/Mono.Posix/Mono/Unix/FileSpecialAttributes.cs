using System;

namespace Mono.Unix
{
	[Flags]
	public enum FileSpecialAttributes
	{
		SetUserId = 0x800,
		SetGroupId = 0x400,
		Sticky = 0x200
	}
}
