using System;

namespace UnityEngine.Purchasing.Security
{
	public interface IPurchaseReceipt
	{
		string transactionID { get; }

		string productID { get; }

		DateTime purchaseDate { get; }
	}
}
