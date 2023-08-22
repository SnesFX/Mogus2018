using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Hazel;
using Hazel.Udp;
using UnityEngine;

namespace InnerNet
{
	public class InnerNetServer : MonoBehaviour
	{
		protected class Player
		{
			private static int IdCount = 1;

			public int Id;

			public Connection Connection;

			public LimboStates LimboState;

			public Player(Connection connection)
			{
				Id = Interlocked.Increment(ref IdCount);
				Connection = connection;
			}
		}

		public const int MaxPlayers = 10;

		public static InnerNetServer Instance;

		public bool Running;

		public const int LocalGameId = 32;

		private const int InvalidHost = -1;

		private int HostId = -1;

		public HashSet<string> ipBans = new HashSet<string>();

		public int Port = 22023;

		[SerializeField]
		private GameStates GameState;

		private ConnectionListener listener;

		private List<Player> Clients = new List<Player>();

		public void Awake()
		{
			if ((bool)Instance)
			{
				if (Instance != this)
				{
					UnityEngine.Object.Destroy(base.gameObject);
				}
			}
			else
			{
				Instance = this;
			}
		}

		public void OnDestroy()
		{
			StopServer();
		}

		public void StartAsServer()
		{
			if (listener != null)
			{
				StopServer();
			}
			GameState = GameStates.NotStarted;
			listener = new UdpConnectionListener(new NetworkEndPoint(IPAddress.Any, Port));
			listener.NewConnection += OnServerConnect;
			listener.Start();
			Running = true;
		}

		public void StopServer()
		{
			HostId = -1;
			Running = false;
			GameState = GameStates.Destroyed;
			if (listener != null)
			{
				listener.Close();
				listener.Dispose();
				listener = null;
			}
			lock (Clients)
			{
				Clients.Clear();
			}
		}

		private void OnServerConnect(object sender, NewConnectionEventArgs evt)
		{
			if (evt.HandshakeData.Length >= 5)
			{
				MessageReader messageReader = MessageReader.Get(evt.HandshakeData);
				messageReader.ReadByte();
				int num = messageReader.ReadInt32();
				if (num != Constants.GetBroadcastVersion())
				{
					SendIncorrectVersion(evt.Connection);
					return;
				}
				Player client = new Player(evt.Connection);
				Debug.Log(string.Format("Client {0} added: {1}", client.Id, ((NetworkEndPoint)evt.Connection.EndPoint).EndPoint));
				evt.Connection.DataReceived += delegate(object o, DataReceivedEventArgs e)
				{
					OnDataReceived(client, e);
				};
				evt.Connection.Disconnected += delegate
				{
					ClientDisconnect(client);
				};
			}
			else
			{
				SendIncorrectVersion(evt.Connection);
			}
		}

		private static void SendIncorrectVersion(Connection connection)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(1);
			messageWriter.Write(5);
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		private void Connection_DataSentRaw(byte[] data, int length)
		{
			Debug.Log("Server Sent: " + string.Join(" ", data.Select((byte b) => b.ToString()).ToArray(), 0, length));
		}

