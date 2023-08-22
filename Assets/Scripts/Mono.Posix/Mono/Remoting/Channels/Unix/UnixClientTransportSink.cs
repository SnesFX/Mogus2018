using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Messaging;
using System.Threading;

namespace Mono.Remoting.Channels.Unix
{
	internal class UnixClientTransportSink : IClientChannelSink, IChannelSinkBase
	{
		private string _path;

		public IDictionary Properties
		{
			get
			{
				return null;
			}
		}

		public IClientChannelSink NextChannelSink
		{
			get
			{
				return null;
			}
		}

		public UnixClientTransportSink(string url)
		{
			string objectURI;
			_path = UnixChannel.ParseUnixURL(url, out objectURI);
		}

		public void AsyncProcessRequest(IClientChannelSinkStack sinkStack, IMessage msg, ITransportHeaders headers, Stream requestStream)
		{
			UnixConnection unixConnection = null;
			bool flag = RemotingServices.IsOneWay(((IMethodMessage)msg).MethodBase);
			try
			{
				if (headers == null)
				{
					headers = new TransportHeaders();
				}
				headers["__RequestUri"] = ((IMethodMessage)msg).Uri;
				unixConnection = UnixConnectionPool.GetConnection(_path);
				UnixMessageIO.SendMessageStream(unixConnection.Stream, requestStream, headers, unixConnection.Buffer);
				unixConnection.Stream.Flush();
				if (!flag)
				{
					sinkStack.Push(this, unixConnection);
					ThreadPool.QueueUserWorkItem(delegate(object data)
					{
						try
						{
							ReadAsyncUnixMessage(data);
						}
						catch
						{
						}
					}, sinkStack);
				}
				else
				{
					unixConnection.Release();
				}
			}
			catch
			{
				if (unixConnection != null)
				{
					unixConnection.Release();
				}
				if (!flag)
				{
					throw;
				}
			}
		}

		private void ReadAsyncUnixMessage(object data)
		{
			IClientChannelSinkStack clientChannelSinkStack = (IClientChannelSinkStack)data;
			UnixConnection unixConnection = (UnixConnection)clientChannelSinkStack.Pop(this);
			try
			{
				if (UnixMessageIO.ReceiveMessageStatus(unixConnection.Stream, unixConnection.Buffer) != 0)
				{
					throw new RemotingException("Unknown response message from server");
				}
				ITransportHeaders headers;
				Stream stream = UnixMessageIO.ReceiveMessageStream(unixConnection.Stream, out headers, unixConnection.Buffer);
				unixConnection.Release();
				unixConnection = null;
				clientChannelSinkStack.AsyncProcessResponse(headers, stream);
			}
			catch
			{
				if (unixConnection != null)
				{
					unixConnection.Release();
				}
				throw;
			}
		}

		public void AsyncProcessResponse(IClientResponseChannelSinkStack sinkStack, object state, ITransportHeaders headers, Stream stream)
		{
			throw new NotSupportedException();
		}

		public Stream GetRequestStream(IMessage msg, ITransportHeaders headers)
		{
			return null;
		}

		public void ProcessMessage(IMessage msg, ITransportHeaders requestHeaders, Stream requestStream, out ITransportHeaders responseHeaders, out Stream responseStream)
		{
			UnixConnection unixConnection = null;
			try
			{
				if (requestHeaders == null)
				{
					requestHeaders = new TransportHeaders();
				}
				requestHeaders["__RequestUri"] = ((IMethodMessage)msg).Uri;
				unixConnection = UnixConnectionPool.GetConnection(_path);
				UnixMessageIO.SendMessageStream(unixConnection.Stream, requestStream, requestHeaders, unixConnection.Buffer);
				unixConnection.Stream.Flush();
				if (UnixMessageIO.ReceiveMessageStatus(unixConnection.Stream, unixConnection.Buffer) != 0)
				{
					throw new RemotingException("Unknown response message from server");
				}
				responseStream = UnixMessageIO.ReceiveMessageStream(unixConnection.Stream, out responseHeaders, unixConnection.Buffer);
			}
			finally
			{
				if (unixConnection != null)
				{
					unixConnection.Release();
				}
			}
		}
	}
}
