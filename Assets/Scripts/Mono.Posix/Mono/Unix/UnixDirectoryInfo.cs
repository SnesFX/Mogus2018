using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using Mono.Unix.Native;

namespace Mono.Unix
{
	public sealed class UnixDirectoryInfo : UnixFileSystemInfo
	{
		public override string Name
		{
			get
			{
				string fileName = UnixPath.GetFileName(base.FullPath);
				if (fileName == null || fileName.Length == 0)
				{
					return base.FullPath;
				}
				return fileName;
			}
		}

		public UnixDirectoryInfo Parent
		{
			get
			{
				if (base.FullPath == "/")
				{
					return this;
				}
				string directoryName = UnixPath.GetDirectoryName(base.FullPath);
				if (directoryName == "")
				{
					throw new InvalidOperationException("Do not know parent directory for path `" + base.FullPath + "'");
				}
				return new UnixDirectoryInfo(directoryName);
			}
		}

		public UnixDirectoryInfo Root
		{
			get
			{
				string pathRoot = UnixPath.GetPathRoot(base.FullPath);
				if (pathRoot == null)
				{
					return null;
				}
				return new UnixDirectoryInfo(pathRoot);
			}
		}

		public UnixDirectoryInfo(string path)
			: base(path)
		{
		}

		internal UnixDirectoryInfo(string path, Stat stat)
			: base(path, stat)
		{
		}

		[CLSCompliant(false)]
		public void Create(FilePermissions mode)
		{
			UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.mkdir(base.FullPath, mode));
			Refresh();
		}

		public void Create(FileAccessPermissions mode)
		{
			Create((FilePermissions)mode);
		}

		public void Create()
		{
			FilePermissions mode = FilePermissions.ACCESSPERMS;
			Create(mode);
		}

		public override void Delete()
		{
			Delete(false);
		}

		public void Delete(bool recursive)
		{
			if (recursive)
			{
				UnixFileSystemInfo[] fileSystemEntries = GetFileSystemEntries();
				foreach (UnixFileSystemInfo unixFileSystemInfo in fileSystemEntries)
				{
					UnixDirectoryInfo unixDirectoryInfo = unixFileSystemInfo as UnixDirectoryInfo;
					if (unixDirectoryInfo != null)
					{
						unixDirectoryInfo.Delete(true);
					}
					else
					{
						unixFileSystemInfo.Delete();
					}
				}
			}
			UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.rmdir(base.FullPath));
			Refresh();
		}

		public Dirent[] GetEntries()
		{
			IntPtr intPtr = Syscall.opendir(base.FullPath);
			if (intPtr == IntPtr.Zero)
			{
				UnixMarshal.ThrowExceptionForLastError();
			}
			bool flag = false;
			try
			{
				Dirent[] entries = GetEntries(intPtr);
				flag = true;
				return entries;
			}
			finally
			{
				int retval = Syscall.closedir(intPtr);
				if (flag)
				{
					UnixMarshal.ThrowExceptionForLastErrorIf(retval);
				}
			}
		}

		private static Dirent[] GetEntries(IntPtr dirp)
		{
			ArrayList arrayList = new ArrayList();
			int num;
			IntPtr result;
			do
			{
				Dirent dirent = new Dirent();
				num = Syscall.readdir_r(dirp, dirent, out result);
				if (num == 0 && result != IntPtr.Zero && dirent.d_name != "." && dirent.d_name != "..")
				{
					arrayList.Add(dirent);
				}
			}
			while (num == 0 && result != IntPtr.Zero);
			if (num != 0)
			{
				UnixMarshal.ThrowExceptionForLastErrorIf(num);
			}
			return (Dirent[])arrayList.ToArray(typeof(Dirent));
		}

		public Dirent[] GetEntries(Regex regex)
		{
			IntPtr intPtr = Syscall.opendir(base.FullPath);
			if (intPtr == IntPtr.Zero)
			{
				UnixMarshal.ThrowExceptionForLastError();
			}
			try
			{
				return GetEntries(intPtr, regex);
			}
			finally
			{
				UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.closedir(intPtr));
			}
		}

		private static Dirent[] GetEntries(IntPtr dirp, Regex regex)
		{
			ArrayList arrayList = new ArrayList();
			int num;
			IntPtr result;
			do
			{
				Dirent dirent = new Dirent();
				num = Syscall.readdir_r(dirp, dirent, out result);
				if (num == 0 && result != IntPtr.Zero && regex.Match(dirent.d_name).Success && dirent.d_name != "." && dirent.d_name != "..")
				{
					arrayList.Add(dirent);
				}
			}
			while (num == 0 && result != IntPtr.Zero);
			if (num != 0)
			{
				UnixMarshal.ThrowExceptionForLastError();
			}
			return (Dirent[])arrayList.ToArray(typeof(Dirent));
		}

		public Dirent[] GetEntries(string regex)
		{
			Regex regex2 = new Regex(regex);
			return GetEntries(regex2);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries()
		{
			Dirent[] entries = GetEntries();
			return GetFileSystemEntries(entries);
		}

		private UnixFileSystemInfo[] GetFileSystemEntries(Dirent[] dentries)
		{
			UnixFileSystemInfo[] array = new UnixFileSystemInfo[dentries.Length];
			for (int i = 0; i != array.Length; i++)
			{
				array[i] = UnixFileSystemInfo.GetFileSystemEntry(UnixPath.Combine(base.FullPath, dentries[i].d_name));
			}
			return array;
		}

		public UnixFileSystemInfo[] GetFileSystemEntries(Regex regex)
		{
			Dirent[] entries = GetEntries(regex);
			return GetFileSystemEntries(entries);
		}

		public UnixFileSystemInfo[] GetFileSystemEntries(string regex)
		{
			Regex regex2 = new Regex(regex);
			return GetFileSystemEntries(regex2);
		}

		public static string GetCurrentDirectory()
		{
			StringBuilder stringBuilder = new StringBuilder(16);
			IntPtr zero = IntPtr.Zero;
			do
			{
				stringBuilder.Capacity *= 2;
				zero = Syscall.getcwd(stringBuilder, (ulong)stringBuilder.Capacity);
			}
			while (zero == IntPtr.Zero && Stdlib.GetLastError() == Errno.ERANGE);
			if (zero == IntPtr.Zero)
			{
				UnixMarshal.ThrowExceptionForLastError();
			}
			return stringBuilder.ToString();
		}

		public static void SetCurrentDirectory(string path)
		{
			UnixMarshal.ThrowExceptionForLastErrorIf(Syscall.chdir(path));
		}
	}
}
