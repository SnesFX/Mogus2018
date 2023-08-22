using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Threading;

namespace Mono.Remoting.Channels.Unix
{
	internal class HostConnectionPool
	{
		private ArrayList _pool = new ArrayList();

		private int _activeConnections;

		private string _path;

		public HostConnectionPool(string path)
		{
			_path = path;
		}

		public UnixConnection GetConnection()
		{
			UnixConnection unixConnection = null;
			lock (_pool)
			{
				do
				{
					if (_pool.Count > 0)
					{
						unixConnection = (UnixConnection)_pool[_pool.Count - 1];
						_pool.RemoveAt(_pool.Count - 1);
						if (!unixConnection.IsAlive)
						{
							CancelConnection(unixConnection);
							unixConnection = null;
							continue;
						}
					}
					if (unixConnection == null && _activeConnections < UnixConnectionPool.MaxOpenConnections)
					{
						break;
					}
					if (unixConnection == null)
					{
						Monitor.Wait(_pool);
					}
				}
				while (unixConnection == null);
			}
			if (unixConnection == null)
			{
				return CreateConnection();
			}
			return unixConnection;
		}

		private UnixConnection CreateConnection()
		{
			try
			{
				ReusableUnixClient client = new ReusableUnixClient(_path);
				UnixConnection result = new UnixConnection(this, client);
				_activeConnections++;
				return result;
			}
			catch (Exception ex)
			{
				throw new RemotingException(ex.Message);
			}
		}

		public void ReleaseConnection(UnixConnection entry)
		{
			lock (_pool)
			{
				entry.ControlTime = DateTime.UtcNow;
				_pool.Add(entry);
				Monitor.Pulse(_pool);
			}
		}

		private void CancelConnection(UnixConnection entry)
		{
			try
			{
				entry.Stream.Close();
				_activeConnections--;
			}
			catch
			{
			}
		}

		public void PurgeConnections()
		{
			lock (_pool)
			{
				for (int i = 0; i < _pool.Count; i++)
				{
					UnixConnection unixConnection = (UnixConnection)_pool[i];
					if ((DateTime.UtcNow - unixConnection.ControlTime).TotalSeconds > (double)UnixConnectionPool.KeepAliveSeconds)
					{
						CancelConnection(unixConnection);
						_pool.RemoveAt(i);
						i--;
					}
				}
			}
		}
	}
}
