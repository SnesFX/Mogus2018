namespace UnityEngine.Purchasing
{
	internal interface INativeFacebookStore : INativeStore
	{
		bool Check();

		void Init();

		void SetUnityPurchasingCallback(UnityPurchasingCallback AsyncCallback);

		bool ConsumeItem(string purchaseToken);
	}
}
