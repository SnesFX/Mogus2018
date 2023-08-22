namespace UnityEngine.Purchasing.Security
{
	public class MissingStoreSecretException : IAPSecurityException
	{
		public MissingStoreSecretException(string message)
			: base(message)
		{
		}
	}
}
