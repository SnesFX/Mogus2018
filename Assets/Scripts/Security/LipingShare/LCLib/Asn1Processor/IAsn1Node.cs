using System.IO;

namespace LipingShare.LCLib.Asn1Processor
{
	internal interface IAsn1Node
	{
		Asn1Node ParentNode { get; }

		long ChildNodeCount { get; }

		byte Tag { get; set; }

		byte MaskedTag { get; }

		string TagName { get; }

		long DataLength { get; }

		long LengthFieldBytes { get; }

		long DataOffset { get; }

		byte UnusedBits { get; }

		byte[] Data { get; set; }

		bool ParseEncapsulatedData { get; set; }

		long Deepness { get; }

		string Path { get; }

		bool LoadData(Stream xdata);

		bool SaveData(Stream xdata);

		void AddChild(Asn1Node xdata);

		int InsertChild(Asn1Node xdata, int index);

		int InsertChild(Asn1Node xdata, Asn1Node indexNode);

		int InsertChildAfter(Asn1Node xdata, int index);

		int InsertChildAfter(Asn1Node xdata, Asn1Node indexNode);

		Asn1Node RemoveChild(int index);

		Asn1Node RemoveChild(Asn1Node node);

		Asn1Node GetChildNode(int index);

		Asn1Node GetDescendantNodeByPath(string nodePath);

		string GetText(Asn1Node startNode, int lineLen);

		string GetDataStr(bool pureHexMode);

		string GetLabel(uint mask);

		Asn1Node Clone();

		void ClearAll();
	}
}
