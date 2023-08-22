using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Hazel.Udp
{
	public sealed class UdpClientConnection : UdpConnection
	{
		private Socket socket;

		private object stateLock = new object();

		private byte[] dataBuffer = new byte[65535];

		public UdpClientConnection(NetworkEndPoint remoteEndPoint)
		{
			base.EndPoint = remoteEndPoint;
			base.RemoteEndPoint = remoteEndPoint.EndPoint;
			base.IPMode = remoteEndPoint.IPMode;
			if (remoteEndPoint.IPMode == IPMode.IPv4)
			{
				socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				return;
			}
			if (!Socket.OSSupportsIPv6)
			{
				throw new HazelException("IPV6 not supported!");
			}
			socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Dgram, ProtocolType.Udp);
			socket.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, false);
		}

		~UdpClientConnection()
		{
			Dispose(false);
		}

		protected override void WriteBytesToConnection(byte[] bytes, int length)
		{
			InvokeDataSentRaw(bytes, length);
			lock (stateLock)
			{
				if (base.State != ConnectionState.Connected && base.State != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Could not send data as this Connection is not connected and is not connecting. Did you disconnect?");
				}
			}
			try
			{
				socket.BeginSendTo(bytes, 0, length, SocketFlags.None, base.RemoteEndPoint, delegate(IAsyncResult result)
				{
					try
					{
						lock (socket)
						{
							socket.EndSendTo(result);
						}
					}
					catch (ObjectDisposedException e4)
					{
						HandleDisconnect(new HazelException("Could not send as the socket was disposed of.", e4));
					}
					catch (SocketException e5)
					{
						HandleDisconnect(new HazelException("Could not send data as a SocketException occured.", e5));
					}
				}, null);
			}
			catch (ObjectDisposedException)
			{
				throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
			}
			catch (SocketException e)
			{
				HazelException ex2 = new HazelException("Could not send data as a SocketException occured.", e);
				HandleDisconnect(ex2);
				throw ex2;
			}
			catch (ArgumentOutOfRangeException e2)
			{
				HazelException e3 = new HazelException("Something wonk with the buffer: " + bytes.Length, e2);
				HandleDisconnect(e3);
			}
		}

		protected override void WriteBytesToConnectionSync(byte[] bytes, int length)
		{
			InvokeDataSentRaw(bytes, length);
			lock (stateLock)
			{
				if (base.State != ConnectionState.Connected && base.State != ConnectionState.Connecting)
				{
					throw new InvalidOperationException("Could not send data as this Connection is not connected and is not connecting. Did you disconnect?");
				}
			}
			try
			{
				socket.SendTo(bytes, 0, length, SocketFlags.None, base.RemoteEndPoint);
			}
			catch (ObjectDisposedException)
			{
				throw new InvalidOperationException("Could not send data as this Connection is not connected. Did you disconnect?");
			}
			catch (SocketException e)
			{
				HazelException ex2 = new HazelException("Could not send data as a SocketException occured.", e);
				HandleDisconnect(ex2);
				throw ex2;
			}
		}

		public override void Connect(byte[] bytes = null, int timeout = 5000)
		{
			ConnectAsync(bytes, timeout);
			if (!WaitOnConnect(timeout))
			{
				Dispose();
				throw new HazelException("Connection attempt timed out.");
			}
		}

		public override void ConnectAsync(byte[] bytes = null, int timeout = 5000)
		{
			lock (stateLock)
			{
				if (base.State != 0)
				{
					throw new InvalidOperationException("Cannot connect as the Connection is already connected.");
				}
				base.State = ConnectionState.Connecting;
			}
			try
			{
				if (base.IPMode == IPMode.IPv4)
				{
					socket.Bind(new IPEndPoint(IPAddress.Any, 0));
				}
				else
				{
					socket.Bind(new IPEndPoint(IPAddress.IPv6Any, 0));
				}
			}
			catch (SocketException e)
			{
				base.State = ConnectionState.NotConnected;
				throw new HazelException("A socket exception occured while binding to the port.", e);
			}
			try
			{
				StartListeningForData();
			}
			catch (ObjectDisposedException)
			{
				lock (stateLock)
				{
					base.State = ConnectionState.NotConnected;
					return;
				}
			}
			catch (SocketException e2)
			{
				Dispose();
				throw new HazelException("A Socket exception occured while initiating a receive operation.", e2);
			}
			SendHello(bytes, delegate
			{
				lock (stateLock)
				{
					base.State = ConnectionState.Connected;
				}
			});
		}

		private void StartListeningForData()
		{
			socket.BeginReceive(dataBuffer, 0, dataBuffer.Length, SocketFlags.None, ReadCallback, dataBuffer);
		}

		private void ReadCallback(IAsyncResult result)
		{
			int num;
			try
			{
				num = socket.EndReceive(result);
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			catch (SocketException e)
			{
				HandleDisconnect(new HazelException("A socket exception occured while reading data.", e));
				return;
			}
			if (num == 0)
			{
				HandleDisconnect(new HazelException("Recieved 0 bytes"));
				return;
			}
			byte[] array = new byte[num];
			Buffer.BlockCopy(dataBuffer, 0, array, 0, num);
			try
			{
				StartListeningForData();
			}
			catch (SocketException e2)
			{
				HandleDisconnect(new HazelException("A Socket exception occured while initiating a receive operation.", e2));
			}
			catch (ObjectDisposedException)
			{
				return;
			}
			if (TestLagMs > 0)
			{
				Thread.Sleep(TestLagMs);
			}
			HandleReceive(array);
		}

		protected override void HandleDisconnect(HazelException e = null)
		{
			bool flag = false;
			lock (stateLock)
			{
				if (base.State == ConnectionState.Connected)
				{
					base.State = ConnectionState.Disconnecting;
					flag = true;
				}
			}
			if (flag)
			{
				try
				{
					InvokeDisconnected(e);
				}
				catch
				{
				}
				Dispose();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				bool flag;
				lock (stateLock)
				{
					flag = base.State == ConnectionState.Connected;
				}
				if (flag)
				{
					SendDisconnect();
				}
				lock (stateLock)
				{
					base.State = ConnectionState.NotConnected;
				}
			}
			if (socket != null)
			{
				socket.Close();
				socket.Dispose();
				socket = null;
			}
			base.Dispose(disposing);
		}
	}
}