		private void OnDataReceived(Player client, DataReceivedEventArgs evt)
		{
			if (evt.Bytes.Length <= 0)
			{
				Debug.Log("Server got 0 bytes");
				return;
			}
			try
			{
				MessageReader messageReader = MessageReader.Get(evt.Bytes);
				while (messageReader.Position < messageReader.Length)
				{
					MessageReader messageReader2 = messageReader.ReadMessage();
					switch (messageReader2.Tag)
					{
					case 1:
					{
						Debug.Log("Server got join game");
						int num2 = messageReader2.ReadInt32();
						if (num2 == 32)
						{
							JoinGame(client);
							break;
						}
						MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
						messageWriter.StartMessage(1);
						messageWriter.Write((byte)3);
						messageWriter.EndMessage();
						client.Connection.Send(messageWriter);
						messageWriter.Recycle();
						break;
					}
					case 0:
					{
						Debug.Log("Server got host game");
						MessageWriter messageWriter3 = MessageWriter.Get(SendOption.Reliable);
						messageWriter3.StartMessage(0);
						messageWriter3.Write(32);
						messageWriter3.EndMessage();
						client.Connection.Send(messageWriter3);
						messageWriter3.Recycle();
						break;
					}
					case 3:
					{
						int num4 = messageReader2.ReadInt32();
						if (num4 == 32)
						{
							ClientDisconnect(client);
						}
						break;
					}
					case 2:
					{
						int num5 = messageReader2.ReadInt32();
						if (num5 == 32)
						{
							StartGame(evt.Bytes, client);
						}
						break;
					}
					case 8:
					{
						int num6 = messageReader2.ReadInt32();
						if (num6 == 32)
						{
							EndGame(evt.Bytes, client);
						}
						break;
					}
					case 6:
						if (Clients.Contains(client))
						{
							int num7 = messageReader2.ReadInt32();
							if (num7 == 32)
							{
								int targetId = messageReader2.ReadPackedInt32();
								MessageWriter messageWriter4 = MessageWriter.Get(evt.SendOption);
								messageWriter4.Write(evt.Bytes);
								SendTo(messageWriter4, targetId);
								messageWriter4.Recycle();
							}
						}
						else if (GameState == GameStates.Started)
						{
							Debug.Log("GameDataTo: Server didn't have client");
							client.Connection.SendDisconnect();
						}
						break;
					case 5:
						if (Clients.Contains(client))
						{
							int num3 = messageReader2.ReadInt32();
							if (num3 == 32)
							{
								MessageWriter messageWriter2 = MessageWriter.Get(evt.SendOption);
								messageWriter2.Write(evt.Bytes);
								Broadcast(messageWriter2, client);
								messageWriter2.Recycle();
							}
						}
						else if (GameState == GameStates.Started)
						{
							client.Connection.SendDisconnect();
						}
						break;
					case 11:
					{
						int num = messageReader2.ReadInt32();
						if (num == 32)
						{
							KickPlayer(messageReader2.ReadPackedInt32(), messageReader2.ReadBoolean());
						}
						break;
					}
					}
				}
			}
			catch (Exception arg)
			{
				Debug.Log(string.Format("{0}\r\n{1}", string.Join(" ", evt.Bytes), arg));
			}
		}

		private void KickPlayer(int targetId, bool ban)
		{
			lock (Clients)
			{
				Player player = null;
				for (int i = 0; i < Clients.Count; i++)
				{
					if (Clients[i].Id == targetId)
					{
						player = Clients[i];
						break;
					}
				}
				if (player == null)
				{
					return;
				}
				if (ban)
				{
					lock (ipBans)
					{
						IPEndPoint iPEndPoint = (IPEndPoint)((NetworkEndPoint)player.Connection.EndPoint).EndPoint;
						ipBans.Add(iPEndPoint.Address.ToString());
					}
				}
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(11);
				messageWriter.Write(32);
				messageWriter.WritePacked(targetId);
				messageWriter.Write(ban);
				messageWriter.EndMessage();
				Broadcast(messageWriter, null);
				messageWriter.Recycle();
			}
		}

		protected void JoinGame(Player client)
		{
			lock (ipBans)
			{
				IPEndPoint iPEndPoint = (IPEndPoint)((NetworkEndPoint)client.Connection.EndPoint).EndPoint;
				if (ipBans.Contains(iPEndPoint.Address.ToString()))
				{
					MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
					messageWriter.StartMessage(1);
					messageWriter.Write(6);
					messageWriter.EndMessage();
					client.Connection.Send(messageWriter);
					messageWriter.Recycle();
					return;
				}
			}
			lock (Clients)
			{
				switch (GameState)
				{
				case GameStates.NotStarted:
					HandleNewGameJoin(client);
					return;
				case GameStates.Ended:
					HandleRejoin(client);
					return;
				}
				MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
				messageWriter2.StartMessage(1);
				messageWriter2.Write(2);
				messageWriter2.EndMessage();
				client.Connection.Send(messageWriter2);
				messageWriter2.Recycle();
			}
		}

		private void HandleRejoin(Player client)
		{
			if (client.Id == HostId)
			{
				GameState = GameStates.NotStarted;
				HandleNewGameJoin(client);
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				for (int i = 0; i < Clients.Count; i++)
				{
					Player player = Clients[i];
					if (player != client)
					{
						try
						{
							WriteJoinedMessage(player, messageWriter, true);
							player.Connection.Send(messageWriter);
						}
						catch
						{
						}
					}
				}
				messageWriter.Recycle();
				return;
			}
			if (Clients.Count >= 9)
			{
				MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
				messageWriter2.StartMessage(1);
				messageWriter2.Write(1);
				messageWriter2.EndMessage();
				client.Connection.Send(messageWriter2);
				messageWriter2.Recycle();
				return;
			}
			Clients.Add(client);
			client.LimboState = LimboStates.WaitingForHost;
			MessageWriter messageWriter3 = MessageWriter.Get(SendOption.Reliable);
			try
			{
				messageWriter3.StartMessage(12);
				messageWriter3.Write(32);
				messageWriter3.Write(client.Id);
				messageWriter3.EndMessage();
				client.Connection.Send(messageWriter3);
				BroadcastJoinMessage(client, messageWriter3);
			}
			catch
			{
			}
			finally
			{
				messageWriter3.Recycle();
			}
		}

