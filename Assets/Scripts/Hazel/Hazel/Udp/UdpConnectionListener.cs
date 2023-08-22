using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Hazel.Udp
{
	public class UdpConnectionListener : NetworkConnectionListener
	{
		private Socket listener;

		private byte[] dataBuffer = new byte[65535];

		private Dictionary<EndPoint, UdpServerConnection> connections = new Dictionary<EndPoint, UdpServerConnection>();

		[Obsolete("Temporary constructor in beta only, use NetworkEndPoint constructor instead.")]
		public UdpConnectionListener(IPAddress IPAddress, int port, IPMode mode = IPMode.IPv4)
			: this(new NetworkEndPoint(IPAddress, port, mode))
		{
		}

		public UdpConnectionListener(NetworkEndPoint endPoint)
		{
			base.EndPoint = endPoint.EndPoint;
			base.IPMode = endPoint.IPMode;
			if (endPoint.IPMode == IPMode.IPv4)
			{
				listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				return;
			}
			if (!Socket.OSSupportsIPv6)
			{
				throw new HazelException("IPV6 not supported!");
			}
			listener = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			listener.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
		}

		~UdpConnectionListener()
		{
			Dispose(false);
		}

		public override void Start()
		{
			try
			{
				listener.Bind(base.EndPoint);
			}
			catch (SocketException e)
			{
				throw new HazelException("Could not start listening as a SocketException occured", e);
			}
			StartListeningForData();
		}

		private void StartListeningForData()
		{
			EndPoint remoteEP = base.EndPoint;
			try
			{
				listener.BeginReceiveFrom(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ref remoteEP, ReadCallback, dataBuffer);
			}
			catch (ObjectDisposedException)
			{
			}
			catch (SocketException)
			{
				StartListeningForData();
			}
		}

		private void ReadCallback(IAsyncResult result)
		{
			EndPoint endPoint = new IPEndPoint((base.IPMode == IPMode.IPv4) ? IPAddress.Any : IPAddress.IPv6Any, 0);
			int num;
			try
			{
				num = listener.EndReceiveFrom(result, ref endPoint);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (SocketException)
			{
				StartListeningForData();
				return;
			}
			if (num == 0)
			{
				return;
			}
			byte[] array = new byte[num];
			Buffer.BlockCopy((byte[])result.AsyncState, 0, array, 0, num);
			StartListeningForData();
			bool flag;
			UdpServerConnection udpServerConnection;
			lock (connections)
			{
				flag = connections.ContainsKey(endPoint);
				if (flag)
				{
					udpServerConnection = connections[endPoint];
				}
				else
				{
					if (array[0] != 8)
					{
						return;
					}
					udpServerConnection = new UdpServerConnection(this, endPoint, base.IPMode);
					connections.Add(endPoint, udpServerConnection);
				}
			}
			udpServerConnection.HandleReceive(array);
			if (!flag)
			{
				byte[] array2 = new byte[array.Length - 3];
				Buffer.BlockCopy(array, 3, array2, 0, array.Length - 3);
				InvokeNewConnection(array2, udpServerConnection);
			}
		}

		internal void SendData(byte[] bytes, int length, EndPoint endPoint)
		{
			if (length > bytes.Length)
			{
				return;
			}
			try
			{
				listener.BeginSendTo(bytes, 0, length, SocketFlags.None, endPoint, delegate(IAsyncResult result)
				{
					listener.EndSendTo(result);
				}, null);
			}
			catch (SocketException e)
			{
				throw new HazelException("Could not send data as a SocketException occured.", e);
			}
			catch (ObjectDisposedException)
			{
			}
		}

		internal void SendDataSync(byte[] bytes, int length, EndPoint endPoint)
		{
			try
			{
				listener.SendTo(bytes, 0, length, SocketFlags.None, endPoint);
			}
			catch (SocketException e)
			{
				throw new HazelException("Could not send data as a SocketException occured.", e);
			}
			catch (ObjectDisposedException)
			{
			}
		}

		internal void RemoveConnectionTo(EndPoint endPoint)
		{
			lock (connections)
			{
				connections.Remove(endPoint);
			}
		}

		protected override void Dispose(bool disposing)
		{
			lock (connections)
			{
				KeyValuePair<EndPoint, UdpServerConnection>[] array = connections.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					KeyValuePair<EndPoint, UdpServerConnection> keyValuePair = array[i];
					if (keyValuePair.Value.State == ConnectionState.Connected)
					{
						try
						{
							keyValuePair.Value.SendDisconnect();
						}
						catch
						{
						}
					}
					keyValuePair.Value.Dispose();
				}
				connections.Clear();
			}
			if (listener != null)
			{
				listener.Close();
				listener.Dispose();
				listener = null;
			}
			base.Dispose(disposing);
		}
	}
}
