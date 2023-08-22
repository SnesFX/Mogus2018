using System;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Threading;
using Mono.Unix;
using Mono.Unix.Native;

namespace Mono.Remoting.Channels.Unix
{
	public class UnixServerChannel : IChannelReceiver, IChannel
	{
		private string path;

		private string name = "unix";

		private int priority = 1;

		private bool supressChannelData;

		private Thread server_thread;

		private UnixListener listener;

		private UnixServerTransportSink sink;

		private ChannelDataStore channel_data;

		private int _maxConcurrentConnections = 100;

		private ArrayList _activeConnections = new ArrayList();

		public object ChannelData
		{
			get
			{
				if (supressChannelData)
				{
					return null;
				}
				return channel_data;
			}
		}

		public string ChannelName
		{
			get
			{
				return name;
			}
		}

		public int ChannelPriority
		{
			get
			{
				return priority;
			}
		}

		private void Init(IServerChannelSinkProvider serverSinkProvider)
		{
			if (serverSinkProvider == null)
			{
				serverSinkProvider = new UnixBinaryServerFormatterSinkProvider();
			}
			channel_data = new ChannelDataStore(null);
			for (IServerChannelSinkProvider serverChannelSinkProvider = serverSinkProvider; serverChannelSinkProvider != null; serverChannelSinkProvider = serverChannelSinkProvider.Next)
			{
				serverChannelSinkProvider.GetChannelData(channel_data);
			}
			IServerChannelSink next = ChannelServices.CreateServerChannelSinkChain(serverSinkProvider, this);
			sink = new UnixServerTransportSink(next);
			StartListening(null);
		}

		public UnixServerChannel(string path)
		{
			this.path = path;
			Init(null);
		}

		public UnixServerChannel(IDictionary properties, IServerChannelSinkProvider serverSinkProvider)
		{
			foreach (DictionaryEntry property in properties)
			{
				switch ((string)property.Key)
				{
				case "path":
					path = property.Value as string;
					break;
				case "priority":
					priority = Convert.ToInt32(property.Value);
					break;
				case "supressChannelData":
					supressChannelData = Convert.ToBoolean(property.Value);
					break;
				}
			}
			Init(serverSinkProvider);
		}

		public UnixServerChannel(string name, string path, IServerChannelSinkProvider serverSinkProvider)
		{
			this.name = name;
			this.path = path;
			Init(serverSinkProvider);
		}

		public UnixServerChannel(string name, string path)
		{
			this.name = name;
			this.path = path;
			Init(null);
		}

		public string GetChannelUri()
		{
			return "unix://" + path;
		}

		public string[] GetUrlsForUri(string uri)
		{
			if (!uri.StartsWith("/"))
			{
				uri = "/" + uri;
			}
			string[] channelUris = channel_data.ChannelUris;
			string[] array = new string[channelUris.Length];
			for (int i = 0; i < channelUris.Length; i++)
			{
				array[i] = channelUris[i] + "?" + uri;
			}
			return array;
		}

		public string Parse(string url, out string objectURI)
		{
			return UnixChannel.ParseUnixURL(url, out objectURI);
		}

		private void WaitForConnections()
		{
			try
			{
				while (true)
				{
					Socket client = listener.AcceptSocket();
					CreateListenerConnection(client);
				}
			}
			catch
			{
			}
		}

		internal void CreateListenerConnection(Socket client)
		{
			lock (_activeConnections)
			{
				if (_activeConnections.Count >= _maxConcurrentConnections)
				{
					Monitor.Wait(_activeConnections);
				}
				if (server_thread != null)
				{
					Thread thread = new Thread(new ClientConnection(this, client, sink).ProcessMessages);
					thread.Start();
					thread.IsBackground = true;
					_activeConnections.Add(thread);
				}
			}
		}

		internal void ReleaseConnection(Thread thread)
		{
			lock (_activeConnections)
			{
				_activeConnections.Remove(thread);
				Monitor.Pulse(_activeConnections);
			}
		}

		public void StartListening(object data)
		{
			listener = new UnixListener(path);
			Syscall.chmod(path, FilePermissions.DEFFILEMODE);
			if (server_thread == null)
			{
				listener.Start();
				string[] array = new string[1];
				array = new string[1] { GetChannelUri() };
				channel_data.ChannelUris = array;
				server_thread = new Thread(WaitForConnections);
				server_thread.IsBackground = true;
				server_thread.Start();
			}
		}

		public void StopListening(object data)
		{
			if (server_thread == null)
			{
				return;
			}
			lock (_activeConnections)
			{
				server_thread.Abort();
				server_thread = null;
				listener.Stop();
				foreach (Thread activeConnection in _activeConnections)
				{
					activeConnection.Abort();
				}
				_activeConnections.Clear();
				Monitor.PulseAll(_activeConnections);
			}
		}
	}
}
