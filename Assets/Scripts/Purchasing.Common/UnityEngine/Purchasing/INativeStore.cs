namespace UnityEngine.Purchasing
{
	internal interface INativeStore
	{
		void RetrieveProducts(string json);

		void Purchase(string productJSON, string developerPayload);

		void FinishTransaction(string productJSON, string transactionID);
	}
}
