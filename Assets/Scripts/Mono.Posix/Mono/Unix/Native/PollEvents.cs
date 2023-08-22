using System;

namespace Mono.Unix.Native
{
	[Map]
	[Flags]
	public enum PollEvents : short
	{
		POLLIN = 1,
		POLLPRI = 2,
		POLLOUT = 4,
		POLLERR = 8,
		POLLHUP = 0x10,
		POLLNVAL = 0x20,
		POLLRDNORM = 0x40,
		POLLRDBAND = 0x80,
		POLLWRNORM = 0x100,
		POLLWRBAND = 0x200
	}
}
