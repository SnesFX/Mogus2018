using System;

namespace Mono.Unix.Native
{
	public struct RealTimeSignum : IEquatable<RealTimeSignum>
	{
		private int rt_offset;

		private static readonly int MaxOffset = UnixSignal.GetSIGRTMAX() - UnixSignal.GetSIGRTMIN() - 1;

		public static readonly RealTimeSignum MinValue = new RealTimeSignum(0);

		public static readonly RealTimeSignum MaxValue = new RealTimeSignum(MaxOffset);

		public int Offset
		{
			get
			{
				return rt_offset;
			}
		}

		public RealTimeSignum(int offset)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("Offset cannot be negative");
			}
			if (offset > MaxOffset)
			{
				throw new ArgumentOutOfRangeException("Offset greater than maximum supported SIGRT");
			}
			rt_offset = offset;
		}

		public override int GetHashCode()
		{
			return rt_offset.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((RealTimeSignum)obj);
		}

		public bool Equals(RealTimeSignum value)
		{
			return Offset == value.Offset;
		}

		public static bool operator ==(RealTimeSignum lhs, RealTimeSignum rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(RealTimeSignum lhs, RealTimeSignum rhs)
		{
			return !lhs.Equals(rhs);
		}
	}
}
