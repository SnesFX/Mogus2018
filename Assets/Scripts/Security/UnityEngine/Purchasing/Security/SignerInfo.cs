using LipingShare.LCLib.Asn1Processor;

namespace UnityEngine.Purchasing.Security
{
	internal class SignerInfo
	{
		public int Version { get; private set; }

		public string IssuerSerialNumber { get; private set; }

		public byte[] EncryptedDigest { get; private set; }

		public SignerInfo(Asn1Node n)
		{
			if (n.ChildNodeCount != 5)
			{
				throw new InvalidPKCS7Data();
			}
			Asn1Node childNode = n.GetChildNode(0);
			if ((childNode.Tag & 0x1F) != 2)
			{
				throw new InvalidPKCS7Data();
			}
			Version = childNode.Data[0];
			if (Version != 1 || childNode.Data.Length != 1)
			{
				throw new UnsupportedSignerInfoVersion();
			}
			childNode = n.GetChildNode(1);
			if ((childNode.Tag & 0x1F) != 16 || childNode.ChildNodeCount != 2)
			{
				throw new InvalidPKCS7Data();
			}
			childNode = childNode.GetChildNode(1);
			if ((childNode.Tag & 0x1F) != 2)
			{
				throw new InvalidPKCS7Data();
			}
			IssuerSerialNumber = Asn1Util.ToHexString(childNode.Data);
			childNode = n.GetChildNode(4);
			if ((childNode.Tag & 0x1F) != 4)
			{
				throw new InvalidPKCS7Data();
			}
			EncryptedDigest = childNode.Data;
		}
	}
}
