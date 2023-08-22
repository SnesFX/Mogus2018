using System;
using System.Collections.Generic;

namespace UnityEngine.Purchasing.Security
{
	public class UnityChannelReceiptParser
	{
		public UnityChannelReceipt ParseUnityChannelReceipt(string receipt)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJson.JsonDecode(receipt);
			object value;
			dictionary.TryGetValue("cpOrderId", out value);
			object obj = "";
			object value2;
			dictionary.TryGetValue("productId", out value2);
			object value3;
			dictionary.TryGetValue("paidTime", out value3);
			object value4;
			dictionary.TryGetValue("quantity", out value4);
			object value5;
			dictionary.TryGetValue("status", out value5);
			object value6;
			dictionary.TryGetValue("clientId", out value6);
			object value7;
			dictionary.TryGetValue("payFee", out value7);
			object value8;
			dictionary.TryGetValue("orderAttemptId", out value8);
			object value9;
			dictionary.TryGetValue("country", out value9);
			object value10;
			dictionary.TryGetValue("currency", out value10);
			DateTime result = default(DateTime);
			if (!DateTime.TryParse((string)value3, out result))
			{
				result = DateTime.MinValue;
			}
			return new UnityChannelReceipt((string)value, (string)obj, (string)value2, result, (string)value5, (string)value6, (string)value7, (string)value8, (string)value9, (string)value10, (string)value4);
		}
	}
}
