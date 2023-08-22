using System;
using System.Net.Sockets;

namespace Mono.Unix
{
	public class UnixClient : MarshalByRefObject, IDisposable
	{
		private NetworkStream stream;

		private Socket client;

		private bool disposed;

		public Socket Client
		{
			get
			{
				return client;
			}
			set
			{
				client = value;
				stream = null;
			}
		}

		public PeerCred PeerCredential
		{
			get
			{
				CheckDisposed();
				return new PeerCred(client);
			}
		}

		public LingerOption LingerState
		{
			get
			{
				CheckDisposed();
				return (LingerOption)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger);
			}
			set
			{
				CheckDisposed();
				client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Linger, value);
			}
		}

		public int ReceiveBufferSize
		{
			get
			{
				CheckDisposed();
				return (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
			}
			set
			{
				CheckDisposed();
				client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, value);
			}
		}

		public int ReceiveTimeout
		{
			get
			{
				CheckDisposed();
				return (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout);
			}
			set
			{
				CheckDisposed();
				client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, value);
			}
		}

		public int SendBufferSize
		{
			get
			{
				CheckDisposed();
				return (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer);
			}
			set
			{
				CheckDisposed();
				client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, value);
			}
		}

		public int SendTimeout
		{
			get
			{
				CheckDisposed();
				return (int)client.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout);
			}
			set
			{
				CheckDisposed();
				client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, value);
			}
		}

		public UnixClient()
		{
			if (client != null)
			{
				client.Close();
				client = null;
			}
			client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
		}

		public UnixClient(string path)
			: this()
		{
			if (path == null)
			{
				throw new ArgumentNullException("ep");
			}
			Connect(path);
		}

		public UnixClient(UnixEndPoint ep)
			: this()
		{
			if (ep == null)
			{
				throw new ArgumentNullException("ep");
			}
			Connect(ep);
		}

		internal UnixClient(Socket sock)
		{
			Client = sock;
		}

		public void Close()
		{
			CheckDisposed();
			Dispose();
		}

		public void Connect(UnixEndPoint remoteEndPoint)
		{
			CheckDisposed();
			client.Connect(remoteEndPoint);
			stream = new NetworkStream(client, true);
		}

		public void Connect(string path)
		{
			CheckDisposed();
			Connect(new UnixEndPoint(path));
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposed)
			{
				return;
			}
			if (disposing)
			{
				NetworkStream networkStream = stream;
				stream = null;
				if (networkStream != null)
				{
					networkStream.Close();
					networkStream = null;
				}
				else if (client != null)
				{
					client.Close();
				}
				client = null;
			}
			disposed = true;
		}

		public NetworkStream GetStream()
		{
			CheckDisposed();
			if (stream == null)
			{
				stream = new NetworkStream(client, true);
			}
			return stream;
		}

		private void CheckDisposed()
		{
			if (disposed)
			{
				throw new ObjectDisposedException(GetType().FullName);
			}
		}

		~UnixClient()
		{
			Dispose(false);
		}
	}
}
