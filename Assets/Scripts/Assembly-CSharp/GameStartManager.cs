using InnerNet;
using UnityEngine;

public class GameStartManager : DestroyableSingleton<GameStartManager>, IDisconnectHandler
{
	public int MinPlayers = 4;

	public TextRenderer PlayerCounter;

	private int LastPlayerCount = -1;

	public GameObject GameSizePopup;

	public TextRenderer GameRoomName;

	public PlayerControl PlayerPrefab;

	public LobbyBehaviour LobbyPrefab;

	public SpriteRenderer StartButton;

	public SpriteRenderer MakePublicButton;

	public Sprite PublicGameImage;

	public Sprite PrivateGameImage;

	public bool starting;

	public override void Start()
	{
		base.Start();
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			Object.Destroy(base.gameObject);
			return;
		}
		string text = InnerNetClient.IntToGameName(AmongUsClient.Instance.GameId);
		if (text != null)
		{
			GameRoomName.Text = "Room\r\n" + text;
		}
		else
		{
			StartButton.transform.localPosition = new Vector3(0f, -0.2f, 0f);
			PlayerCounter.transform.localPosition = new Vector3(0f, -0.8f, 0f);
		}
		AmongUsClient.Instance.DisconnectHandlers.Add(this);
		if (!AmongUsClient.Instance.AmHost)
		{
			StartButton.gameObject.SetActive(false);
		}
		else
		{
			LobbyBehaviour.Instance = Object.Instantiate(LobbyPrefab);
			AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance);
		}
		MakePublicButton.gameObject.SetActive(AmongUsClient.Instance.GameMode == GameModes.OnlineGame);
	}

	public void MakePublic()
	{
		if (AmongUsClient.Instance.AmHost)
		{
			AmongUsClient.Instance.ChangeGamePublic(!AmongUsClient.Instance.IsGamePublic);
		}
	}

	public void Update()
	{
		if (!GameData.Instance)
		{
			return;
		}
		MakePublicButton.sprite = ((!AmongUsClient.Instance.IsGamePublic) ? PrivateGameImage : PublicGameImage);
		if (GameData.Instance.AllPlayers.Count == LastPlayerCount)
		{
			return;
		}
		LastPlayerCount = GameData.Instance.AllPlayers.Count;
		string text = "[FF0000FF]";
		if (LastPlayerCount > MinPlayers)
		{
			text = "[00FF00FF]";
		}
		if (LastPlayerCount == MinPlayers)
		{
			text = "[FFFF00FF]";
		}
		PlayerCounter.Text = text + LastPlayerCount + "/10";
		StartButton.color = ((LastPlayerCount < MinPlayers) ? Palette.DisabledColor : Palette.EnabledColor);
	}

	public void BeginGame()
	{
		if (SaveManager.ShowMinPlayerWarning && GameData.Instance.AllPlayers.Count == MinPlayers)
		{
			GameSizePopup.SetActive(true);
		}
		else if (GameData.Instance.AllPlayers.Count < MinPlayers)
		{
			StartCoroutine(Effects.Shake(PlayerCounter.transform));
		}
		else
		{
			ReallyBegin(false);
		}
	}

	public void ReallyBegin(bool neverShow)
	{
		if (!starting)
		{
			starting = true;
			if (neverShow)
			{
				SaveManager.ShowMinPlayerWarning = false;
			}
			Object.Destroy(LobbyBehaviour.Instance.gameObject);
			AmongUsClient.Instance.StartGame();
			AmongUsClient.Instance.DisconnectHandlers.Remove(this);
			Object.Destroy(base.gameObject);
		}
	}

	public void HandleDisconnect(PlayerControl pc)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			LastPlayerCount = -1;
			StartButton.gameObject.SetActive(true);
		}
	}

	public void HandleDisconnect()
	{
		HandleDisconnect(null);
	}
}
