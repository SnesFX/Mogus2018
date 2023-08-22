using System.Collections;
using System.Linq;
using Assets.CoreScripts;
using Hazel;
using InnerNet;
using UnityEngine;

public class MeetingHud : InnerNetObject, IDisconnectHandler
{
	public enum VoteStates
	{
		Dead = 0,
		PreVote = 1,
		Voted = 2,
		Results = 3
	}

	private enum RpcCalls
	{
		Close = 0,
		VotingComplete = 1,
		CastVote = 2,
		ClearVote = 3
	}

	private const float Depth = -100f;

	public static MeetingHud Instance;

	public Transform ButtonParent;

	public TextRenderer TitleText;

	public Vector3 VoteOrigin = new Vector3(-3.6f, 1.75f);

	public Vector3 VoteButtonOffsets = new Vector2(3.6f, -0.91f);

	private Vector3 CounterOrigin = new Vector2(0.5f, -0.13f);

	private Vector3 CounterOffsets = new Vector2(0.3f, 0f);

	public PlayerVoteArea SkipVoteButton;

	[HideInInspector]
	private PlayerVoteArea[] playerStates;

	public PlayerVoteArea PlayerButtonPrefab;

	public SpriteRenderer PlayerVotePrefab;

	public Sprite CrackedGlass;

	public SpriteRenderer Glass;

	public PassiveButton ProceedButton;

	public ExileController ExileCutscenePrefab;

	public AudioClip VoteSound;

	public AudioClip VoteLockinSound;

	public VoteStates state = VoteStates.PreVote;

	public SpriteRenderer SkippedVoting;

	public SpriteRenderer HostIcon;

	public Sprite KillBackground;

	public ChatController ChatControl;

	private PlayerControl exiledPlayer;

	private bool wasTie;

	public TextRenderer TimerText;

	public float discussionTimer;

	private bool voteEnabled;

	private void Start()
	{
		ChatController.Instance = ChatControl;
		DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
		DestroyableSingleton<HudManager>.Instance.StopReactorFlash();
		Instance = this;
		SkipVoteButton.TargetPlayerId = -1;
		SkipVoteButton.Parent = this;
		Camera.main.GetComponent<FollowerCamera>().Locked = true;
		PlayerControl.LocalPlayer.canMove = false;
		if (PlayerControl.LocalPlayer.IsDead)
		{
			state = VoteStates.Dead;
			SkipVoteButton.gameObject.SetActive(false);
			Glass.sprite = CrackedGlass;
			Glass.color = Color.white;
		}
	}

	public void Update()
	{
		discussionTimer += Time.deltaTime;
		if (!voteEnabled)
		{
			if (discussionTimer < (float)PlayerControl.GameOptions.DiscussionTime)
			{
				float f = (float)PlayerControl.GameOptions.DiscussionTime - discussionTimer;
				TimerText.Text = string.Format("Voting Begins In: {0}s", Mathf.CeilToInt(f));
				for (int i = 0; i < playerStates.Length; i++)
				{
					playerStates[i].SetDisabled();
				}
				SkipVoteButton.SetDisabled();
				return;
			}
			voteEnabled = true;
			bool active = PlayerControl.GameOptions.VotingTime > 0;
			TimerText.gameObject.SetActive(active);
			for (int j = 0; j < playerStates.Length; j++)
			{
				playerStates[j].SetEnabled();
			}
			SkipVoteButton.SetEnabled();
		}
		else if (PlayerControl.GameOptions.VotingTime > 0 && state != VoteStates.Results)
		{
			float num = discussionTimer - (float)PlayerControl.GameOptions.DiscussionTime;
			float f2 = Mathf.Max(0f, (float)PlayerControl.GameOptions.VotingTime - num);
			TimerText.Text = string.Format("Voting Ends In: {0}s", Mathf.CeilToInt(f2));
			if (AmongUsClient.Instance.AmHost && num >= (float)PlayerControl.GameOptions.VotingTime)
			{
				ForceSkipAll();
			}
		}
	}

