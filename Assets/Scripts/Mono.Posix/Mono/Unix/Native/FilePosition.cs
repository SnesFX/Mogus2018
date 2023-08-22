using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Mono.Unix.Native
{
	public sealed class FilePosition : MarshalByRefObject, IDisposable, IEquatable<FilePosition>
	{
		private static readonly int FilePositionDumpSize = Stdlib.DumpFilePosition(null, new HandleRef(null, IntPtr.Zero), 0);

		private HandleRef pos;

		internal HandleRef Handle
		{
			get
			{
				return pos;
			}
		}

		public FilePosition()
		{
			IntPtr intPtr = Stdlib.CreateFilePosition();
			if (intPtr == IntPtr.Zero)
			{
				throw new OutOfMemoryException("Unable to malloc fpos_t!");
			}
			pos = new HandleRef(this, intPtr);
		}

		public void Dispose()
		{
			Cleanup();
			GC.SuppressFinalize(this);
		}

		private void Cleanup()
		{
			if (pos.Handle != IntPtr.Zero)
			{
				Stdlib.free(pos.Handle);
				pos = new HandleRef(this, IntPtr.Zero);
			}
		}

		public override string ToString()
		{
			return "(" + base.ToString() + " " + GetDump() + ")";
		}

		private string GetDump()
		{
			if (FilePositionDumpSize <= 0)
			{
				return "internal error";
			}
			StringBuilder stringBuilder = new StringBuilder(FilePositionDumpSize + 1);
			if (Stdlib.DumpFilePosition(stringBuilder, Handle, FilePositionDumpSize + 1) <= 0)
			{
				return "internal error dumping fpos_t";
			}
			return stringBuilder.ToString();
		}

		public override bool Equals(object obj)
		{
			FilePosition filePosition = obj as FilePosition;
			if (obj == null || filePosition == null)
			{
				return false;
			}
			return ToString().Equals(obj.ToString());
		}

		public bool Equals(FilePosition value)
		{
			if ((object)this == value)
			{
				return true;
			}
			return ToString().Equals(value.ToString());
		}

		public override int GetHashCode()
		{
			return ToString().GetHashCode();
		}

		~FilePosition()
		{
			Cleanup();
		}

		public static bool operator ==(FilePosition lhs, FilePosition rhs)
		{
			return object.Equals(lhs, rhs);
		}

		public static bool operator !=(FilePosition lhs, FilePosition rhs)
		{
			return !object.Equals(lhs, rhs);
		}
	}
}
