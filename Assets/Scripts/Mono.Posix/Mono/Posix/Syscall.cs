using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Posix
{
	[CLSCompliant(false)]
	[Obsolete("Use Mono.Unix.Native.Syscall.")]
	public class Syscall
	{
		public delegate void sighandler_t(int v);

		[DllImport("libc", SetLastError = true)]
		public static extern int exit(int status);

		[DllImport("libc", SetLastError = true)]
		public static extern int fork();

		[DllImport("libc", SetLastError = true)]
		public unsafe static extern IntPtr read(int fileDescriptor, void* buf, IntPtr count);

		[DllImport("libc", SetLastError = true)]
		public unsafe static extern IntPtr write(int fileDescriptor, void* buf, IntPtr count);

		[DllImport("libc", EntryPoint = "open", SetLastError = true)]
		internal static extern int syscall_open(string pathname, int flags, int mode);

		[DllImport("MonoPosixHelper")]
		internal static extern int map_Mono_Posix_OpenFlags(OpenFlags flags);

		[DllImport("MonoPosixHelper")]
		internal static extern int map_Mono_Posix_FileMode(FileMode mode);

		public static int open(string pathname, OpenFlags flags)
		{
			if ((flags & OpenFlags.O_CREAT) != 0)
			{
				throw new ArgumentException("If you pass O_CREAT, you must call the method with the mode flag");
			}
			int flags2 = map_Mono_Posix_OpenFlags(flags);
			return syscall_open(pathname, flags2, 0);
		}

		public static int open(string pathname, OpenFlags flags, FileMode mode)
		{
			int flags2 = map_Mono_Posix_OpenFlags(flags);
			int mode2 = map_Mono_Posix_FileMode(mode);
			return syscall_open(pathname, flags2, mode2);
		}

		[DllImport("libc", SetLastError = true)]
		public static extern int close(int fileDescriptor);

		[DllImport("libc", EntryPoint = "waitpid", SetLastError = true)]
		internal unsafe static extern int syscall_waitpid(int pid, int* status, int options);

		[DllImport("MonoPosixHelper")]
		internal static extern int map_Mono_Posix_WaitOptions(WaitOptions wait_options);

		public unsafe static int waitpid(int pid, out int status, WaitOptions options)
		{
			int num = 0;
			int result = syscall_waitpid(pid, &num, map_Mono_Posix_WaitOptions(options));
			status = num;
			return result;
		}

		public unsafe static int waitpid(int pid, WaitOptions options)
		{
			return syscall_waitpid(pid, null, map_Mono_Posix_WaitOptions(options));
		}

		[DllImport("MonoPosixHelper", EntryPoint = "wifexited")]
		public static extern int WIFEXITED(int status);

		[DllImport("MonoPosixHelper", EntryPoint = "wexitstatus")]
		public static extern int WEXITSTATUS(int status);

		[DllImport("MonoPosixHelper", EntryPoint = "wifsignaled")]
		public static extern int WIFSIGNALED(int status);

		[DllImport("MonoPosixHelper", EntryPoint = "wtermsig")]
		public static extern int WTERMSIG(int status);

		[DllImport("MonoPosixHelper", EntryPoint = "wifstopped")]
		public static extern int WIFSTOPPED(int status);

		[DllImport("MonoPosixHelper", EntryPoint = "wstopsig")]
		public static extern int WSTOPSIG(int status);

		[DllImport("libc", EntryPoint = "creat", SetLastError = true)]
		internal static extern int syscall_creat(string pathname, int flags);

		public static int creat(string pathname, FileMode flags)
		{
			return syscall_creat(pathname, map_Mono_Posix_FileMode(flags));
		}

		[DllImport("libc", SetLastError = true)]
		public static extern int link(string oldPath, string newPath);

		[DllImport("libc", SetLastError = true)]
		public static extern int unlink(string path);

		[DllImport("libc", SetLastError = true)]
		public static extern int symlink(string oldpath, string newpath);

		[DllImport("libc", SetLastError = true)]
		public static extern int chdir(string path);

		[DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
		internal static extern int syscall_chmod(string path, int mode);

		public static int chmod(string path, FileMode mode)
		{
			return syscall_chmod(path, map_Mono_Posix_FileMode(mode));
		}

		[DllImport("libc", SetLastError = true)]
		public static extern int chown(string path, int owner, int group);

		[DllImport("libc", SetLastError = true)]
		public static extern int lchown(string path, int owner, int group);

		[DllImport("libc", SetLastError = true)]
		public static extern int lseek(int fileDescriptor, int offset, int whence);

		[DllImport("libc", SetLastError = true)]
		public static extern int getpid();

		[DllImport("libc", SetLastError = true)]
		public static extern int setuid(int uid);

		[DllImport("libc", SetLastError = true)]
		public static extern int getuid();

		[DllImport("libc")]
		public static extern uint alarm(uint seconds);

		[DllImport("libc", SetLastError = true)]
		public static extern int pause();

		[DllImport("libc", EntryPoint = "access", SetLastError = true)]
		internal static extern int syscall_access(string pathname, int mode);

		[DllImport("MonoPosixHelper")]
		internal static extern int map_Mono_Posix_AccessMode(AccessMode mode);

		public static int access(string pathname, AccessMode mode)
		{
			return syscall_access(pathname, map_Mono_Posix_AccessMode(mode));
		}

		[DllImport("libc", SetLastError = true)]
		public static extern int nice(int increment);

		[DllImport("libc")]
		public static extern void sync();

		[DllImport("libc", SetLastError = true)]
		public static extern void kill(int pid, int sig);

		[DllImport("libc", SetLastError = true)]
		public static extern int rename(string oldPath, string newPath);

		[DllImport("libc", EntryPoint = "mkdir", SetLastError = true)]
		internal static extern int syscall_mkdir(string pathname, int mode);

		public static int mkdir(string pathname, FileMode mode)
		{
			return syscall_mkdir(pathname, map_Mono_Posix_FileMode(mode));
		}

		[DllImport("libc", SetLastError = true)]
		public static extern int rmdir(string path);

		[DllImport("libc", SetLastError = true)]
		public static extern int dup(int fileDescriptor);

		[DllImport("libc", SetLastError = true)]
		public static extern int setgid(int gid);

		[DllImport("libc", SetLastError = true)]
		public static extern int getgid();

		[DllImport("libc", SetLastError = true)]
		public static extern int signal(int signum, sighandler_t handler);

		[DllImport("libc", SetLastError = true)]
		public static extern int geteuid();

		[DllImport("libc", SetLastError = true)]
		public static extern int getegid();

		[DllImport("libc", SetLastError = true)]
		public static extern int setpgid(int pid, int pgid);

		[DllImport("libc")]
		public static extern int umask(int umask);

		[DllImport("libc", SetLastError = true)]
		public static extern int chroot(string path);

		[DllImport("libc", SetLastError = true)]
		public static extern int dup2(int oldFileDescriptor, int newFileDescriptor);

		[DllImport("libc", SetLastError = true)]
		public static extern int getppid();

		[DllImport("libc", SetLastError = true)]
		public static extern int getpgrp();

		[DllImport("libc", SetLastError = true)]
		public static extern int setsid();

		[DllImport("libc", SetLastError = true)]
		public static extern int setreuid(int ruid, int euid);

		[DllImport("libc", SetLastError = true)]
		public static extern int setregid(int rgid, int egid);

		[DllImport("MonoPosixHelper", SetLastError = true)]
		private static extern string helper_Mono_Posix_GetUserName(int uid);

		[DllImport("MonoPosixHelper", SetLastError = true)]
		private static extern string helper_Mono_Posix_GetGroupName(int gid);

		public static string getusername(int uid)
		{
			return helper_Mono_Posix_GetUserName(uid);
		}

		public static string getgroupname(int gid)
		{
			return helper_Mono_Posix_GetGroupName(gid);
		}

		[DllImport("libc", EntryPoint = "gethostname", SetLastError = true)]
		private static extern int syscall_gethostname(byte[] p, int len);

		public static string GetHostName()
		{
			byte[] array = new byte[256];
			int num = syscall_gethostname(array, array.Length);
			if (num == -1)
			{
				return "localhost";
			}
			for (num = 0; num < array.Length && array[num] != 0; num++)
			{
			}
			return Encoding.UTF8.GetString(array, 0, num);
		}

		[CLSCompliant(false)]
		public static string gethostname()
		{
			return GetHostName();
		}

		[DllImport("libc", EntryPoint = "isatty")]
		private static extern int syscall_isatty(int desc);

		public static bool isatty(int desc)
		{
			if (syscall_isatty(desc) == 1)
			{
				return true;
			}
			return false;
		}

		[DllImport("MonoPosixHelper")]
		internal static extern int helper_Mono_Posix_Stat(string filename, bool dereference, out int device, out int inode, out int mode, out int nlinks, out int uid, out int gid, out int rdev, out long size, out long blksize, out long blocks, out long atime, out long mtime, out long ctime);

		private static int stat2(string filename, bool dereference, out Stat stat)
		{
			int device;
			int inode;
			int mode;
			int nlinks;
			int uid;
			int gid;
			int rdev;
			long size;
			long blksize;
			long blocks;
			long atime;
			long mtime;
			long ctime;
			int num = helper_Mono_Posix_Stat(filename, dereference, out device, out inode, out mode, out nlinks, out uid, out gid, out rdev, out size, out blksize, out blocks, out atime, out mtime, out ctime);
			stat = new Stat(device, inode, mode, nlinks, uid, gid, rdev, size, blksize, blocks, atime, mtime, ctime);
			if (num != 0)
			{
				return num;
			}
			return 0;
		}

		public static int stat(string filename, out Stat stat)
		{
			return stat2(filename, false, out stat);
		}

		public static int lstat(string filename, out Stat stat)
		{
			return stat2(filename, true, out stat);
		}

		[DllImport("libc")]
		private static extern int readlink(string path, byte[] buffer, int buflen);

		public static string readlink(string path)
		{
			byte[] array = new byte[512];
			int num = readlink(path, array, array.Length);
			if (num == -1)
			{
				return null;
			}
			char[] array2 = new char[512];
			int chars = Encoding.Default.GetChars(array, 0, num, array2, 0);
			return new string(array2, 0, chars);
		}

		[DllImport("libc", EntryPoint = "strerror")]
		private static extern IntPtr _strerror(int errnum);

		public static string strerror(int errnum)
		{
			return Marshal.PtrToStringAnsi(_strerror(errnum));
		}

		[DllImport("libc")]
		public static extern IntPtr opendir(string path);

		[DllImport("libc")]
		public static extern int closedir(IntPtr dir);

		[DllImport("MonoPosixHelper", EntryPoint = "helper_Mono_Posix_readdir")]
		public static extern string readdir(IntPtr dir);
	}
}
