using System;
using System.Text;
using Mono.Unix.Native;

namespace Mono.Unix
{
	internal class ErrorMarshal
	{
		internal delegate string ErrorTranslator(Errno errno);

		internal static readonly ErrorTranslator Translate;

		static ErrorMarshal()
		{
			try
			{
				Translate = strerror_r;
				Translate(Errno.ERANGE);
			}
			catch (EntryPointNotFoundException)
			{
				Translate = strerror;
			}
		}

		private static string strerror(Errno errno)
		{
			return Stdlib.strerror(errno);
		}

		private static string strerror_r(Errno errno)
		{
			StringBuilder stringBuilder = new StringBuilder(16);
			int num = 0;
			do
			{
				stringBuilder.Capacity *= 2;
				num = Syscall.strerror_r(errno, stringBuilder);
			}
			while (num == -1 && Stdlib.GetLastError() == Errno.ERANGE);
			if (num == -1)
			{
				return "** Unknown error code: " + (int)errno + "**";
			}
			return stringBuilder.ToString();
		}
	}
}
