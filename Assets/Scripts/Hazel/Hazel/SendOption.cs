using System;

namespace Hazel
{
	[Flags]
	public enum SendOption : byte
	{
		None = 0,
		Reliable = 1,
		FragmentedReliable = 2
	}
}
