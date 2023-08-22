using System;

namespace Mono.Posix
{
	[Obsolete("Use Mono.Unix.Native.Stat")]
	public struct Stat
	{
		[Obsolete("Use Mono.Unix.Native.Stat.st_dev")]
		public readonly int Device;

		[Obsolete("Use Mono.Unix.Native.Stat.st_ino")]
		public readonly int INode;

		[Obsolete("Use Mono.Unix.Native.Stat.st_mode")]
		public readonly StatMode Mode;

		[Obsolete("Use Mono.Unix.Native.Stat.st_nlink")]
		public readonly int NLinks;

		[Obsolete("Use Mono.Unix.Native.Stat.st_uid")]
		public readonly int Uid;

		[Obsolete("Use Mono.Unix.Native.Stat.st_gid")]
		public readonly int Gid;

		[Obsolete("Use Mono.Unix.Native.Stat.st_rdev")]
		public readonly long DeviceType;

		[Obsolete("Use Mono.Unix.Native.Stat.st_size")]
		public readonly long Size;

		[Obsolete("Use Mono.Unix.Native.Stat.st_blksize")]
		public readonly long BlockSize;

		[Obsolete("Use Mono.Unix.Native.Stat.st_blocks")]
		public readonly long Blocks;

		[Obsolete("Use Mono.Unix.Native.Stat.st_atime")]
		public readonly DateTime ATime;

		[Obsolete("Use Mono.Unix.Native.Stat.st_mtime")]
		public readonly DateTime MTime;

		[Obsolete("Use Mono.Unix.Native.Stat.st_ctime")]
		public readonly DateTime CTime;

		[Obsolete("Use Mono.Unix.Native.NativeConvert.LocalUnixEpoch")]
		public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1);

		[Obsolete("Use Mono.Unix.Native.NativeConvert.ToDateTime")]
		public static DateTime UnixToDateTime(long unix)
		{
			return UnixEpoch.Add(TimeSpan.FromSeconds(unix)).ToLocalTime();
		}

		internal Stat(int device, int inode, int mode, int nlinks, int uid, int gid, int rdev, long size, long blksize, long blocks, long atime, long mtime, long ctime)
		{
			Device = device;
			INode = inode;
			Mode = (StatMode)mode;
			NLinks = nlinks;
			Uid = uid;
			Gid = gid;
			DeviceType = rdev;
			Size = size;
			BlockSize = blksize;
			Blocks = blocks;
			if (atime != 0L)
			{
				ATime = UnixToDateTime(atime);
			}
			else
			{
				ATime = default(DateTime);
			}
			if (mtime != 0L)
			{
				MTime = UnixToDateTime(mtime);
			}
			else
			{
				MTime = default(DateTime);
			}
			if (ctime != 0L)
			{
				CTime = UnixToDateTime(ctime);
			}
			else
			{
				CTime = default(DateTime);
			}
		}
	}
}
