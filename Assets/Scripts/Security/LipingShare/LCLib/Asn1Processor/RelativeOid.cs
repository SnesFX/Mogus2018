using System;
using System.IO;

namespace LipingShare.LCLib.Asn1Processor
{
	internal class RelativeOid : Oid
	{
		public override void Encode(Stream bt, string oidStr)
		{
			string[] array = oidStr.Split('.');
			ulong[] array2 = new ulong[array.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array2[i] = Convert.ToUInt64(array[i]);
			}
			for (int j = 0; j < array2.Length; j++)
			{
				EncodeValue(bt, array2[j]);
			}
		}

		public override string Decode(Stream bt)
		{
			string text = "";
			ulong v = 0uL;
			bool flag = true;
			while (bt.Position < bt.Length)
			{
				try
				{
					DecodeValue(bt, ref v);
					if (flag)
					{
						text = v.ToString();
						flag = false;
					}
					else
					{
						text = text + "." + v;
					}
				}
				catch (Exception ex)
				{
					throw new Exception("Failed to decode OID value: " + ex.Message);
				}
			}
			return text;
		}
	}
}
