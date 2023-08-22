namespace UnityEngine.Purchasing.Security
{
	public class AppleValidator
	{
		private X509Cert cert;

		private AppleReceiptParser parser = new AppleReceiptParser();

		public AppleValidator(byte[] appleRootCertificate)
		{
			cert = new X509Cert(appleRootCertificate);
		}

		public AppleReceipt Validate(byte[] receiptData)
		{
			PKCS7 receipt;
			AppleReceipt appleReceipt = parser.Parse(receiptData, out receipt);
			if (!receipt.Verify(cert, appleReceipt.receiptCreationDate))
			{
				throw new InvalidSignatureException();
			}
			return appleReceipt;
		}
	}
}
