using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Hazel;
using Hazel.Udp;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace InnerNet
{
	public abstract class InnerNetClient : MonoBehaviour
	{
		private static readonly DisconnectReasons[] disconnectReasons = new DisconnectReasons[7]
		{
			DisconnectReasons.Error,
			DisconnectReasons.GameFull,
			DisconnectReasons.GameStarted,
			DisconnectReasons.GameNotFound,
			DisconnectReasons.IncorrectVersion,
			DisconnectReasons.Banned,
			DisconnectReasons.Kicked
		};

		public string NetworkAddress = "127.0.0.1";

		public int Port;

		[Range(-1f, 1000f)]
		public int TestLagMs = -1;

		private UdpClientConnection connection;

		public MatchMakerModes mode;

		public int GameId = 32;

		public int HostId;

		public int ClientId = -1;

		public List<ClientData> allClients = new List<ClientData>();

		public DisconnectReasons LastDisconnectReason;

		private readonly List<Action> DispatchQueue = new List<Action>();

		public const int CurrentClient = -3;

		public const int InvalidClient = -2;

		internal const byte DataFlag = 1;

		internal const byte RpcFlag = 2;

		internal const byte SpawnFlag = 4;

		internal const byte DespawnFlag = 5;

		internal const byte SceneChangeFlag = 6;

		public float MinSendInterval = 0.1f;

		private uint NetIdCnt = 1u;

		private float timer;

		public InnerNetObject[] SpawnableObjects;

		public List<InnerNetObject> allObjects = new List<InnerNetObject>();

		private Dictionary<uint, InnerNetObject> allObjectsFast = new Dictionary<uint, InnerNetObject>();

		private MessageWriter[] Streams;

		public int Ping
		{
			get
			{
				return (connection != null) ? ((int)connection.AveragePingMs) : 0;
			}
		}

		public bool AmHost
		{
			get
			{
				return HostId == ClientId;
			}
		}

		public bool AmClient
		{
			get
			{
				return ClientId > 0;
			}
		}

		public bool IsGamePublic { get; private set; }

		public virtual void Start()
		{
			SceneManager.activeSceneChanged += delegate(Scene oldScene, Scene scene)
			{
				SendSceneChange(scene.name);
			};
			ClientId = -1;
			GameId = 32;
		}

		public ClientData GetHost()
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				if (allClients[i].Id == HostId)
				{
					return allClients[i];
				}
			}
			return null;
		}

		public int GetClientIdFromCharacter(InnerNetObject character)
		{
			for (int i = 0; i < allClients.Count; i++)
			{
				if (allClients[i].Character == character)
				{
					return allClients[i].Id;
				}
			}
			return -1;
		}

		public virtual void OnDestroy()
		{
			Disconnect(DisconnectReasons.Destroy);
		}

		public IEnumerator CoConnect()
		{
			if (connection == null)
			{
				LastDisconnectReason = DisconnectReasons.ExitGame;
				connection = new UdpClientConnection(new NetworkEndPoint(NetworkAddress, Port));
				connection.KeepAliveInterval = 1500;
				connection.DisconnectTimeout = 5000;
				connection.DataReceived += OnMessageReceived;
				connection.Disconnected += OnDisconnect;
				connection.ConnectAsync(GetConnectionData());
				yield return WaitWithTimeout(() => connection == null || connection.State == ConnectionState.Connected);
			}
		}

		public IEnumerator CoConnect(MatchMakerModes mode)
		{
			if (this.mode != 0)
			{
				Disconnect(DisconnectReasons.NewConnection);
			}
			this.mode = mode;
			yield return CoConnect();
			if (connection == null)
			{
				yield break;
			}
			switch (this.mode)
			{
			case MatchMakerModes.Client:
				JoinGame();
				yield return WaitWithTimeout(() => ClientId >= 0);
				if (connection != null)
				{
				}
				break;
			case MatchMakerModes.Host:
				GameId = 0;
				HostGame();
				yield return WaitWithTimeout(() => GameId != 0);
				if (connection != null)
				{
				}
				break;
			case MatchMakerModes.Both:
				GameId = 0;
				HostGame();
				yield return WaitWithTimeout(() => GameId != 0);
				if (connection != null)
				{
					JoinGame();
					yield return WaitWithTimeout(() => ClientId >= 0);
					if (connection != null)
					{
					}
				}
				break;
			}
		}

		private void Connection_DataSentRaw(byte[] data, int length)
		{
			Debug.Log("Client Sent: " + string.Join(" ", data.Select((byte b) => b.ToString()).ToArray(), 0, length));
		}

		private IEnumerator WaitWithTimeout(Func<bool> success)
		{
			bool failed = true;
			for (float timer = 0f; timer < 5f; timer += Time.deltaTime)
			{
				if (success())
				{
					failed = false;
					break;
				}
				if (connection == null)
				{
					yield break;
				}
				yield return null;
			}
			if (failed)
			{
				Disconnect(DisconnectReasons.Error);
			}
		}

		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Return) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt)))
			{
				ResolutionManager.ToggleFullscreen();
			}
			if (DispatchQueue.Count <= 0)
			{
				return;
			}
			lock (DispatchQueue)
			{
				for (int i = 0; i < DispatchQueue.Count; i++)
				{
					try
					{
						DispatchQueue[i]();
					}
					catch (Exception exception)
					{
						Debug.LogException(exception);
						DispatchQueue.RemoveAt(i);
						i--;
					}
				}
				DispatchQueue.Clear();
			}
		}

		private void OnDisconnect(object sender, DisconnectedEventArgs e)
		{
			if (e.Exception != null)
			{
				Debug.LogException(e.Exception);
			}
			Disconnect(DisconnectReasons.Error);
			lock (DispatchQueue)
			{
				DispatchQueue.Clear();
				DispatchQueue.Add(delegate
				{
					OnDisconnected();
				});
			}
		}

		public void Disconnect(DisconnectReasons reason)
		{
			lock (DispatchQueue)
			{
				DispatchQueue.Clear();
			}
			allObjects.Clear();
			allClients.Clear();
			allObjectsFast.Clear();
			LastDisconnectReason = reason;
			if (mode == MatchMakerModes.Host || mode == MatchMakerModes.Both)
			{
				GameId = 0;
			}
			if (mode == MatchMakerModes.Client || mode == MatchMakerModes.Both)
			{
				ClientId = -1;
			}
			mode = MatchMakerModes.None;
			if (connection != null)
			{
				Debug.Log(string.Format("Client DC because {0}", reason));
				connection.Dispose();
				connection = null;
			}
			if ((bool)InnerNetServer.Instance)
			{
				InnerNetServer.Instance.StopServer();
			}
		}

		public void HostGame()
		{
			IsGamePublic = false;
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(0);
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
			Debug.Log("Client requesting new game.");
		}

		public void JoinGame()
		{
			ClientId = -1;
			if (connection == null)
			{
				Disconnect(DisconnectReasons.Error);
				return;
			}
			Debug.Log("Client joining game: " + IntToGameName(GameId));
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(1);
			messageWriter.Write(GameId);
			messageWriter.EndMessage();
			try
			{
				connection.Send(messageWriter);
			}
			catch
			{
				return;
			}
			messageWriter.Recycle();
			if (Streams == null)
			{
				Streams = new MessageWriter[2];
				for (int i = 0; i < Streams.Length; i++)
				{
					Streams[i] = MessageWriter.Get((SendOption)i);
				}
			}
			for (int j = 0; j < Streams.Length; j++)
			{
				MessageWriter messageWriter2 = Streams[j];
				messageWriter2.Clear((SendOption)j);
				messageWriter2.StartMessage(5);
				messageWriter2.Write(GameId);
			}
		}

		public void KickPlayer(int clientId, bool ban)
		{
			if (AmHost)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(11);
				messageWriter.Write(GameId);
				messageWriter.WritePacked(clientId);
				messageWriter.Write(ban);
				messageWriter.EndMessage();
				connection.Send(messageWriter);
				messageWriter.Recycle();
			}
		}

		public MessageWriter StartEndGame()
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(8);
			messageWriter.Write(GameId);
			return messageWriter;
		}

		public void FinishEndGame(MessageWriter msg)
		{
			msg.EndMessage();
			connection.Send(msg);
			msg.Recycle();
		}

		protected void LockGame()
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(2);
			messageWriter.Write(GameId);
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		public void RequestGameList(bool includePrivate)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(9);
			messageWriter.Write(includePrivate);
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		public void ChangeGamePublic(bool isPublic)
		{
			if (AmHost)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(10);
				messageWriter.Write(GameId);
				messageWriter.Write((byte)1);
				messageWriter.Write(isPublic);
				messageWriter.EndMessage();
				connection.Send(messageWriter);
				messageWriter.Recycle();
				IsGamePublic = isPublic;
			}
		}

		private void OnMessageReceived(object sender, DataReceivedEventArgs e)
		{
			MessageReader messageReader = MessageReader.Get(e.Bytes);
			while (messageReader.Position < messageReader.Length)
			{
				MessageReader reader = messageReader.ReadMessage();
				switch (reader.Tag)
				{
				case 0:
					GameId = reader.ReadInt32();
					Debug.Log("Client hosting game: " + IntToGameName(GameId));
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnGameCreated(IntToGameName(GameId));
						});
					}
					break;
				case 4:
				{
					int num10 = reader.ReadInt32();
					if (GameId != num10)
					{
						break;
					}
					int playerIdThatLeft = reader.ReadInt32();
					bool amHost2 = AmHost;
					HostId = reader.ReadInt32();
					RemovePlayer(playerIdThatLeft);
					if (!AmHost || amHost2)
					{
						break;
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnBecomeHost();
						});
					}
					break;
				}
				case 8:
				{
					int num9 = reader.ReadInt32();
					if (GameId != num9)
					{
						break;
					}
					lock (allClients)
					{
						allClients.Clear();
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnGameEnd(reader);
						});
					}
					break;
				}
				case 12:
				{
					int num7 = reader.ReadInt32();
					if (GameId != num7)
					{
						break;
					}
					ClientId = reader.ReadInt32();
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnWaitForHost(IntToGameName(GameId));
						});
					}
					break;
				}
				case 7:
				{
					int num11 = reader.ReadInt32();
					if (GameId != num11)
					{
						break;
					}
					ClientId = reader.ReadInt32();
					ClientData myClient = GetOrCreateClient(ClientId);
					bool amHost3 = AmHost;
					HostId = reader.ReadInt32();
					int num12 = reader.ReadPackedInt32();
					for (int i = 0; i < num12; i++)
					{
						GetOrCreateClient(reader.ReadPackedInt32());
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnGameJoined(IntToGameName(GameId), myClient);
						});
					}
					break;
				}
				case 1:
				{
					int num3 = reader.ReadInt32();
					if (disconnectReasons.Contains((DisconnectReasons)num3))
					{
						GameId = -1;
						Disconnect((DisconnectReasons)num3);
						return;
					}
					if (GameId == num3)
					{
						int num4 = reader.ReadInt32();
						bool amHost = AmHost;
						HostId = reader.ReadInt32();
						ClientData client = GetOrCreateClient(num4);
						Debug.Log(string.Format("Player {0} joined", num4));
						lock (DispatchQueue)
						{
							DispatchQueue.Add(delegate
							{
								OnPlayerJoined(client);
							});
						}
						if (!AmHost || amHost)
						{
							break;
						}
						lock (DispatchQueue)
						{
							DispatchQueue.Add(delegate
							{
								OnBecomeHost();
							});
						}
					}
					else
					{
						Disconnect(DisconnectReasons.IncorrectGame);
					}
					break;
				}
				case 5:
				case 6:
				{
					int num5 = reader.ReadInt32();
					if (GameId != num5)
					{
						break;
					}
					if (reader.Tag == 6)
					{
						int num6 = reader.ReadPackedInt32();
						if (ClientId != num6)
						{
							Debug.LogWarning(string.Format("Got data meant for {0}", num6));
							break;
						}
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							HandleGameData(reader);
						});
					}
					break;
				}
				case 9:
				{
					int totalGames = reader.ReadPackedInt32();
					List<GameListing> output = new List<GameListing>();
					while (reader.Position < reader.Length)
					{
						output.Add(new GameListing(reader.ReadInt32(), reader.ReadPackedInt32(), reader.ReadPackedInt32(), reader.ReadString()));
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnGetGameList(totalGames, output);
						});
					}
					break;
				}
				case 10:
				{
					int num8 = reader.ReadInt32();
					if (GameId == num8)
					{
						byte b = reader.ReadByte();
						if (b == 1)
						{
							IsGamePublic = reader.ReadBoolean();
							Debug.Log("Alter Public = " + IsGamePublic);
						}
						else
						{
							Debug.Log("Alter unknown");
						}
					}
					break;
				}
				case 3:
					Disconnect(DisconnectReasons.ServerRequest);
					lock (DispatchQueue)
					{
						DispatchQueue.Clear();
						DispatchQueue.Add(delegate
						{
							OnDisconnected();
						});
					}
					break;
				case 2:
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnStartGame();
						});
					}
					break;
				case 11:
				{
					int num = reader.ReadInt32();
					if (GameId != num)
					{
						break;
					}
					int num2 = reader.ReadPackedInt32();
					if (num2 != ClientId)
					{
						break;
					}
					bool flag = reader.ReadBoolean();
					Disconnect((!flag) ? DisconnectReasons.Kicked : DisconnectReasons.Banned);
					lock (DispatchQueue)
					{
						DispatchQueue.Clear();
						DispatchQueue.Add(delegate
						{
							OnDisconnected();
						});
					}
					break;
				}
				default:
					Debug.Log(string.Format("Bad tag {0} at {1}+{2}={3}:  ", reader.Tag, reader.Offset, reader.Position, reader.Length) + string.Join(" ", reader.Buffer));
					break;
				}
			}
		}

		private ClientData GetOrCreateClient(int clientId)
		{
			lock (allClients)
			{
				ClientData clientData = allClients.FirstOrDefault((ClientData c) => c.Id == clientId);
				if (clientData == null)
				{
					clientData = new ClientData(clientId);
					allClients.Add(clientData);
					return clientData;
				}
				return clientData;
			}
		}

		private void RemovePlayer(int playerIdThatLeft)
		{
			ClientData client = null;
			lock (allClients)
			{
				int num = allClients.FindIndex((ClientData c) => c.Id == playerIdThatLeft);
				if (num != -1)
				{
					client = allClients[num];
					allClients.RemoveAt(num);
				}
			}
			if (client == null)
			{
				return;
			}
			lock (DispatchQueue)
			{
				DispatchQueue.Add(delegate
				{
					OnPlayerLeft(client);
				});
			}
		}

		public void OnApplicationPause(bool pause)
		{
			if (!pause && AmHost)
			{
				RemoveUnownedObjects();
			}
		}

		protected void SendInitialData(int clientId)
		{
			MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
			messageWriter.StartMessage(6);
			messageWriter.Write(GameId);
			messageWriter.WritePacked(clientId);
			lock (allObjects)
			{
				HashSet<GameObject> hashSet = new HashSet<GameObject>();
				for (int i = 0; i < allObjects.Count; i++)
				{
					InnerNetObject innerNetObject = allObjects[i];
					if ((bool)innerNetObject && hashSet.Add(innerNetObject.gameObject))
					{
						WriteSpawnMessage(innerNetObject, innerNetObject.OwnerId, innerNetObject.SpawnFlags, messageWriter);
					}
				}
			}
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		protected abstract void OnGameCreated(string gameIdString);

		protected abstract void OnGameJoined(string gameIdString, ClientData client);

		protected abstract void OnWaitForHost(string gameIdString);

		protected abstract void OnStartGame();

		protected abstract void OnGameEnd(MessageReader msg);

		protected abstract void OnBecomeHost();

		protected abstract void OnPlayerJoined(ClientData client);

		protected abstract void OnPlayerChangedScene(ClientData client, string targetScene);

		protected abstract void OnPlayerLeft(ClientData client);

		protected abstract void OnDisconnected();

		protected abstract void OnGetGameList(int totalGames, List<GameListing> availableGames);

		protected abstract byte[] GetConnectionData();

		protected ClientData FindClientById(int id)
		{
			lock (allClients)
			{
				for (int i = 0; i < allClients.Count; i++)
				{
					if (allClients[i].Id == id)
					{
						return allClients[i];
					}
				}
				return null;
			}
		}

		public static string IntToGameName(int gameId)
		{
			char[] array = new char[4]
			{
				(char)((uint)(gameId >> 0) & 0xFFu),
				(char)((uint)(gameId >> 8) & 0xFFu),
				(char)((uint)(gameId >> 16) & 0xFFu),
				(char)((uint)(gameId >> 24) & 0xFFu)
			};
			if (array.Any((char c) => c < 'A' || c > 'z'))
			{
				return null;
			}
			return new string(array);
		}

		public static int GameNameToInt(string gameId)
		{
			if (gameId.Length != 4)
			{
				return -1;
			}
			gameId = gameId.ToUpperInvariant();
			return (int)(gameId[0] | ((uint)gameId[1] << 8) | ((uint)gameId[2] << 16) | ((uint)gameId[3] << 24));
		}

		private void FixedUpdate()
		{
			if (mode == MatchMakerModes.None || Streams == null)
			{
				timer = 0f;
				return;
			}
			timer += Time.fixedDeltaTime;
			if (timer < MinSendInterval)
			{
				return;
			}
			timer = 0f;
			lock (allObjects)
			{
				for (int i = 0; i < allObjects.Count; i++)
				{
					InnerNetObject innerNetObject = allObjects[i];
					if ((bool)innerNetObject && innerNetObject.DirtyBits != 0 && (innerNetObject.AmOwner || (innerNetObject.OwnerId == -2 && AmHost)))
					{
						MessageWriter messageWriter = Streams[(uint)innerNetObject.sendMode];
						messageWriter.StartMessage(1);
						messageWriter.WritePacked(innerNetObject.NetId);
						if (innerNetObject.Serialize(messageWriter, false))
						{
							messageWriter.EndMessage();
						}
						else
						{
							messageWriter.CancelMessage();
						}
					}
				}
			}
			for (int j = 0; j < Streams.Length; j++)
			{
				MessageWriter messageWriter2 = Streams[j];
				try
				{
					if (!messageWriter2.HasBytes(7))
					{
						continue;
					}
					messageWriter2.EndMessage();
					connection.Send(messageWriter2);
					goto IL_0169;
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					goto IL_0169;
				}
				IL_0169:
				messageWriter2.Clear((SendOption)j);
				messageWriter2.StartMessage(5);
				messageWriter2.Write(GameId);
			}
		}

		public T FindObjectByNetId<T>(uint netId) where T : InnerNetObject
		{
			InnerNetObject value;
			if (allObjectsFast.TryGetValue(netId, out value))
			{
				return (T)value;
			}
			Debug.LogWarning("Couldn't find target object: " + netId);
			return (T)null;
		}

		public void SendRpcImmediately(uint targetNetId, byte callId, SendOption option)
		{
			MessageWriter messageWriter = MessageWriter.Get(option);
			messageWriter.StartMessage(5);
			messageWriter.Write(GameId);
			messageWriter.StartMessage(2);
			messageWriter.WritePacked(targetNetId);
			messageWriter.Write(callId);
			messageWriter.EndMessage();
			messageWriter.EndMessage();
			connection.Send(messageWriter);
			messageWriter.Recycle();
		}

		public MessageWriter StartRpcImmediately(uint targetNetId, byte callId, SendOption option, int targetClientId = -1)
		{
			MessageWriter messageWriter = MessageWriter.Get(option);
			if (targetClientId < 0)
			{
				messageWriter.StartMessage(5);
				messageWriter.Write(GameId);
			}
			else
			{
				messageWriter.StartMessage(6);
				messageWriter.Write(GameId);
				messageWriter.WritePacked(targetClientId);
			}
			messageWriter.StartMessage(2);
			messageWriter.WritePacked(targetNetId);
			messageWriter.Write(callId);
			return messageWriter;
		}

		public void FinishRpcImmediately(MessageWriter msg)
		{
			msg.EndMessage();
			msg.EndMessage();
			connection.Send(msg);
			msg.Recycle();
		}

		public void SendRpc(uint targetNetId, byte callId, SendOption option = SendOption.Reliable)
		{
			MessageWriter messageWriter = StartRpc(targetNetId, callId, option);
			messageWriter.EndMessage();
		}

		public MessageWriter StartRpc(uint targetNetId, byte callId, SendOption option = SendOption.Reliable)
		{
			MessageWriter messageWriter = Streams[(uint)option];
			messageWriter.StartMessage(2);
			messageWriter.WritePacked(targetNetId);
			messageWriter.Write(callId);
			return messageWriter;
		}

		private void SendSceneChange(string sceneName)
		{
			if (connection != null)
			{
				StartCoroutine(CoSendSceneChange(sceneName));
			}
		}

		private IEnumerator CoSendSceneChange(string sceneName)
		{
			lock (allObjects)
			{
				for (int num = allObjects.Count - 1; num > -1; num--)
				{
					InnerNetObject innerNetObject = allObjects[num];
					if (!innerNetObject)
					{
						allObjects.RemoveAt(num);
					}
				}
			}
			while (connection != null && ClientId < 0)
			{
				yield return null;
			}
			if (connection == null)
			{
				yield break;
			}
			if (!AmHost && connection != null && connection.State == ConnectionState.Connected)
			{
				MessageWriter messageWriter = MessageWriter.Get(SendOption.Reliable);
				messageWriter.StartMessage(5);
				messageWriter.Write(GameId);
				messageWriter.StartMessage(6);
				messageWriter.WritePacked(ClientId);
				messageWriter.Write(sceneName);
				messageWriter.EndMessage();
				messageWriter.EndMessage();
				try
				{
					connection.Send(messageWriter);
				}
				catch
				{
				}
				finally
				{
					messageWriter.Recycle();
				}
			}
			ClientData client = FindClientById(ClientId);
			if (client != null)
			{
				Debug.Log(string.Format("Changed scene: {0} {1}", ClientId, sceneName));
				lock (DispatchQueue)
				{
					DispatchQueue.Add(delegate
					{
						OnPlayerChangedScene(client, sceneName);
					});
					yield break;
				}
			}
			Debug.Log(string.Format("Couldn't find self in clients: {0}.", ClientId));
		}

		public void Spawn(InnerNetObject netObjParent, int ownerId = -2, SpawnFlags flags = SpawnFlags.None)
		{
			if (!AmHost)
			{
				if (AmClient)
				{
					Debug.LogError("Tried to spawn while not host:" + netObjParent);
				}
			}
			else
			{
				ownerId = ((ownerId != -3) ? ownerId : ClientId);
				MessageWriter msg = Streams[1];
				WriteSpawnMessage(netObjParent, ownerId, flags, msg);
			}
		}

		private void WriteSpawnMessage(InnerNetObject netObjParent, int ownerId, SpawnFlags flags, MessageWriter msg)
		{
			msg.StartMessage(4);
			msg.WritePacked(netObjParent.SpawnId);
			msg.WritePacked(ownerId);
			msg.Write((byte)flags);
			InnerNetObject[] componentsInChildren = netObjParent.GetComponentsInChildren<InnerNetObject>();
			msg.WritePacked(componentsInChildren.Length);
			foreach (InnerNetObject innerNetObject in componentsInChildren)
			{
				innerNetObject.OwnerId = ownerId;
				innerNetObject.SpawnFlags = flags;
				if (innerNetObject.NetId == 0)
				{
					RegisterNetObject(innerNetObject);
				}
				msg.WritePacked(innerNetObject.NetId);
				msg.StartMessage(1);
				innerNetObject.Serialize(msg, true);
				msg.EndMessage();
			}
			msg.EndMessage();
		}

		public void Despawn(InnerNetObject objToDespawn)
		{
			if (objToDespawn.NetId < 1)
			{
				Debug.Log("Tried to net destroy: " + objToDespawn);
				return;
			}
			MessageWriter messageWriter = Streams[1];
			messageWriter.StartMessage(5);
			messageWriter.WritePacked(objToDespawn.NetId);
			messageWriter.EndMessage();
			RemoveNetObject(objToDespawn);
		}

		private void RegisterNetObject(InnerNetObject obj)
		{
			if (obj.NetId == 0)
			{
				obj.NetId = NetIdCnt++;
				allObjects.Add(obj);
				allObjectsFast.Add(obj.NetId, obj);
			}
			else
			{
				Debug.LogError("Attempted to double register: " + obj.name);
			}
		}

		private bool AddNetObject(InnerNetObject obj)
		{
			uint num = obj.NetId + 1;
			NetIdCnt = ((NetIdCnt <= num) ? num : NetIdCnt);
			if (!allObjectsFast.ContainsKey(obj.NetId))
			{
				allObjects.Add(obj);
				allObjectsFast.Add(obj.NetId, obj);
				return true;
			}
			return false;
		}

		public void RemoveNetObject(InnerNetObject obj)
		{
			int num = allObjects.BinarySearch(obj);
			if (num > -1)
			{
				allObjects.RemoveAt(num);
			}
			allObjectsFast.Remove(obj.NetId);
			obj.NetId = uint.MaxValue;
		}

		public void RemoveUnownedObjects()
		{
			HashSet<int> hashSet = new HashSet<int>();
			hashSet.Add(-2);
			lock (allClients)
			{
				for (int num = allClients.Count - 1; num >= 0; num--)
				{
					ClientData clientData = allClients[num];
					if ((bool)clientData.Character)
					{
						hashSet.Add(clientData.Id);
					}
				}
			}
			lock (allObjects)
			{
				for (int num2 = allObjects.Count - 1; num2 > -1; num2--)
				{
					InnerNetObject innerNetObject = allObjects[num2];
					if (!innerNetObject)
					{
						allObjects.RemoveAt(num2);
					}
					else if (!hashSet.Contains(innerNetObject.OwnerId))
					{
						innerNetObject.OwnerId = ClientId;
						UnityEngine.Object.Destroy(innerNetObject.gameObject);
					}
				}
			}
		}

		private void HandleGameData(MessageReader parentReader)
		{
			while (parentReader.Position < parentReader.Length)
			{
				MessageReader messageReader = parentReader.ReadMessage();
				switch (messageReader.Tag)
				{
				case 1:
				{
					uint num5 = messageReader.ReadPackedUInt32();
					InnerNetObject value;
					if (allObjectsFast.TryGetValue(num5, out value))
					{
						value.Deserialize(messageReader, false);
					}
					else
					{
						Debug.LogWarning("Couldn't find target obj: " + num5);
					}
					break;
				}
				case 2:
				{
					uint num6 = messageReader.ReadPackedUInt32();
					InnerNetObject value2;
					if (allObjectsFast.TryGetValue(num6, out value2))
					{
						value2.HandleRpc(messageReader.ReadByte(), messageReader);
					}
					else
					{
						Debug.LogWarning(string.Format("Couldn't find target obj: {0} = {1}", num6, string.Join(" ", parentReader.Buffer)));
					}
					break;
				}
				case 4:
				{
					uint num2 = messageReader.ReadPackedUInt32();
					if (num2 < SpawnableObjects.Length)
					{
						InnerNetObject innerNetObject = UnityEngine.Object.Instantiate(SpawnableObjects[num2]);
						int num3 = messageReader.ReadPackedInt32();
						innerNetObject.SpawnFlags = (SpawnFlags)messageReader.ReadByte();
						Debug.Log(string.Format("Spawn {0} ({1}) for {2} with flags {3}", num2, innerNetObject.name, num3, innerNetObject.SpawnFlags));
						int num4 = messageReader.ReadPackedInt32();
						InnerNetObject[] componentsInChildren = innerNetObject.GetComponentsInChildren<InnerNetObject>();
						if (num4 != componentsInChildren.Length)
						{
							Debug.LogError("Children didn't match for spawnable " + num2);
							UnityEngine.Object.Destroy(innerNetObject.gameObject);
							break;
						}
						if ((innerNetObject.SpawnFlags & SpawnFlags.IsClientCharacter) != 0)
						{
							ClientData clientData2 = FindClientById(num3);
							if (clientData2 != null)
							{
								if ((bool)clientData2.Character)
								{
									Debug.LogError("Double spawn character");
									UnityEngine.Object.Destroy(innerNetObject.gameObject);
									break;
								}
								clientData2.InScene = true;
								clientData2.Character = innerNetObject as PlayerControl;
							}
						}
						for (int i = 0; i < num4; i++)
						{
							InnerNetObject innerNetObject2 = componentsInChildren[i];
							innerNetObject2.NetId = messageReader.ReadPackedUInt32();
							innerNetObject2.OwnerId = num3;
							if (!AddNetObject(innerNetObject2))
							{
								Debug.LogWarning(string.Format("Duplicate spawn {0}: {1}", innerNetObject2.NetId, innerNetObject2.name));
								innerNetObject.NetId = uint.MaxValue;
								UnityEngine.Object.Destroy(innerNetObject.gameObject);
								break;
							}
							MessageReader messageReader2 = messageReader.ReadMessage();
							if (messageReader2.Length > 0)
							{
								innerNetObject2.Deserialize(messageReader2, true);
							}
						}
					}
					else
					{
						Debug.LogWarning("Couldn't find spawnable prefab: " + num2);
					}
					break;
				}
				case 5:
				{
					uint num7 = messageReader.ReadPackedUInt32();
					InnerNetObject innerNetObject3 = FindObjectByNetId<InnerNetObject>(num7);
					if ((bool)innerNetObject3)
					{
						RemoveNetObject(innerNetObject3);
						UnityEngine.Object.Destroy(innerNetObject3.gameObject);
					}
					else
					{
						Debug.LogWarning("Couldn't despawn netId: " + num7);
					}
					break;
				}
				case 6:
				{
					ClientData client = FindClientById(messageReader.ReadPackedInt32());
					string targetScene = messageReader.ReadString();
					ClientData clientData = client;
					int? num = ((clientData != null) ? new int?(clientData.Id) : null);
					Debug.Log(string.Format("Client {0} changed scene to {1}", (!num.HasValue) ? (-1) : num.Value, targetScene));
					if (client == null || string.IsNullOrWhiteSpace(targetScene))
					{
						break;
					}
					lock (DispatchQueue)
					{
						DispatchQueue.Add(delegate
						{
							OnPlayerChangedScene(client, targetScene);
						});
					}
					break;
				}
				default:
					Debug.Log(string.Format("Bad tag {0} at {1}+{2}={3}:  ", messageReader.Tag, messageReader.Offset, messageReader.Position, messageReader.Length) + string.Join(" ", messageReader.Buffer));
					break;
				}
			}
		}
	}
}
