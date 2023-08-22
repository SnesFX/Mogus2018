using System;

namespace Mono.Unix.Native
{
	public sealed class Passwd : IEquatable<Passwd>
	{
		public string pw_name;

		public string pw_passwd;

		[CLSCompliant(false)]
		public uint pw_uid;

		[CLSCompliant(false)]
		public uint pw_gid;

		public string pw_gecos;

		public string pw_dir;

		public string pw_shell;

		public override int GetHashCode()
		{
			return pw_name.GetHashCode() ^ pw_passwd.GetHashCode() ^ pw_uid.GetHashCode() ^ pw_gid.GetHashCode() ^ pw_gecos.GetHashCode() ^ pw_dir.GetHashCode() ^ pw_dir.GetHashCode() ^ pw_shell.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || GetType() != obj.GetType())
			{
				return false;
			}
			Passwd value = (Passwd)obj;
			return Equals(value);
		}

		public bool Equals(Passwd value)
		{
			if (value == null)
			{
				return false;
			}
			if (value.pw_uid == pw_uid && value.pw_gid == pw_gid && value.pw_name == pw_name && value.pw_passwd == pw_passwd && value.pw_gecos == pw_gecos && value.pw_dir == pw_dir)
			{
				return value.pw_shell == pw_shell;
			}
			return false;
		}

		public override string ToString()
		{
			return string.Format("{0}:{1}:{2}:{3}:{4}:{5}:{6}", pw_name, pw_passwd, pw_uid, pw_gid, pw_gecos, pw_dir, pw_shell);
		}

		public static bool operator ==(Passwd lhs, Passwd rhs)
		{
			return object.Equals(lhs, rhs);
		}

		public static bool operator !=(Passwd lhs, Passwd rhs)
		{
			return !object.Equals(lhs, rhs);
		}
	}
}
