using System;

namespace Mono.Unix.Native
{
	[Map("struct flock")]
	public struct Flock : IEquatable<Flock>
	{
		[CLSCompliant(false)]
		public LockType l_type;

		[CLSCompliant(false)]
		public SeekFlags l_whence;

		[off_t]
		public long l_start;

		[off_t]
		public long l_len;

		[pid_t]
		public int l_pid;

		public override int GetHashCode()
		{
			return l_type.GetHashCode() ^ l_whence.GetHashCode() ^ l_start.GetHashCode() ^ l_len.GetHashCode() ^ l_pid.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}
			Flock flock = (Flock)obj;
			if (l_type == flock.l_type && l_whence == flock.l_whence && l_start == flock.l_start && l_len == flock.l_len)
			{
				return l_pid == flock.l_pid;
			}
			return false;
		}

		public bool Equals(Flock value)
		{
			if (l_type == value.l_type && l_whence == value.l_whence && l_start == value.l_start && l_len == value.l_len)
			{
				return l_pid == value.l_pid;
			}
			return false;
		}

		public static bool operator ==(Flock lhs, Flock rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Flock lhs, Flock rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}
