using System;

namespace Mono.Unix.Native
{
	public sealed class Fstab : IEquatable<Fstab>
	{
		public string fs_spec;

		public string fs_file;

		public string fs_vfstype;

		public string fs_mntops;

		public string fs_type;

		public int fs_freq;

		public int fs_passno;

		public override int GetHashCode()
		{
			return fs_spec.GetHashCode() ^ fs_file.GetHashCode() ^ fs_vfstype.GetHashCode() ^ fs_mntops.GetHashCode() ^ fs_type.GetHashCode() ^ fs_freq ^ fs_passno;
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Fstab value = (Fstab)obj;
			return Equals(value);
		}

		public bool Equals(Fstab value)
		{
			if (value == null)
			{
				return false;
			}
			if (value.fs_spec == fs_spec && value.fs_file == fs_file && value.fs_vfstype == fs_vfstype && value.fs_mntops == fs_mntops && value.fs_type == fs_type && value.fs_freq == fs_freq)
			{
				return value.fs_passno == fs_passno;
			}
			return false;
		}

		public override string ToString()
		{
			return fs_spec;
		}

		public static bool operator ==(Fstab lhs, Fstab rhs)
		{
			return object.Equals(lhs, rhs);
		}

		public static bool operator !=(Fstab lhs, Fstab rhs)
		{
			return !object.Equals(lhs, rhs);
		}
	}
}
