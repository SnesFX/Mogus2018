using System;

namespace Mono.Unix.Native
{
	[AttributeUsage(AttributeTargets.Field)]
	internal class fsfilcnt_tAttribute : MapAttribute
	{
		public fsfilcnt_tAttribute()
			: base("fsfilcnt_t")
		{
		}
	}
}
