using System;

namespace UnityEngine.Purchasing.Security
{
	public class GooglePlayReceipt : IPurchaseReceipt
	{
		public string productID { get; private set; }

		public string transactionID { get; private set; }

		public string packageName { get; private set; }

		public string purchaseToken { get; private set; }

		public DateTime purchaseDate { get; private set; }

		public GooglePurchaseState purchaseState { get; private set; }

		public GooglePlayReceipt(string productID, string transactionID, string packageName, string purchaseToken, DateTime purchaseTime, GooglePurchaseState purchaseState)
		{
			this.productID = productID;
			this.transactionID = transactionID;
			this.packageName = packageName;
			this.purchaseToken = purchaseToken;
			purchaseDate = purchaseTime;
			this.purchaseState = purchaseState;
		}
	}
}
