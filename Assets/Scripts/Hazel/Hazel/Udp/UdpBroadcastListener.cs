using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Hazel.Udp
{
	public class UdpBroadcastListener : IDisposable
	{
		private Socket socket;

		private EndPoint endpoint;

		private byte[] buffer = new byte[1024];

		private List<BroadcastPacket> packets = new List<BroadcastPacket>();

		public bool Running { get; private set; }

		public UdpBroadcastListener(int port)
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
			endpoint = new IPEndPoint(IPAddress.Any, port);
			socket.Bind(endpoint);
		}

		public void StartListen()
		{
			if (Running)
			{
				return;
			}
			Running = true;
			try
			{
				EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
				IAsyncResult asyncResult = socket.BeginReceiveFrom(buffer, 0, buffer.Length, SocketFlags.None, ref remoteEP, HandleData, null);
				if (asyncResult.CompletedSynchronously)
				{
					HandleData(asyncResult);
				}
			}
			catch
			{
				Dispose();
			}
		}

		private void HandleData(IAsyncResult result)
		{
			Running = false;
			EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);
			int num;
			try
			{
				num = socket.EndReceiveFrom(result, ref endPoint);
			}
			catch
			{
				Dispose();
				return;
			}
			if (num < 2 || buffer[0] != 4 || buffer[1] != 2)
			{
				StartListen();
				return;
			}
			IPEndPoint iPEndPoint = (IPEndPoint)endPoint;
			string @string = Encoding.ASCII.GetString(buffer, 2, num - 2);
			int hashCode = @string.GetHashCode();
			lock (packets)
			{
				bool flag = false;
				for (int i = 0; i < packets.Count; i++)
				{
					BroadcastPacket broadcastPacket = packets[i];
					if (broadcastPacket.Data.GetHashCode() == hashCode && broadcastPacket.Sender.Equals(iPEndPoint))
					{
						packets[i].ReceiveTime = DateTime.Now;
						break;
					}
				}
				if (!flag)
				{
					packets.Add(new BroadcastPacket(@string, iPEndPoint));
				}
			}
			StartListen();
		}

		public BroadcastPacket[] GetPackets()
		{
			lock (packets)
			{
				BroadcastPacket[] result = packets.ToArray();
				packets.Clear();
				return result;
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