		private void HandleNewGameJoin(Player client)
		{
			if (Clients.Count >= 10)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				try
				{
					messageWriter.StartMessage(1);
					messageWriter.Write(1);
					messageWriter.EndMessage();
					client.Connection.Send(messageWriter);
					return;
				}
				catch
				{
					return;
				}
				finally
				{
					messageWriter.Recycle();
				}
			}
			Clients.Add(client);
			client.LimboState = LimboStates.PreSpawn;
			if (HostId == -1)
			{
				HostId = Clients[0].Id;
			}
			if (HostId == client.Id)
			{
				client.LimboState = LimboStates.NotLimbo;
			}
			MessageWriter messageWriter2 = MessageWriter.Get(SendOption.Reliable);
			try
			{
				WriteJoinedMessage(client, messageWriter2, true);
				client.Connection.Send(messageWriter2);
				BroadcastJoinMessage(client, messageWriter2);
			}
			catch
			{
			}
			finally
			{
				messageWriter2.Recycle();
			}
		}

		private void EndGame(byte[] bytes, Player source)
		{
			if (source.Id == HostId)
			{
				GameState = GameStates.Ended;
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.Write(bytes);
				Broadcast(messageWriter, null);
				messageWriter.Recycle();
				lock (Clients)
				{
					Clients.Clear();
					return;
				}
			}
			Debug.LogWarning("Reset request rejected from: " + source.Id);
		}

		private void StartGame(byte[] bytes, Player source)
		{
			GameState = GameStates.Started;
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.Write(bytes);
			Broadcast(messageWriter, null);
			messageWriter.Recycle();
		}

		private void ClientDisconnect(Player client)
		{
			Debug.Log("Server DC client " + client.Id);
			lock (Clients)
			{
				Clients.Remove(client);
				client.Connection.Dispose();
				if (Clients.Count > 0)
				{
					HostId = Clients[0].Id;
				}
			}
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(4);
			messageWriter.Write(32);
			messageWriter.Write(client.Id);
			messageWriter.Write(HostId);
			messageWriter.EndMessage();
			Broadcast(messageWriter, null);
			messageWriter.Recycle();
		}

		protected void SendTo(MessageWriter msg, int targetId)
		{
			lock (Clients)
			{
				for (int i = 0; i < Clients.Count; i++)
				{
					Player player = Clients[i];
					if (player.Id == targetId)
					{
						try
						{
							player.Connection.Send(msg);
							break;
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
							break;
						}
					}
				}
			}
		}

		protected void Broadcast(MessageWriter msg, Player source)
		{
			lock (Clients)
			{
				for (int i = 0; i < Clients.Count; i++)
				{
					Player player = Clients[i];
					if (player != source)
					{
						try
						{
							player.Connection.Send(msg);
						}
						catch (Exception exception)
						{
							Debug.LogException(exception);
						}
					}
				}
			}
		}

		private void BroadcastJoinMessage(Player client, MessageWriter msg)
		{
			msg.Clear(SendOption.Reliable);
			msg.StartMessage(1);
			msg.Write(32);
			msg.Write(client.Id);
			msg.Write(HostId);
			msg.EndMessage();
			Broadcast(msg, client);
		}

		private void WriteJoinedMessage(Player client, MessageWriter msg, bool clear)
		{
			if (clear)
			{
				msg.Clear(SendOption.Reliable);
			}
			msg.StartMessage(7);
			msg.Write(32);
			msg.Write(client.Id);
			msg.Write(HostId);
			msg.WritePacked(Clients.Count - 1);
			for (int i = 0; i < Clients.Count; i++)
			{
				Player player = Clients[i];
				if (player != client)
				{
					msg.WritePacked(player.Id);
				}
			}
			msg.EndMessage();
		}
	}
}
