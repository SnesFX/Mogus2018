using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Hazel;
using InnerNet;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AmongUsClient : InnerNetClient
{
	public static AmongUsClient Instance;

	public GameModes GameMode;

	public string OnlineScene;

	public string MainMenuScene;

	public GameData GameDataPrefab;

	public PlayerControl PlayerPrefab;

	public ShipStatus ShipPrefab;

	public float SpawnRadius = 1.75f;

	public DiscoveryState discoverState;

	public List<IDisconnectHandler> DisconnectHandlers = new List<IDisconnectHandler>();

	public List<IGameListHandler> GameListHandlers = new List<IGameListHandler>();

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
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = 30;
		}
	}

	protected override byte[] GetConnectionData()
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			using (BinaryWriter binaryWriter = new BinaryWriter(memoryStream))
			{
				binaryWriter.Write(Constants.GetBroadcastVersion());
				binaryWriter.Write(SaveManager.PlayerName);
				binaryWriter.Flush();
				return memoryStream.ToArray();
			}
		}
	}

	public void StartGame()
	{
		LockGame();
		discoverState = DiscoveryState.Off;
	}

	public void ExitGame()
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		SoundManager.Instance.StopAllSound();
		discoverState = DiscoveryState.Off;
		DisconnectHandlers.Clear();
		Disconnect(DisconnectReasons.ExitGame);
		SceneManager.LoadScene(MainMenuScene);
	}

	protected override void OnGetGameList(int totalGames, List<GameListing> availableGames)
	{
		for (int i = 0; i < GameListHandlers.Count; i++)
		{
			try
			{
				GameListHandlers[i].HandleList(totalGames, availableGames);
			}
			catch
			{
			}
		}
	}

	protected override void OnGameCreated(string gameIdString)
	{
	}

	protected override void OnWaitForHost(string gameIdString)
	{
		Debug.Log("Waiting for host: " + gameIdString);
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Show();
		}
	}

	protected override void OnStartGame()
	{
		StartCoroutine(CoStartGame());
	}

	private IEnumerator CoStartGame()
	{
		while (!GameData.Instance)
		{
			yield return null;
		}
		GameData.Instance.GameStarted = true;
		CustomPlayerMenu game = UnityEngine.Object.FindObjectOfType<CustomPlayerMenu>();
		if ((bool)game)
		{
			game.Close(false);
		}
		if (DestroyableSingleton<GameStartManager>.InstanceExists)
		{
			Instance.DisconnectHandlers.Remove(DestroyableSingleton<GameStartManager>.Instance);
			UnityEngine.Object.Destroy(DestroyableSingleton<GameStartManager>.Instance.gameObject);
		}
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black);
		if ((bool)LobbyBehaviour.Instance)
		{
			LobbyBehaviour.Instance.StopAllCoroutines();
		}
		do
		{
			if (base.AmHost)
			{
				float waittime = 0f;
				bool keepWaiting = true;
				while (keepWaiting && waittime < 1f)
				{
					keepWaiting = false;
					for (int i = 0; i < allClients.Count; i++)
					{
						ClientData clientData = allClients[i];
						if (clientData.InScene)
						{
							if (!clientData.Character)
							{
								CreatePlayer(clientData);
							}
							else if (!clientData.Character.isReady)
							{
								keepWaiting = true;
							}
						}
						else
						{
							keepWaiting = true;
						}
					}
					yield return null;
					waittime += Time.deltaTime;
				}
				if (!ShipStatus.Instance)
				{
					ShipStatus.Instance = UnityEngine.Object.Instantiate(ShipPrefab);
				}
				if (!DisconnectHandlers.Contains(ShipStatus.Instance))
				{
					DisconnectHandlers.Add(ShipStatus.Instance);
				}
				Spawn(ShipStatus.Instance);
				GameOptionsData options = PlayerControl.GameOptions;
				if (options.Validate(GameData.Instance.AllPlayers.Count) && (object)PlayerControl.LocalPlayer != null)
				{
					PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
				}
				ShipStatus.Instance.SelectInfected();
				ShipStatus.Instance.Begin();
				break;
			}
			yield return new WaitUntil(() => (bool)ShipStatus.Instance || base.AmHost);
		}
		while (base.AmHost);
		for (int j = 0; j < GameData.Instance.AllPlayers.Count; j++)
		{
			PlayerControl @object = GameData.Instance.AllPlayers[j].Object;
			if ((bool)@object)
			{
				@object.NetTransform.enabled = true;
				@object.MyPhysics.enabled = true;
				@object.MyPhysics.Start();
				@object.MyPhysics.ResetAnim();
				@object.Collider.enabled = true;
				Vector2 spawnLocation = ShipStatus.Instance.GetSpawnLocation(j, GameData.Instance.AllPlayers.Count);
				@object.NetTransform.SnapTo(spawnLocation);
			}
		}
	}

	protected override void OnBecomeHost()
	{
		ClientData clientData = FindClientById(ClientId);
		if (!clientData.Character)
		{
			OnGameJoined(null, clientData);
		}
		Debug.Log("Became Host");
		RemoveUnownedObjects();
	}

	protected override void OnGameEnd(MessageReader msg)
	{
		DisconnectHandlers.Clear();
		if ((bool)PlayerControl.LocalPlayer)
		{
			PlayerControl.LocalPlayer.EndGame((GameOverReason)msg.ReadByte(), msg.ReadBoolean());
		}
	}

	protected override void OnPlayerJoined(ClientData data)
	{
		if (base.AmHost && data.InScene && !data.Character)
		{
			CreatePlayer(data);
		}
	}

	protected override void OnGameJoined(string gameIdString, ClientData data)
	{
		if (DestroyableSingleton<WaitForHostPopup>.InstanceExists)
		{
			DestroyableSingleton<WaitForHostPopup>.Instance.Hide();
		}
		if (!string.IsNullOrWhiteSpace(OnlineScene))
		{
			SceneManager.LoadScene(OnlineScene);
		}
	}

	protected override void OnPlayerLeft(ClientData data)
	{
		if ((bool)data.Character)
		{
			PlayerControl character = data.Character;
			Debug.Log(string.Format("Player {0}({1}) left: ", data.Id, character.name) + character.isReady);
			for (int num = DisconnectHandlers.Count - 1; num > -1; num--)
			{
				try
				{
					DisconnectHandlers[num].HandleDisconnect(character);
				}
				catch (Exception exception)
				{
					Debug.LogException(exception);
					DisconnectHandlers.RemoveAt(num);
				}
			}
			UnityEngine.Object.Destroy(data.Character.gameObject);
		}
		else
		{
			Debug.LogWarning("A player without a character disconnected");
			for (int num2 = DisconnectHandlers.Count - 1; num2 > -1; num2--)
			{
				try
				{
					DisconnectHandlers[num2].HandleDisconnect();
				}
				catch (Exception exception2)
				{
					Debug.LogException(exception2);
					DisconnectHandlers.RemoveAt(num2);
				}
			}
		}
		if (!base.AmHost)
		{
			return;
		}
		bool? flag = ((PlayerControl.GameOptions != null) ? new bool?(PlayerControl.GameOptions.isDefaults) : null);
		if (flag.HasValue && flag.Value)
		{
			PlayerControl.GameOptions.SetRecommendations(GameData.Instance.AllPlayers.Count);
			if ((object)PlayerControl.LocalPlayer != null)
			{
				PlayerControl.LocalPlayer.RpcSyncSettings(PlayerControl.GameOptions);
			}
		}
	}

	protected override void OnDisconnected()
	{
		SceneManager.LoadScene(MainMenuScene);
	}

	protected override void OnPlayerChangedScene(ClientData client, string currentScene)
	{
		client.InScene = true;
		if (!base.AmHost)
		{
			return;
		}
		if (currentScene.Equals("Tutorial"))
		{
			GameData.Instance = UnityEngine.Object.Instantiate(GameDataPrefab);
			Spawn(GameData.Instance);
			ShipStatus netObjParent = UnityEngine.Object.Instantiate(ShipPrefab);
			Spawn(netObjParent);
			CreatePlayer(client);
		}
		else
		{
			if (!currentScene.Equals("OnlineGame"))
			{
				return;
			}
			if (client.Id != ClientId)
			{
				SendInitialData(client.Id);
			}
			else
			{
				PlayerControl.GameOptions = SaveManager.GameOptions;
				if (GameMode == GameModes.LocalGame)
				{
					StartCoroutine(CoBroadcastManager());
				}
				if (!GameData.Instance)
				{
					GameData.Instance = UnityEngine.Object.Instantiate(GameDataPrefab);
					Spawn(GameData.Instance);
				}
			}
			if (!client.Character)
			{
				CreatePlayer(client);
			}
		}
	}

	[ContextMenu("Spawn Tester")]
	private void SpawnTester()
	{
		byte availableId = GameData.Instance.GetAvailableId();
		Vector2 vector = Vector2.up.Rotate((float)(int)availableId * (360f / (float)Palette.PlayerColors.Length)) * SpawnRadius;
		PlayerControl playerControl = UnityEngine.Object.Instantiate(PlayerPrefab, vector, Quaternion.identity);
		playerControl.PlayerName = "Test";
		playerControl.PlayerId = availableId;
		Spawn(playerControl);
		GameData.Instance.AddPlayer(playerControl);
		playerControl.CmdCheckName(playerControl.PlayerName);
		playerControl.CmdCheckColor(0);
		if (DestroyableSingleton<HatManager>.InstanceExists)
		{
			playerControl.RpcSetHat((uint)((int)availableId % DestroyableSingleton<HatManager>.Instance.AllHats.Count));
		}
	}

	private void CreatePlayer(ClientData clientData)
	{
		if (!base.AmHost)
		{
			Debug.Log("Waiting for host to make my player");
			return;
		}
		byte availableId = GameData.Instance.GetAvailableId();
		Vector2 vector = (ShipStatus.Instance ? ShipStatus.Instance.GetSpawnLocation(availableId, Palette.PlayerColors.Length) : ((!DestroyableSingleton<TutorialManager>.InstanceExists) ? (Vector2.up.Rotate((float)(int)availableId * (360f / (float)Palette.PlayerColors.Length)) * SpawnRadius) : new Vector2(-1.9f, 3.25f)));
		Debug.Log(string.Format("Spawned player {0} for client {1}", availableId, clientData.Id));
		PlayerControl playerControl = UnityEngine.Object.Instantiate(PlayerPrefab, vector, Quaternion.identity);
		playerControl.PlayerId = availableId;
		clientData.Character = playerControl;
		Spawn(playerControl, clientData.Id, SpawnFlags.IsClientCharacter);
		GameData.Instance.AddPlayer(playerControl);
		if (PlayerControl.GameOptions.isDefaults)
		{
			PlayerControl.GameOptions.SetRecommendations(GameData.Instance.AllPlayers.Count);
		}
		playerControl.RpcSyncSettings(PlayerControl.GameOptions);
	}

	private IEnumerator CoBroadcastManager()
	{
		while (!GameData.Instance)
		{
			yield return null;
		}
		int lastPlayerCount = 0;
		discoverState = DiscoveryState.Broadcast;
		while (discoverState == DiscoveryState.Broadcast)
		{
			if (lastPlayerCount != GameData.Instance.AllPlayers.Count)
			{
				lastPlayerCount = GameData.Instance.AllPlayers.Count;
				string data = string.Format("{0}~Open~{1}~", SaveManager.PlayerName, GameData.Instance.AllPlayers.Count);
				DestroyableSingleton<InnerDiscover>.Instance.Interval = 1f;
				DestroyableSingleton<InnerDiscover>.Instance.StartAsServer(data);
			}
			yield return null;
		}
		DestroyableSingleton<InnerDiscover>.Instance.StopServer();
	}
}
