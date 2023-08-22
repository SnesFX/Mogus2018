using System;
using System.IO;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixConnection
	{
		private DateTime _controlTime;

		private Stream _stream;

		private ReusableUnixClient _client;

		private HostConnectionPool _pool;

		private byte[] _buffer;

		public Stream Stream
		{
			get
			{
				return _stream;
			}
		}

		public DateTime ControlTime
		{
			get
			{
				return _controlTime;
			}
			set
			{
				_controlTime = value;
			}
		}

		public bool IsAlive
		{
			get
			{
				return _client.IsAlive;
			}
		}

		public byte[] Buffer
		{
			get
			{
				return _buffer;
			}
		}

		public UnixConnection(HostConnectionPool pool, ReusableUnixClient client)
		{
			_pool = pool;
			_client = client;
			_stream = new BufferedStream(client.GetStream());
			_controlTime = DateTime.UtcNow;
			_buffer = new byte[UnixMessageIO.DefaultStreamBufferSize];
		}

		public void Release()
		{
			_pool.ReleaseConnection(this);
		}

		public void Close()
		{
			_client.Close();
		}
	}
}
