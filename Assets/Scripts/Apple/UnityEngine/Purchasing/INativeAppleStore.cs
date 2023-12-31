namespace UnityEngine.Purchasing
{
	internal interface INativeAppleStore : INativeStore
	{
		string appReceipt { get; }

		bool canMakePayments { get; }

		bool simulateAskToBuy { get; set; }

		void SetUnityPurchasingCallback(UnityPurchasingCallback AsyncCallback);

		void RestoreTransactions();

		void RefreshAppReceipt();

		void AddTransactionObserver();

		void SetApplicationUsername(string applicationUsername);

		void SetStorePromotionOrder(string json);

		void SetStorePromotionVisibility(string productId, string visibility);

		string GetTransactionReceiptForProductId(string productId);

		void InterceptPromotionalPurchases();

		void ContinuePromotionalPurchases();
	}
}
