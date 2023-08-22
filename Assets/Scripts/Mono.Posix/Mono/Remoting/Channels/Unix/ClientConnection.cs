using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;

namespace Mono.Remoting.Channels.Unix
{
	internal class ClientConnection
	{
		private Socket _client;

		private UnixServerTransportSink _sink;

		private Stream _stream;

		private UnixServerChannel _serverChannel;

		private byte[] _buffer = new byte[UnixMessageIO.DefaultStreamBufferSize];

		public Socket Client
		{
			get
			{
				return _client;
			}
		}

		public byte[] Buffer
		{
			get
			{
				return _buffer;
			}
		}

		public bool IsLocal
		{
			get
			{
				return true;
			}
		}

		public ClientConnection(UnixServerChannel serverChannel, Socket client, UnixServerTransportSink sink)
		{
			_serverChannel = serverChannel;
			_client = client;
			_sink = sink;
		}

		public void ProcessMessages()
		{
			byte[] buffer = new byte[256];
			_stream = new BufferedStream(new NetworkStream(_client));
			try
			{
				bool flag = false;
				while (!flag)
				{
					switch (UnixMessageIO.ReceiveMessageStatus(_stream, buffer))
					{
					case MessageStatus.MethodMessage:
						_sink.InternalProcessMessage(this, _stream);
						break;
					case MessageStatus.CancelSignal:
					case MessageStatus.Unknown:
						flag = true;
						break;
					}
				}
			}
			catch (Exception)
			{
			}
			finally
			{
				try
				{
					_serverChannel.ReleaseConnection(Thread.CurrentThread);
					_stream.Close();
					_client.Close();
				}
				catch
				{
				}
			}
		}
	}
}
