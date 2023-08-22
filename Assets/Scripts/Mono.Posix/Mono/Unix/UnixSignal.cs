using System;
using System.Runtime.InteropServices;
using System.Threading;
using Mono.Unix.Native;

namespace Mono.Unix
{
	public class UnixSignal : WaitHandle
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int Mono_Posix_RuntimeIsShuttingDown();

		[Map]
		private struct SignalInfo
		{
			public int signum;

			public int count;

			public int read_fd;

			public int write_fd;

			public int pipecnt;

			public int pipelock;

			public int have_handler;

			public IntPtr handler;
		}

		private int signum;

		private IntPtr signal_info;

		private static Mono_Posix_RuntimeIsShuttingDown ShuttingDown;

		public Signum Signum
		{
			get
			{
				if (IsRealTimeSignal)
				{
					throw new InvalidOperationException("This signal is a RealTimeSignum");
				}
				return NativeConvert.ToSignum(signum);
			}
		}

		public RealTimeSignum RealTimeSignum
		{
			get
			{
				if (!IsRealTimeSignal)
				{
					throw new InvalidOperationException("This signal is not a RealTimeSignum");
				}
				return NativeConvert.ToRealTimeSignum(signum - GetSIGRTMIN());
			}
		}

		public bool IsRealTimeSignal
		{
			get
			{
				AssertValid();
				int sIGRTMIN = GetSIGRTMIN();
				if (sIGRTMIN == -1)
				{
					return false;
				}
				return signum >= sIGRTMIN;
			}
		}

		private unsafe SignalInfo* Info
		{
			get
			{
				AssertValid();
				return (SignalInfo*)(void*)signal_info;
			}
		}

		public bool IsSet
		{
			get
			{
				return Count > 0;
			}
		}

		public unsafe int Count
		{
			get
			{
				return Info->count;
			}
			set
			{
				Interlocked.Exchange(ref Info->count, value);
			}
		}

		static UnixSignal()
		{
			ShuttingDown = RuntimeShuttingDownCallback;
			Stdlib.VersionCheck();
		}

		public UnixSignal(Signum signum)
		{
			this.signum = NativeConvert.FromSignum(signum);
			signal_info = install(this.signum);
			if (signal_info == IntPtr.Zero)
			{
				throw new ArgumentException("Unable to handle signal", "signum");
			}
		}

		public UnixSignal(RealTimeSignum rtsig)
		{
			signum = NativeConvert.FromRealTimeSignum(rtsig);
			signal_info = install(signum);
			Errno lastError = Stdlib.GetLastError();
			if (signal_info == IntPtr.Zero)
			{
				if (lastError == Errno.EADDRINUSE)
				{
					throw new ArgumentException("Signal registered outside of Mono.Posix", "signum");
				}
				throw new ArgumentException("Unable to handle signal", "signum");
			}
		}

		[DllImport("MonoPosixHelper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mono_Unix_UnixSignal_install", SetLastError = true)]
		private static extern IntPtr install(int signum);

		[DllImport("MonoPosixHelper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mono_Unix_UnixSignal_uninstall")]
		private static extern int uninstall(IntPtr info);

		private static int RuntimeShuttingDownCallback()
		{
			if (!Environment.HasShutdownStarted)
			{
				return 0;
			}
			return 1;
		}

		[DllImport("MonoPosixHelper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mono_Unix_UnixSignal_WaitAny")]
		private static extern int WaitAny(IntPtr[] infos, int count, int timeout, Mono_Posix_RuntimeIsShuttingDown shutting_down);

		[DllImport("MonoPosixHelper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mono_Posix_SIGRTMIN")]
		internal static extern int GetSIGRTMIN();

		[DllImport("MonoPosixHelper", CallingConvention = CallingConvention.Cdecl, EntryPoint = "Mono_Posix_SIGRTMAX")]
		internal static extern int GetSIGRTMAX();

		private void AssertValid()
		{
			if (signal_info == IntPtr.Zero)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		public unsafe bool Reset()
		{
			return Interlocked.Exchange(ref Info->count, 0) != 0;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (!(signal_info == IntPtr.Zero))
			{
				uninstall(signal_info);
				signal_info = IntPtr.Zero;
			}
		}

		public override bool WaitOne()
		{
			return WaitOne(-1, false);
		}

		public override bool WaitOne(TimeSpan timeout, bool exitContext)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			return WaitOne((int)num, exitContext);
		}

		public override bool WaitOne(int millisecondsTimeout, bool exitContext)
		{
			AssertValid();
			if (exitContext)
			{
				throw new InvalidOperationException("exitContext is not supported");
			}
			if (millisecondsTimeout == 0)
			{
				return IsSet;
			}
			return WaitAny(new UnixSignal[1] { this }, millisecondsTimeout) == 0;
		}

		public static int WaitAny(UnixSignal[] signals)
		{
			return WaitAny(signals, -1);
		}

		public static int WaitAny(UnixSignal[] signals, TimeSpan timeout)
		{
			long num = (long)timeout.TotalMilliseconds;
			if (num < -1 || num > int.MaxValue)
			{
				throw new ArgumentOutOfRangeException("timeout");
			}
			return WaitAny(signals, (int)num);
		}

		public static int WaitAny(UnixSignal[] signals, int millisecondsTimeout)
		{
			if (signals == null)
			{
				throw new ArgumentNullException("signals");
			}
			if (millisecondsTimeout < -1)
			{
				throw new ArgumentOutOfRangeException("millisecondsTimeout");
			}
			IntPtr[] array = new IntPtr[signals.Length];
			for (int i = 0; i < signals.Length; i++)
			{
				array[i] = signals[i].signal_info;
				if (array[i] == IntPtr.Zero)
				{
					throw new InvalidOperationException("Disposed UnixSignal");
				}
			}
			return WaitAny(array, array.Length, millisecondsTimeout, ShuttingDown);
		}
	}
}
