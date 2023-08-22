using System;

namespace UnityEngine.Purchasing.Security
{
	public class UnityChannelReceipt : IPurchaseReceipt
	{
		public string transactionID { get; internal set; }

		public string productID { get; internal set; }

		public DateTime purchaseDate { get; internal set; }

		public string packageName { get; internal set; }

		public string status { get; internal set; }

		public string clientId { get; internal set; }

		public string payFee { get; internal set; }

		public string orderAttemptId { get; internal set; }

		public string country { get; internal set; }

		public string currency { get; internal set; }

		public string quantity { get; internal set; }

		public UnityChannelReceipt(string transactionId, string packageName, string productId, DateTime purchaseDate, string status, string clientId, string payFee, string orderAttemptId, string country, string currency, string quantity)
		{
			transactionID = transactionId;
			productID = productId;
			this.purchaseDate = purchaseDate;
			this.packageName = packageName;
			this.status = status;
			this.clientId = clientId;
			this.payFee = payFee;
			this.orderAttemptId = orderAttemptId;
			this.country = country;
			this.currency = currency;
			this.quantity = quantity;
		}
	}
}
