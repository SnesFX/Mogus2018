namespace UnityEngine.Purchasing.Security
{
	public class GenericValidationException : IAPSecurityException
	{
		public GenericValidationException(string message)
			: base(message)
		{
		}
	}
}
