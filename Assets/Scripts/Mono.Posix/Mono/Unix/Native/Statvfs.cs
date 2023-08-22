using System;

namespace Mono.Unix.Native
{
	[Map]
	[CLSCompliant(false)]
	public struct Statvfs : IEquatable<Statvfs>
	{
		public ulong f_bsize;

		public ulong f_frsize;

		[fsblkcnt_t]
		public ulong f_blocks;

		[fsblkcnt_t]
		public ulong f_bfree;

		[fsblkcnt_t]
		public ulong f_bavail;

		[fsfilcnt_t]
		public ulong f_files;

		[fsfilcnt_t]
		public ulong f_ffree;

		[fsfilcnt_t]
		public ulong f_favail;

		public ulong f_fsid;

		public MountFlags f_flag;

		public ulong f_namemax;

		public override int GetHashCode()
		{
			return f_bsize.GetHashCode() ^ f_frsize.GetHashCode() ^ f_blocks.GetHashCode() ^ f_bfree.GetHashCode() ^ f_bavail.GetHashCode() ^ f_files.GetHashCode() ^ f_ffree.GetHashCode() ^ f_favail.GetHashCode() ^ f_fsid.GetHashCode() ^ f_flag.GetHashCode() ^ f_namemax.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}
			Statvfs statvfs = (Statvfs)obj;
			if (statvfs.f_bsize == f_bsize && statvfs.f_frsize == f_frsize && statvfs.f_blocks == f_blocks && statvfs.f_bfree == f_bfree && statvfs.f_bavail == f_bavail && statvfs.f_files == f_files && statvfs.f_ffree == f_ffree && statvfs.f_favail == f_favail && statvfs.f_fsid == f_fsid && statvfs.f_flag == f_flag)
			{
				return statvfs.f_namemax == f_namemax;
			}
			return false;
		}

		public bool Equals(Statvfs value)
		{
			if (value.f_bsize == f_bsize && value.f_frsize == f_frsize && value.f_blocks == f_blocks && value.f_bfree == f_bfree && value.f_bavail == f_bavail && value.f_files == f_files && value.f_ffree == f_ffree && value.f_favail == f_favail && value.f_fsid == f_fsid && value.f_flag == f_flag)
			{
				return value.f_namemax == f_namemax;
			}
			return false;
		}

		public static bool operator ==(Statvfs lhs, Statvfs rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Statvfs lhs, Statvfs rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}
