using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hazel.Udp
{
	public class UdpBroadcaster : IDisposable
	{
		private Socket socket;

		private byte[] data;

		private EndPoint endpoint;

		public UdpBroadcaster(int port)
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
			endpoint = new IPEndPoint(IPAddress.Broadcast, port);
		}

		public void SetData(string data)
		{
			int byteCount = Encoding.ASCII.GetByteCount(data);
			this.data = new byte[byteCount + 2];
			this.data[0] = 4;
			this.data[1] = 2;
			Encoding.ASCII.GetBytes(data, 0, data.Length, this.data, 2);
		}

		public void Broadcast()
		{
			if (data != null)
			{
				socket.BeginSendTo(data, 0, data.Length, SocketFlags.None, endpoint, delegate(IAsyncResult evt)
				{
					socket.EndSendTo(evt);
				}, null);
			}
		}

		public void Dispose()
		{
			if (socket != null)
			{
				socket.Close();
				socket = null;
			}
		}
	}
}
