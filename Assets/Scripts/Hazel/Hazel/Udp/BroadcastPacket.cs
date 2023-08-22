using System;
using System.Net;

namespace Hazel.Udp
{
	public class BroadcastPacket
	{
		public string Data;

		public DateTime ReceiveTime;

		public IPEndPoint Sender;

		public BroadcastPacket(string data, IPEndPoint sender)
		{
			Data = data;
			Sender = sender;
			ReceiveTime = DateTime.Now;
		}

		public string GetAddress()
		{
			return Sender.Address.ToString();
		}
	}
}
