using System;

namespace Mono.Unix.Native
{
	public sealed class Dirent : IEquatable<Dirent>
	{
		[CLSCompliant(false)]
		public ulong d_ino;

		public long d_off;

		[CLSCompliant(false)]
		public ushort d_reclen;

		public byte d_type;

		public string d_name;

		public override int GetHashCode()
		{
			return d_ino.GetHashCode() ^ d_off.GetHashCode() ^ d_reclen.GetHashCode() ^ d_type.GetHashCode() ^ d_name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Dirent value = (Dirent)obj;
			return Equals(value);
		}

		public bool Equals(Dirent value)
		{
			if (value == null)
			{
				return false;
			}
			if (value.d_ino == d_ino && value.d_off == d_off && value.d_reclen == d_reclen && value.d_type == d_type)
			{
				return value.d_name == d_name;
			}
			return false;
		}

		public override string ToString()
		{
			return d_name;
		}

		public static bool operator ==(Dirent lhs, Dirent rhs)
		{
			return object.Equals(lhs, rhs);
		}

		public static bool operator !=(Dirent lhs, Dirent rhs)
		{
			return !object.Equals(lhs, rhs);
		}
	}
}
