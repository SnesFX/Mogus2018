using System;

namespace Mono.Unix.Native
{
	[AttributeUsage(AttributeTargets.Field)]
	internal class gid_tAttribute : MapAttribute
	{
		public gid_tAttribute()
			: base("gid_t")
		{
		}
	}
}
