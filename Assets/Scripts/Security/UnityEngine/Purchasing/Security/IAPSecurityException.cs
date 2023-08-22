using System;

namespace UnityEngine.Purchasing.Security
{
	public class IAPSecurityException : Exception
	{
		public IAPSecurityException()
		{
		}

		public IAPSecurityException(string message)
			: base(message)
		{
		}
	}
}
