using System;
using System.Collections;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixServerTransportSink : IServerChannelSink, IChannelSinkBase
	{
		private IServerChannelSink next_sink;

		public IServerChannelSink NextChannelSink
		{
			get
			{
				return next_sink;
			}
		}

		public IDictionary Properties
		{
			get
			{
				if (next_sink != null)
				{
					return next_sink.Properties;
				}
				return null;
			}
		}

		public UnixServerTransportSink(IServerChannelSink next)
		{
			next_sink = next;
		}

		public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers, Stream responseStream)
		{
			ClientConnection clientConnection = (ClientConnection)state;
			NetworkStream networkStream = new NetworkStream(clientConnection.Client);
			UnixMessageIO.SendMessageStream(networkStream, responseStream, headers, clientConnection.Buffer);
			networkStream.Flush();
			networkStream.Close();
		}

		public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, object state, IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack, IMessage requestMsg, ITransportHeaders requestHeaders, Stream requestStream, out IMessage responseMsg, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			throw new NotSupportedException();
		}

		internal void InternalProcessMessage(ClientConnection connection, Stream stream)
		{
			ITransportHeaders headers;
			Stream requestStream = UnixMessageIO.ReceiveMessageStream(stream, out headers, connection.Buffer);
			ServerChannelSinkStack serverChannelSinkStack = new ServerChannelSinkStack();
			serverChannelSinkStack.Push(this, connection);
			IMessage responseMsg;
			ITransportHeaders responseHeaders;
			Stream responseStream;
			ServerProcessing serverProcessing = next_sink.ProcessMessage(serverChannelSinkStack, null, headers, requestStream, out responseMsg, out responseHeaders, out responseStream);
			if (serverProcessing != 0)
			{
				ServerProcessing serverProcessing2 = serverProcessing - 1;
				int num = 1;
			}
			else
			{
				UnixMessageIO.SendMessageStream(stream, responseStream, responseHeaders, connection.Buffer);
				stream.Flush();
			}
		}
	}
}
