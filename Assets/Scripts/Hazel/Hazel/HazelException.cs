using System;

namespace Hazel
{
	[Serializable]
	public class HazelException : Exception
	{
		internal HazelException(string msg)
			: base(msg)
		{
		}

		internal HazelException(string msg, Exception e)
			: base(msg, e)
		{
		}
	}
}
