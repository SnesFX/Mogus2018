using System.Text;
using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	internal class DistinguishedName
	{
		public string Country { get; set; }

		public string Organization { get; set; }

		public string OrganizationalUnit { get; set; }

		public string Dnq { get; set; }

		public string State { get; set; }

		public string CommonName { get; set; }

		public string SerialNumber { get; set; }

		public DistinguishedName(Asn1Node n)
		{
			if (n.MaskedTag != 16)
			{
				return;
			}
			for (int i = 0; i < n.ChildNodeCount; i++)
			{
				Asn1Node childNode = n.GetChildNode(i);
				if (childNode.MaskedTag != 17 || childNode.ChildNodeCount != 1)
				{
					throw new InvalidX509Data();
				}
				childNode = childNode.GetChildNode(0);
				if (childNode.MaskedTag != 16 || childNode.ChildNodeCount != 2)
				{
					throw new InvalidX509Data();
				}
				Asn1Node childNode2 = childNode.GetChildNode(0);
				Asn1Node childNode3 = childNode.GetChildNode(1);
				if (childNode2.MaskedTag != 6 || (childNode3.MaskedTag != 19 && childNode3.MaskedTag != 12))
				{
					throw new InvalidX509Data();
				}
				Oid oid = new Oid();
				string text = oid.Decode(childNode2.Data);
				UTF8Encoding uTF8Encoding = new UTF8Encoding();
				switch (text)
				{
				case "2.5.4.6":
					Country = uTF8Encoding.GetString(childNode3.Data);
					break;
				case "2.5.4.10":
					Organization = uTF8Encoding.GetString(childNode3.Data);
					break;
				case "2.5.4.11":
					OrganizationalUnit = uTF8Encoding.GetString(childNode3.Data);
					break;
				case "2.5.4.3":
					CommonName = uTF8Encoding.GetString(childNode3.Data);
					break;
				case "2.5.4.5":
					SerialNumber = Asn1Util.ToHexString(childNode3.Data);
					break;
				case "2.5.4.46":
					Dnq = uTF8Encoding.GetString(childNode3.Data);
					break;
				case "2.5.4.8":
					State = uTF8Encoding.GetString(childNode3.Data);
					break;
				}
			}
		}

		public bool Equals(DistinguishedName n2)
		{
			return Organization == n2.Organization && OrganizationalUnit == n2.OrganizationalUnit && Dnq == n2.Dnq && Country == n2.Country && State == n2.State && CommonName == n2.CommonName;
		}

		public override string ToString()
		{
			return "CN: " + CommonName + "\nON: " + Organization + "\nUnit Name: " + OrganizationalUnit + "\nCountry: " + Country;
		}
	}
}
