namespace UnityEngine.Purchasing.Security
{
	public class InvalidPublicKeyException : IAPSecurityException
	{
		public InvalidPublicKeyException(string message)
			: base(message)
		{
		}
	}
}
