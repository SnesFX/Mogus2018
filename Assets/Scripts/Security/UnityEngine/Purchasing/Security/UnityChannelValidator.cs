using System;
using System.Text;

namespace UnityEngine.Purchasing.Security
{
	public class UnityChannelValidator
	{
		private readonly RSAKey key;

		private UnityChannelReceiptParser parser = new UnityChannelReceiptParser();

		public UnityChannelValidator(byte[] rsaKey)
		{
			try
			{
				key = new RSAKey(rsaKey);
			}
			catch (Exception ex)
			{
				throw new Exception(string.Concat("Cannot instantiate self with an invalid public key. (", ex, ")"));
			}
		}

		public UnityChannelReceipt Validate(string receipt, string signature)
		{
			byte[] signature2 = Convert.FromBase64String(signature);
			byte[] bytes = Encoding.UTF8.GetBytes(receipt);
			if (!key.Verify(bytes, signature2))
			{
				throw new InvalidSignatureException();
			}
			return parser.ParseUnityChannelReceipt(receipt);
		}
	}
}
