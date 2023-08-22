using System;

namespace UnityEngine.Purchasing.Security
{
	public class AppleReceipt
	{
		public AppleInAppPurchaseReceipt[] inAppPurchaseReceipts;

		public string bundleID { get; internal set; }

		public string appVersion { get; internal set; }

		public DateTime expirationDate { get; internal set; }

		public byte[] opaque { get; internal set; }

		public byte[] hash { get; internal set; }

		public string originalApplicationVersion { get; internal set; }

		public DateTime receiptCreationDate { get; internal set; }
	}
}