	public IEnumerator CoIntro(PlayerControl reporter, PlayerControl targetPlayer)
	{
		if (DestroyableSingleton<HudManager>.InstanceExists)
		{
			base.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform);
			base.transform.localPosition = new Vector3(0f, -10f, -100f);
			DestroyableSingleton<HudManager>.Instance.SetHudActive(false);
		}
		OverlayKillAnimation overlay = (targetPlayer ? DestroyableSingleton<HudManager>.Instance.KillOverlay.ReportOverlay : DestroyableSingleton<HudManager>.Instance.KillOverlay.EmergencyOverlay);
		DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowOne(overlay, reporter, targetPlayer);
		yield return DestroyableSingleton<HudManager>.Instance.KillOverlay.WaitForFinish();
		Vector3 temp = new Vector3(0f, 0f, -50f);
		for (float timer = 0f; timer < 0.25f; timer += Time.deltaTime)
		{
			float t = timer / 0.25f;
			temp.y = Mathf.SmoothStep(-10f, 0f, t);
			base.transform.localPosition = temp;
			yield return null;
		}
		temp.y = 0f;
		base.transform.localPosition = temp;
		TitleText.Text = "Who Is The Impostor?";
		if (!PlayerControl.LocalPlayer.IsDead)
		{
			yield return DestroyableSingleton<HudManager>.Instance.ShowEmblem(false);
		}
		for (int i = 0; i < playerStates.Length; i++)
		{
			StartCoroutine(playerStates[i].CoAnimateOverlay());
		}
	}

	public void OnEnable()
	{
		if (playerStates != null)
		{
			for (int i = 0; i < playerStates.Length; i++)
			{
				PlayerVoteArea playerVoteArea = playerStates[i];
				int num = i / 5;
				int num2 = i % 5;
				playerVoteArea.transform.SetParent(base.transform);
				playerVoteArea.transform.localPosition = VoteOrigin + new Vector3(VoteButtonOffsets.x * (float)num, VoteButtonOffsets.y * (float)num2, -0.1f);
			}
		}
	}

	private IEnumerator CoStartCutscene()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 1f);
		ExileController cutscene = Object.Instantiate(ExileCutscenePrefab);
		cutscene.transform.SetParent(DestroyableSingleton<HudManager>.Instance.transform, false);
		cutscene.transform.localPosition = new Vector3(0f, 0f, -60f);
		cutscene.Begin(exiledPlayer, wasTie);
		DespawnOnDestroy = false;
		Object.Destroy(base.gameObject);
	}

	public void ServerStart(PlayerControl[] players, byte reporter)
	{
		PopulateButtons(reporter);
	}

	public void Close()
	{
		StartCoroutine(CoStartCutscene());
	}

	private void VotingComplete(byte[] states, PlayerControl exiled, bool tie)
	{
		if (state != VoteStates.Results)
		{
			state = VoteStates.Results;
			exiledPlayer = exiled;
			wasTie = tie;
			TimerText.gameObject.SetActive(false);
			SkipVoteButton.gameObject.SetActive(false);
			SkippedVoting.gameObject.SetActive(true);
			AmongUsClient.Instance.DisconnectHandlers.Remove(this);
			ShipStatus.Instance.enabled = true;
			PopulateResults(states);
			SetupProceedButton();
		}
	}

	public bool Select(int suspectStateIdx)
	{
		if (discussionTimer < (float)PlayerControl.GameOptions.DiscussionTime)
		{
			return false;
		}
		if (PlayerControl.LocalPlayer.IsDead)
		{
			return false;
		}
		SoundManager.Instance.PlaySound(VoteSound, false).volume = 0.8f;
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (suspectStateIdx != playerVoteArea.TargetPlayerId)
			{
				playerVoteArea.ClearButtons();
			}
		}
		if (suspectStateIdx != -1)
		{
			SkipVoteButton.ClearButtons();
		}
		return true;
	}

	public void Confirm(sbyte suspectStateIdx)
	{
		if (!PlayerControl.LocalPlayer.IsDead)
		{
			for (int i = 0; i < playerStates.Length; i++)
			{
				playerStates[i].ClearButtons();
				playerStates[i].voteComplete = true;
			}
			SkipVoteButton.ClearButtons();
			SkipVoteButton.voteComplete = true;
			SkipVoteButton.gameObject.SetActive(false);
			switch (state)
			{
			case VoteStates.PreVote:
				state = VoteStates.Voted;
				SoundManager.Instance.PlaySound(VoteLockinSound, false);
				RpcCastVote(PlayerControl.LocalPlayer.PlayerId, suspectStateIdx);
				break;
			}
		}
	}

	public void HandleDisconnect(PlayerControl pc)
	{
		if (!AmongUsClient.Instance.AmHost)
		{
			return;
		}
		int num = playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == pc.PlayerId);
		PlayerVoteArea playerVoteArea = playerStates[num];
		playerVoteArea.isDead = true;
		playerVoteArea.Overlay.gameObject.SetActive(true);
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea2 = playerStates[i];
			if (playerVoteArea2.isDead || !playerVoteArea2.didVote || playerVoteArea2.votedFor != pc.PlayerId)
			{
				continue;
			}
			playerVoteArea2.UnsetVote();
			SetDirtyBit((uint)(1 << i));
			GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)playerVoteArea2.TargetPlayerId);
			if (playerById != null)
			{
				int clientIdFromCharacter = AmongUsClient.Instance.GetClientIdFromCharacter(playerById.Object);
				if (clientIdFromCharacter != -1)
				{
					RpcClearVote(clientIdFromCharacter);
				}
			}
		}
		SetDirtyBit((uint)(1 << num));
		CheckForEndVoting();
		if (state == VoteStates.Results)
		{
			SetupProceedButton();
		}
	}

	public void HandleDisconnect()
	{
	}

	private void ForceSkipAll()
	{
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (!playerVoteArea.didVote)
			{
				playerVoteArea.didVote = true;
				playerVoteArea.votedFor = -2;
				SetDirtyBit((uint)(1 << i));
			}
		}
		CheckForEndVoting();
	}

	public void CastVote(byte srcPlayerId, sbyte suspectPlayerId)
	{
		int num = playerStates.IndexOf((PlayerVoteArea pv) => pv.TargetPlayerId == srcPlayerId);
		PlayerVoteArea playerVoteArea = playerStates[num];
		if (!playerVoteArea.isDead && !playerVoteArea.didVote)
		{
			playerVoteArea.SetVote(suspectPlayerId);
			SetDirtyBit((uint)(1 << num));
			CheckForEndVoting();
		}
	}

	public void ClearVote()
	{
		for (int i = 0; i < playerStates.Length; i++)
		{
			playerStates[i].voteComplete = false;
		}
		SkipVoteButton.voteComplete = false;
		SkipVoteButton.gameObject.SetActive(true);
		state = VoteStates.PreVote;
	}

	private void CheckForEndVoting()
	{
		if (playerStates.All((PlayerVoteArea ps) => ps.isDead || ps.didVote))
		{
			byte[] self = CalculateVotes();
			bool tie;
			int maxIdx = self.IndexOfMax((byte p) => p, out tie) - 1;
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers.FirstOrDefault((GameData.PlayerInfo v) => v.Id == maxIdx);
			byte[] states = playerStates.Select((PlayerVoteArea ps) => ps.GetState()).ToArray();
			RpcVotingComplete(states, (playerInfo != null) ? playerInfo.Object : null, tie);
		}
	}

	private byte[] CalculateVotes()
	{
		byte[] array = new byte[11];
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			if (playerVoteArea.didVote)
			{
				int num = playerVoteArea.votedFor + 1;
				if (num >= 0 && num < array.Length)
				{
					array[num]++;
				}
			}
		}
		return array;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (playerStates == null)
		{
			return false;
		}
		if (initialState)
		{
			for (int i = 0; i < playerStates.Length; i++)
			{
				playerStates[i].Serialize(writer);
			}
		}
		else
		{
			writer.WritePacked(DirtyBits);
			for (int j = 0; j < playerStates.Length; j++)
			{
				if ((DirtyBits & (uint)(1 << j)) != 0)
				{
					playerStates[j].Serialize(writer);
				}
			}
		}
		DirtyBits = 0u;
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			PopulateButtons(0);
			for (int i = 0; i < playerStates.Length; i++)
			{
				playerStates[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < playerStates.Length; j++)
		{
			if ((num & (uint)(1 << j)) != 0)
			{
				playerStates[j].Deserialize(reader);
			}
		}
	}

	private void SetupProceedButton()
	{
		ProceedButton.gameObject.SetActive(true);
		HostIcon.gameObject.SetActive(true);
		ProceedButton.OnClick.RemoveAllListeners();
		if (AmongUsClient.Instance.AmHost)
		{
			ProceedButton.OnClick.AddListener(delegate
			{
				RpcClose();
			});
		}
		else
		{
			ProceedButton.OnClick.AddListener(delegate
			{
				StartCoroutine(Effects.Shake(HostIcon.transform));
			});
		}
		GameData.PlayerInfo host = GameData.Instance.GetHost();
		if (host != null)
		{
			PlayerControl.SetPlayerMaterialColors(host.Color, HostIcon);
		}
		else
		{
			HostIcon.enabled = false;
		}
	}

	private void PopulateResults(byte[] states)
	{
		DestroyableSingleton<Telemetry>.Instance.WriteMeetingEnded(states, discussionTimer);
		TitleText.Text = "Voting Results";
		int num = 0;
		for (int i = 0; i < playerStates.Length; i++)
		{
			PlayerVoteArea playerVoteArea = playerStates[i];
			playerVoteArea.ClearForResults();
			int num2 = 0;
			for (int j = 0; j < playerStates.Length; j++)
			{
				if (!states[j].HasAnyBit((byte)128))
				{
					GameData.PlayerInfo playerById = GameData.Instance.GetPlayerById((byte)playerStates[j].TargetPlayerId);
					int num3 = (states[j] & 0xF) - 1;
					if (num3 == playerVoteArea.TargetPlayerId)
					{
						SpriteRenderer spriteRenderer = Object.Instantiate(PlayerVotePrefab);
						PlayerControl.SetPlayerMaterialColors(playerById.Color, spriteRenderer);
						spriteRenderer.transform.SetParent(playerVoteArea.transform);
						spriteRenderer.transform.localPosition = CounterOrigin + new Vector3(CounterOffsets.x * (float)num2, 0f, 0f);
						spriteRenderer.transform.localScale = Vector3.zero;
						StartCoroutine(Effects.Bloop((float)num2 * 0.5f, spriteRenderer.transform));
						num2++;
					}
					else if (i == 0 && num3 == -1)
					{
						SpriteRenderer spriteRenderer2 = Object.Instantiate(PlayerVotePrefab);
						PlayerControl.SetPlayerMaterialColors(playerById.Color, spriteRenderer2);
						spriteRenderer2.transform.SetParent(SkippedVoting.transform);
						spriteRenderer2.transform.localPosition = CounterOrigin + new Vector3(CounterOffsets.x * (float)num, 0f, 0f);
						spriteRenderer2.transform.localScale = Vector3.zero;
						StartCoroutine(Effects.Bloop((float)num * 0.5f, spriteRenderer2.transform));
						num++;
					}
				}
			}
		}
	}

	private void PopulateButtons(byte reporter)
	{
		playerStates = new PlayerVoteArea[GameData.Instance.AllPlayers.Count];
		for (int i = 0; i < playerStates.Length; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			PlayerControl @object = playerInfo.Object;
			PlayerVoteArea playerVoteArea = (playerStates[i] = CreateButton(playerInfo));
			playerVoteArea.Parent = this;
			playerVoteArea.TargetPlayerId = (sbyte)playerInfo.Id;
			playerVoteArea.SetDead(@object == PlayerControl.LocalPlayer, reporter == playerInfo.Id, playerInfo.Disconnected || !@object || @object.IsDead);
		}
		SortButtons();
	}

	private void SortButtons()
	{
		PlayerVoteArea[] array = (from p in playerStates
			orderby p.isDead ? 50 : 0, p.TargetPlayerId
			select p).ToArray();
		for (int i = 0; i < array.Length; i++)
		{
			int num = i % 2;
			int num2 = i / 2;
			array[i].transform.localPosition = VoteOrigin + new Vector3(VoteButtonOffsets.x * (float)num, VoteButtonOffsets.y * (float)num2, -1f);
		}
	}

	private PlayerVoteArea CreateButton(GameData.PlayerInfo playerInfo)
	{
		PlayerVoteArea playerVoteArea = Object.Instantiate(PlayerButtonPrefab, ButtonParent.transform);
		PlayerControl.SetPlayerMaterialColors(playerInfo.Color, playerVoteArea.PlayerIcon);
		playerVoteArea.NameText.Text = playerInfo.Name;
		playerVoteArea.transform.localScale = Vector3.one;
		return playerVoteArea;
	}

	public void RpcClose()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			Close();
		}
		AmongUsClient.Instance.SendRpc(NetId, 0);
	}

	public void RpcCastVote(byte playerId, sbyte suspectIdx)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CastVote(playerId, suspectIdx);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 2, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(playerId);
		messageWriter.Write(suspectIdx);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	private void RpcVotingComplete(byte[] states, PlayerControl exiled, bool tie)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			VotingComplete(states, exiled, tie);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 1);
		messageWriter.WriteBytesAndSize(states);
		messageWriter.WriteNetObject(exiled);
		messageWriter.Write(tie);
		messageWriter.EndMessage();
	}

	private void RpcClearVote(int clientId)
	{
		if (AmongUsClient.Instance.ClientId == clientId)
		{
			ClearVote();
			return;
		}
		MessageWriter msg = AmongUsClient.Instance.StartRpcImmediately(NetId, 3, SendOption.Reliable, clientId);
		AmongUsClient.Instance.FinishRpcImmediately(msg);
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.Close:
			Close();
			break;
		case RpcCalls.CastVote:
		{
			byte srcPlayerId = reader.ReadByte();
			sbyte suspectPlayerId = reader.ReadSByte();
			CastVote(srcPlayerId, suspectPlayerId);
			break;
		}
		case RpcCalls.VotingComplete:
		{
			byte[] states = reader.ReadBytesAndSize();
			PlayerControl exiled = reader.ReadNetObject<PlayerControl>();
			bool tie = reader.ReadBoolean();
			VotingComplete(states, exiled, tie);
			break;
		}
		case RpcCalls.ClearVote:
			ClearVote();
			break;
		}
	}
}
