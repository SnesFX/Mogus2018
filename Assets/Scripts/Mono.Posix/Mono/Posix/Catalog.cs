using System;
using System.Runtime.InteropServices;

namespace Mono.Posix
{
	[Obsolete("Use Mono.Unix.Catalog")]
	public class Catalog
	{
		[DllImport("intl")]
		private static extern IntPtr bindtextdomain(IntPtr domainname, IntPtr dirname);

		[DllImport("intl")]
		private static extern IntPtr bind_textdomain_codeset(IntPtr domainname, IntPtr codeset);

		[DllImport("intl")]
		private static extern IntPtr textdomain(IntPtr domainname);

		public static void Init(string package, string localedir)
		{
			IntPtr intPtr = Marshal.StringToHGlobalAuto(package);
			IntPtr intPtr2 = Marshal.StringToHGlobalAuto(localedir);
			IntPtr intPtr3 = Marshal.StringToHGlobalAuto("UTF-8");
			bindtextdomain(intPtr, intPtr2);
			bind_textdomain_codeset(intPtr, intPtr3);
			textdomain(intPtr);
			Marshal.FreeHGlobal(intPtr);
			Marshal.FreeHGlobal(intPtr2);
			Marshal.FreeHGlobal(intPtr3);
		}

		[DllImport("intl")]
		private static extern IntPtr gettext(IntPtr instring);

		public static string GetString(string s)
		{
			IntPtr intPtr = Marshal.StringToHGlobalAuto(s);
			string result = Marshal.PtrToStringAuto(gettext(intPtr));
			Marshal.FreeHGlobal(intPtr);
			return result;
		}

		[DllImport("intl")]
		private static extern IntPtr ngettext(IntPtr singular, IntPtr plural, int n);

		public static string GetPluralString(string s, string p, int n)
		{
			IntPtr intPtr = Marshal.StringToHGlobalAuto(s);
			IntPtr intPtr2 = Marshal.StringToHGlobalAuto(p);
			string result = Marshal.PtrToStringAnsi(ngettext(intPtr, intPtr2, n));
			Marshal.FreeHGlobal(intPtr);
			Marshal.FreeHGlobal(intPtr2);
			return result;
		}
	}
}
