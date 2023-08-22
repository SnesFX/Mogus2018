namespace UnityEngine.Purchasing.Security
{
	public class StoreNotSupportedException : IAPSecurityException
	{
		public StoreNotSupportedException(string message)
			: base(message)
		{
		}
	}
}
