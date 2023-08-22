using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerControl : InnerNetObject
{
	public class ColliderComparer : IEqualityComparer<Collider2D>
	{
		public static readonly ColliderComparer Instance = new ColliderComparer();

		public bool Equals(Collider2D x, Collider2D y)
		{
			return x == y;
		}

		public int GetHashCode(Collider2D obj)
		{
			return obj.GetInstanceID();
		}
	}

	public class UsableComparer : IEqualityComparer<IUsable>
	{
		public static readonly UsableComparer Instance = new UsableComparer();

		public bool Equals(IUsable x, IUsable y)
		{
			return x == y;
		}

		public int GetHashCode(IUsable obj)
		{
			return obj.GetHashCode();
		}
	}

	public enum RpcCalls : byte
	{
		PlayAnimation = 0,
		CompleteTask = 1,
		SyncSettings = 2,
		SetInfected = 3,
		Exiled = 4,
		CheckName = 5,
		SetName = 6,
		CheckColor = 7,
		SetColor = 8,
		SetHat = 9,
		ReportDeadBody = 10,
		MurderPlayer = 11,
		SendChat = 12,
		TimesImpostor = 13,
		SetReady = 14,
		StartMeeting = 15
	}

	public bool IsImpostor;

	public bool IsDead;

	private bool IsGameOver;

	public byte PlayerId;

	public string PlayerName;

	public float MaxReportDistance = 5f;

	public bool canMove = true;

	public bool inVent;

	public int ProfilesCompleted;

	public static PlayerControl LocalPlayer;

	public AudioSource FootSteps;

	public AudioClip KillSfx;

	public KillAnimation[] KillAnimations;

	public float killTimer;

	public int RemainingEmergencies;

	public TextRenderer nameText;

	public LightSource LightPrefab;

	private LightSource myLight;

	[HideInInspector]
	public Collider2D Collider;

	[HideInInspector]
	public PlayerPhysics MyPhysics;

	[HideInInspector]
	public CustomNetworkTransform NetTransform;

	public SpriteRenderer HatRenderer;

	private SpriteRenderer myRend;

	private Collider2D[] hitBuffer = new Collider2D[20];

	public static GameOptionsData GameOptions = new GameOptionsData();

	public List<PlayerTask> myTasks = new List<PlayerTask>();

	public uint myTaskCount;

	public float percImpostor;

	public SpriteAnim[] ScannerAnims;

	public SpriteRenderer[] ScannersImages;

	public AudioClip[] VentMoveSounds;

	public AudioClip VentEnterSound;

	private IUsable closest;

	private bool isNew = true;

	public bool isReady;

	public static List<PlayerControl> AllPlayerControls = new List<PlayerControl>();

	private Dictionary<Collider2D, IUsable> cache = new Dictionary<Collider2D, IUsable>(ColliderComparer.Instance);

	private List<IUsable> itemsInRange = new List<IUsable>();

	private List<IUsable> newItemsInRange = new List<IUsable>();

	public byte ColorId { get; set; }

	public uint HatId { get; set; }

	public bool Visible
	{
		set
		{
			myRend.enabled = value;
			HatRenderer.enabled = value;
			nameText.gameObject.SetActive(value);
		}
	}

	private void Start()
	{
		AllPlayerControls.Add(this);
		RemainingEmergencies = GameOptions.NumEmergencyMeetings;
		myRend = GetComponent<SpriteRenderer>();
		MyPhysics = GetComponent<PlayerPhysics>();
		NetTransform = GetComponent<CustomNetworkTransform>();
		Collider = GetComponent<Collider2D>();
		if (base.AmOwner)
		{
			myLight = UnityEngine.Object.Instantiate(LightPrefab);
			myLight.transform.SetParent(base.transform);
			myLight.transform.localPosition = Collider.offset;
			LocalPlayer = this;
			Camera.main.GetComponent<FollowerCamera>().SetTarget(this);
			SetName(SaveManager.PlayerName);
			CmdCheckName(PlayerName);
			SetColor(SaveManager.BodyColor);
			CmdCheckColor(SaveManager.BodyColor);
			RpcSetHat(SaveManager.LastHat);
			RpcSetTimesImpostor((float)SaveManager.TimesImpostor / ((float)SaveManager.GamesStarted + 1f));
		}
		if ((bool)LobbyBehaviour.Instance && isNew)
		{
			LobbyBehaviour.Instance.StartCoroutine(LobbyBehaviour.Instance.CoSpawnPlayer(this));
		}
		isNew = false;
	}

	public override void OnDestroy()
	{
		AllPlayerControls.Remove(this);
		base.OnDestroy();
	}

	private void FixedUpdate()
	{
		if (base.AmOwner && !isReady)
		{
			bool flag = true;
			for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
			{
				GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
				if (!playerInfo.Disconnected && !playerInfo.Object)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				RpcSetReady();
			}
		}
		if (IsDead && (bool)myRend && (bool)LocalPlayer)
		{
			Visible = LocalPlayer.IsDead;
		}
		if (!base.AmOwner)
		{
			return;
		}
		if ((bool)ShipStatus.Instance)
		{
			myLight.LightRadius = ShipStatus.Instance.CalculateLightRadius(this);
		}
		if (IsImpostor && canMove && !IsDead)
		{
			killTimer = Mathf.Max(0f, killTimer - Time.fixedDeltaTime);
			PlayerControl target = FindClosestTarget();
			DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(target);
			if (GameOptions.KillCooldown > 0f)
			{
				DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(killTimer / GameOptions.KillCooldown);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(0f);
			}
		}
		else
		{
			DestroyableSingleton<HudManager>.Instance.KillButton.SetTarget(null);
		}
		if (canMove || inVent)
		{
			newItemsInRange.Clear();
			bool flag2 = (GameOptions.GhostsDoTasks || !IsDead) && !IsGameOver && canMove;
			Vector2 truePosition = GetTruePosition();
			int num = Physics2D.OverlapCircleNonAlloc(truePosition, MaxReportDistance, hitBuffer, Constants.Usables);
			IUsable usable = null;
			float num2 = float.MaxValue;
			bool flag3 = false;
			for (int k = 0; k < num; k++)
			{
				Collider2D collider2D = hitBuffer[k];
				IUsable value;
				if (!cache.TryGetValue(collider2D, out value))
				{
					IUsable component = collider2D.GetComponent<IUsable>();
					cache[collider2D] = component;
					value = component;
				}
				if (value != null)
				{
					newItemsInRange.Add(value);
					if (flag2 || inVent)
					{
						float num3 = Vector2.Distance(collider2D.transform.TransformPoint(collider2D.offset), truePosition);
						if (num3 < num2 && num3 <= value.UsableDistance && value.CanUse(this))
						{
							num2 = num3;
							usable = value;
						}
					}
				}
				if (flag2 && !IsDead && !flag3 && collider2D.tag == "DeadBody")
				{
					DeadBody component2 = collider2D.GetComponent<DeadBody>();
					if (!PhysicsHelpers.AnythingBetween(truePosition, component2.TruePosition, Constants.ShipAndObjectsMask, false))
					{
						flag3 = true;
					}
				}
			}
			for (int num4 = itemsInRange.Count - 1; num4 > -1; num4--)
			{
				IUsable item = itemsInRange[num4];
				int num5 = newItemsInRange.FindIndex((IUsable j) => j == item);
				if (num5 == -1 || !item.CanUse(this))
				{
					item.SetOutline(false, false);
					itemsInRange.RemoveAt(num4);
				}
				else
				{
					newItemsInRange.RemoveAt(num5);
				}
			}
			for (int l = 0; l < newItemsInRange.Count; l++)
			{
				IUsable usable2 = newItemsInRange[l];
				if (usable2.CanUse(this))
				{
					usable2.SetOutline(true, usable == usable2);
					itemsInRange.Add(usable2);
				}
			}
			closest = usable;
			DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(usable);
			DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(flag3);
		}
		else
		{
			closest = null;
			DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
			DestroyableSingleton<HudManager>.Instance.ReportButton.SetActive(false);
		}
	}

	public void UseClosest()
	{
		if (closest != null)
		{
			closest.Use();
		}
		closest = null;
		DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
	}

	public void ReportClosest()
	{
		if (IsGameOver)
		{
			return;
		}
		Collider2D[] array = Physics2D.OverlapCircleAll(base.transform.position, MaxReportDistance, Constants.NotShipMask);
		foreach (Collider2D collider2D in array)
		{
			if (collider2D.tag != "DeadBody")
			{
				continue;
			}
			DeadBody component = collider2D.GetComponent<DeadBody>();
			if ((bool)component && !component.Reported)
			{
				component.OnClick();
				if (component.Reported)
				{
					break;
				}
			}
		}
	}

	public void PlayStepSound()
	{
		if (Constants.ShouldPlaySfx() && DestroyableSingleton<HudManager>.InstanceExists && LocalPlayer == this)
		{
			ShipRoom lastRoom = DestroyableSingleton<HudManager>.Instance.roomTracker.LastRoom;
			if ((bool)lastRoom)
			{
				AudioClip clip = lastRoom.FootStepSounds.Random();
				FootSteps.clip = clip;
				FootSteps.Play();
			}
		}
	}

	public void SetScanner(bool on)
	{
		for (int i = 0; i < ScannerAnims.Length; i++)
		{
			SpriteAnim spriteAnim = ScannerAnims[i];
			if (on && !IsDead)
			{
				spriteAnim.gameObject.SetActive(true);
				spriteAnim.Play();
				ScannersImages[i].flipX = !myRend.flipX;
				continue;
			}
			if (spriteAnim.isActiveAndEnabled)
			{
				spriteAnim.Stop();
			}
			spriteAnim.gameObject.SetActive(false);
		}
	}

	public Vector2 GetTruePosition()
	{
		return (Vector2)base.transform.position + Collider.offset;
	}

	private PlayerControl FindClosestTarget()
	{
		PlayerControl result = null;
		float num = GameOptionsData.KillDistances[GameOptions.KillDistance];
		if (!ShipStatus.Instance)
		{
			return null;
		}
		Vector2 truePosition = GetTruePosition();
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = allPlayers[i];
			if (playerInfo.Disconnected)
			{
				continue;
			}
			PlayerControl @object = playerInfo.Object;
			if (!@object || @object == this || @object.IsDead || @object.IsImpostor)
			{
				continue;
			}
			Vector2 truePosition2 = @object.GetTruePosition();
			Vector2 vector = truePosition2 - truePosition;
			float magnitude = vector.magnitude;
			if (magnitude <= num)
			{
				RaycastHit2D[] source = Physics2D.RaycastAll(truePosition, vector.normalized, magnitude, Constants.ShipAndObjectsMask);
				if (source.All((RaycastHit2D h) => h.collider.isTrigger))
				{
					result = @object;
					num = magnitude;
				}
			}
		}
		return result;
	}

	public void SetTasks(byte[] tasks)
	{
		StartCoroutine(CoSetTasks(tasks));
	}

	private IEnumerator CoSetTasks(byte[] tasks)
	{
		while (!ShipStatus.Instance)
		{
			yield return null;
		}
		myTasks.Clear();
		if (LocalPlayer == this)
		{
			DestroyableSingleton<HudManager>.Instance.TaskStuff.SetActive(true);
			SaveManager.GamesStarted++;
			if (IsImpostor)
			{
				SaveManager.TimesImpostor++;
				ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
				importantTextTask.transform.SetParent(base.transform, false);
				importantTextTask.Text = "Sabotage and kill everyone\r\n[FFFFFFFF]Fake Tasks:";
				myTasks.Add(importantTextTask);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
			}
			if (SaveManager.SendTelemetry)
			{
				DestroyableSingleton<Telemetry>.Instance.StartGame(SaveManager.SendName, AmongUsClient.Instance.AmHost, IsImpostor, (byte)GameData.Instance.AllPlayers.Count, (byte)AmongUsClient.Instance.GameMode, percImpostor);
			}
		}
		foreach (int num in tasks)
		{
			NormalPlayerTask original = ShipStatus.Instance.NormalTasks[num];
			NormalPlayerTask normalPlayerTask = UnityEngine.Object.Instantiate(original);
			normalPlayerTask.transform.SetParent(base.transform, false);
			normalPlayerTask.Id = myTaskCount++;
			normalPlayerTask.Owner = this;
			normalPlayerTask.Initialize();
			myTasks.Add(normalPlayerTask);
		}
	}

	public void AddSystemTask(SystemTypes system)
	{
		PlayerControl localPlayer = LocalPlayer;
		if (GameOptions.GhostsDoTasks && !localPlayer.IsDead)
		{
			PlayerTask original;
			switch (system)
			{
			default:
				return;
			case SystemTypes.Reactor:
				original = ShipStatus.Instance.SpecialTasks[0];
				break;
			case SystemTypes.LifeSupp:
				original = ShipStatus.Instance.SpecialTasks[3];
				break;
			case SystemTypes.Electrical:
				original = ShipStatus.Instance.SpecialTasks[1];
				break;
			case SystemTypes.Comms:
				original = ShipStatus.Instance.SpecialTasks[2];
				break;
			}
			PlayerTask playerTask = UnityEngine.Object.Instantiate(original, localPlayer.transform);
			playerTask.Id = (byte)localPlayer.myTaskCount++;
			playerTask.Initialize();
			localPlayer.myTasks.Add(playerTask);
		}
	}

	public void RemoveTask(PlayerTask task)
	{
		task.OnRemove();
		myTasks.Remove(task);
		GameData.Instance.TutOnlyRemoveTask(PlayerId, task.Id);
		DestroyableSingleton<HudManager>.Instance.UseButton.SetTarget(null);
		UnityEngine.Object.Destroy(task.gameObject);
	}

	private void ClearTasks()
	{
		for (int i = 0; i < myTasks.Count; i++)
		{
			PlayerTask playerTask = myTasks[i];
			playerTask.OnRemove();
			UnityEngine.Object.Destroy(playerTask.gameObject);
		}
		myTasks.Clear();
	}

	public void RemoveInfected()
	{
		if (IsImpostor)
		{
			IsImpostor = false;
			myTasks.RemoveAt(0);
			DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(false);
		}
	}

	public void Die(DeathReason reason)
	{
		GameData.Instance.LastDeathReason = reason;
		IsDead = true;
		if (!Collider)
		{
			Collider = GetComponent<Collider2D>();
		}
		Collider.enabled = false;
		nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
	}

	public void Revive()
	{
		IsDead = false;
		Collider.enabled = true;
		MyPhysics.ResetAnim();
		nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 4);
	}

	public void PlayAnimation(byte animType)
	{
		switch ((TaskTypes)animType)
		{
		case TaskTypes.ClearAsteroids:
			ShipStatus.Instance.FireWeapon();
			break;
		case TaskTypes.PrimeShields:
			ShipStatus.Instance.StartShields();
			break;
		case TaskTypes.EmptyChute:
		case TaskTypes.EmptyGarbage:
			ShipStatus.Instance.OpenHatch();
			break;
		}
	}

	public void CompleteTask(uint idx)
	{
		PlayerTask playerTask = myTasks.Find((PlayerTask p) => p.Id == idx);
		if ((bool)playerTask)
		{
			GameData.Instance.CompleteTask(this, idx);
			playerTask.Complete();
			DestroyableSingleton<Telemetry>.Instance.WriteUpdateTask(PlayerId, (byte)playerTask.TaskType, (byte)playerTask.TaskStep);
		}
		else
		{
			Debug.LogWarning(PlayerName + ": Server didn't have task: " + idx);
		}
	}

	public void SetInfected(PlayerControl[] infected)
	{
		foreach (PlayerControl playerControl in infected)
		{
			if ((bool)playerControl)
			{
				Debug.Log("Infected " + playerControl.PlayerName);
				playerControl.IsImpostor = true;
			}
		}
		DestroyableSingleton<HudManager>.Instance.MapButton.gameObject.SetActive(true);
		DestroyableSingleton<HudManager>.Instance.ReportButton.gameObject.SetActive(true);
		PlayerControl localPlayer = LocalPlayer;
		localPlayer.RemainingEmergencies = GameOptions.NumEmergencyMeetings;
		if (localPlayer.IsImpostor)
		{
			DestroyableSingleton<HudManager>.Instance.KillButton.gameObject.SetActive(true);
		}
		if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			List<PlayerControl> yourTeam = ((!localPlayer.IsImpostor) ? (from pcd in GameData.Instance.AllPlayers
				where !pcd.Disconnected
				select pcd.Object into pc
				orderby (!(pc == LocalPlayer)) ? 1 : 0
				select pc).ToList() : (from pcd in GameData.Instance.AllPlayers
				where !pcd.Disconnected
				select pcd.Object into pc
				where pc.IsImpostor
				orderby (!(pc == LocalPlayer)) ? 1 : 0
				select pc).ToList());
			DestroyableSingleton<HudManager>.Instance.StartCoroutine(DestroyableSingleton<HudManager>.Instance.CoShowIntro(yourTeam));
		}
	}

	public void Exiled()
	{
		Die(DeathReason.Exile);
		if (base.AmOwner)
		{
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
			if (!GameOptions.GhostsDoTasks)
			{
				ClearTasks();
				ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
				importantTextTask.transform.SetParent(base.transform, false);
				importantTextTask.Text = "You're dead, enjoy the chaos";
				myTasks.Add(importantTextTask);
			}
		}
	}

	public void CheckName(string name)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		bool flag = false;
		for (int i = 0; i < allPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = allPlayers[i];
			if (!(playerInfo.Object == this) && playerInfo.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				if (playerInfo.Disconnected)
				{
					GameData.Instance.ReclaimDisconnected(i, this);
					RpcSetName(name);
					return;
				}
				flag = true;
				break;
			}
		}
		if (flag)
		{
			for (int j = 1; j < 100; j++)
			{
				string text = name + " " + j;
				flag = false;
				for (int k = 0; k < allPlayers.Count; k++)
				{
					if (!(allPlayers[k].Object == this) && allPlayers[k].Name.Equals(text))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					name = text;
					break;
				}
			}
		}
		RpcSetName(name);
		GameData.Instance.UpdateName(PlayerId, name);
	}

	public void SetName(string name)
	{
		PlayerName = name;
		base.gameObject.name = name;
		nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 4);
		nameText.Text = PlayerName;
	}

	public void CheckColor(byte bodyColor)
	{
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		int num = 0;
		while (allPlayers.Any((GameData.PlayerInfo p) => (bool)p.Object && p.Object != this && p.Object.ColorId == bodyColor) && num++ < 100)
		{
			bodyColor = (byte)((bodyColor + 1) % Palette.PlayerColors.Length);
		}
		GameData.Instance.UpdateColor(PlayerId, bodyColor);
		SetColor(bodyColor);
		RpcSetColor(bodyColor);
	}

	public void SetHatAlpha(float a)
	{
		Color white = Color.white;
		white.a = a;
		HatRenderer.color = white;
	}

	public void SetHat(uint hatId)
	{
		HatId = hatId;
		SetHatImage(hatId, HatRenderer);
		nameText.transform.localPosition = new Vector3(0f, (hatId != 0) ? 1f : 0.7f, -0.5f);
	}

	public static void SetHatImage(uint hatId, SpriteRenderer target)
	{
		if (DestroyableSingleton<HatManager>.InstanceExists)
		{
			HatBehaviour hatById = DestroyableSingleton<HatManager>.Instance.GetHatById(hatId);
			SetHatImage(hatById, target);
		}
	}

	public static void SetHatImage(HatBehaviour hat, SpriteRenderer target)
	{
		if ((bool)target && (bool)hat)
		{
			target.sprite = hat.MainImage;
			Vector3 localPosition = target.transform.localPosition;
			localPosition.z = ((!hat.InFront) ? 0.0001f : (-0.0001f));
			target.transform.localPosition = localPosition;
		}
		else
		{
			string arg = (target ? target.name : "null");
			string arg2 = (hat ? hat.name : "null");
			Debug.LogError(string.Format("Player: {0}\tHat: {1}", arg, arg2));
		}
	}

	public void SetColor(byte bodyColor)
	{
		myRend = myRend ?? GetComponent<SpriteRenderer>();
		ColorId = bodyColor;
		SetPlayerMaterialColors(myRend);
	}

	public void SendChat(string chatText)
	{
		if ((bool)ChatController.Instance)
		{
			ChatController.Instance.AddChat(this, chatText);
		}
	}

	private void ReportDeadBody(PlayerControl target)
	{
		if (!IsDead && !IsGameOver && !MeetingHud.Instance && !ShipStatus.Instance.CheckTaskCompletion())
		{
			ShipStatus.Instance.enabled = false;
			DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(this);
			RpcStartMeeting(target);
		}
	}

	public IEnumerator CoStartMeeting(PlayerControl target)
	{
		MeetingRoomManager.Instance.AssignSelf(this);
		DeadBody[] allBodies = UnityEngine.Object.FindObjectsOfType<DeadBody>();
		for (int i = 0; i < allBodies.Length; i++)
		{
			UnityEngine.Object.Destroy(allBodies[i].gameObject);
		}
		bool isEmergency = !target;
		DestroyableSingleton<Telemetry>.Instance.WriteMeetingStarted(isEmergency);
		PlayerControl player = LocalPlayer;
		if (player.inVent)
		{
			player.MyPhysics.RpcExitAllVents();
		}
		DestroyableSingleton<HudManager>.Instance.Map.Close();
		Minigame miniGame = UnityEngine.Object.FindObjectOfType<Minigame>();
		if ((bool)miniGame)
		{
			miniGame.Close(false);
		}
		player.NetTransform.RpcSnapTo(ShipStatus.Instance.GetSpawnLocation(GameData.Instance.GetPlayerIndex(player), GameData.Instance.AllPlayers.Count));
		player.canMove = false;
		while (!MeetingHud.Instance)
		{
			yield return null;
		}
		AmongUsClient.Instance.DisconnectHandlers.Add(MeetingHud.Instance);
		MeetingRoomManager.Instance.RemoveSelf();
		MeetingHud.Instance.StartCoroutine(MeetingHud.Instance.CoIntro(this, target));
	}

	public void MurderPlayer(PlayerControl target)
	{
		Debug.Log(string.Format("{0} murdered {1}", PlayerName, (((object)target != null) ? target.PlayerName : null) ?? "No One"));
		if (IsGameOver || target.IsDead)
		{
			return;
		}
		int num = KillAnimations.RandomIdx();
		killTimer = GameOptions.KillCooldown;
		DestroyableSingleton<Telemetry>.Instance.WriteMurder(PlayerId, target.PlayerId, target.transform.position);
		if (LocalPlayer == target)
		{
			Minigame minigame = UnityEngine.Object.FindObjectOfType<Minigame>();
			if ((bool)minigame)
			{
				minigame.Close(true);
			}
			DestroyableSingleton<HudManager>.Instance.ShadowQuad.gameObject.SetActive(false);
			base.gameObject.layer = 12;
			nameText.GetComponent<MeshRenderer>().material.SetInt("_Mask", 0);
			DestroyableSingleton<HudManager>.Instance.KillOverlay.ShowOne(this, target);
			target.SetScanner(false);
			if (!GameOptions.GhostsDoTasks)
			{
				target.ClearTasks();
				ImportantTextTask importantTextTask = new GameObject("_Player").AddComponent<ImportantTextTask>();
				importantTextTask.transform.SetParent(base.transform, false);
				importantTextTask.Text = "You're dead, enjoy the chaos";
				target.myTasks.Add(importantTextTask);
			}
		}
		StartCoroutine(KillAnimations[num].CoPerformKill(num, this, target));
	}

	public void EndGame(GameOverReason endReason, bool showAd)
	{
		if (IsGameOver)
		{
			return;
		}
		IsGameOver = true;
		Minigame minigame = UnityEngine.Object.FindObjectOfType<Minigame>();
		if ((bool)minigame)
		{
			minigame.Close(false);
		}
		LocalPlayer.canMove = false;
		try
		{
			if (SaveManager.SendTelemetry)
			{
				DestroyableSingleton<Telemetry>.Instance.EndGame(endReason);
			}
		}
		catch
		{
		}
		TempData.EndReason = endReason;
		TempData.showAd = showAd;
		bool flag = TempData.DidHumansWin(endReason);
		TempData.winners = new List<WinningPlayerData>();
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if ((bool)playerInfo.Object && flag != playerInfo.Object.IsImpostor)
			{
				TempData.winners.Add(new WinningPlayerData(playerInfo.Object));
			}
		}
		StartCoroutine(CoEndGame());
	}

	public IEnumerator CoEndGame()
	{
		yield return DestroyableSingleton<HudManager>.Instance.CoFadeFullScreen(Color.clear, Color.black, 0.5f);
		SceneManager.LoadScene("EndGame");
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			writer.Write(PlayerName);
			writer.Write(ColorId);
			writer.WritePacked(HatId);
			writer.Write(isNew);
		}
		byte b = 0;
		if (IsImpostor)
		{
			b = (byte)(b | 1u);
		}
		if (IsDead)
		{
			b = (byte)(b | 2u);
		}
		if (IsGameOver)
		{
			b = (byte)(b | 4u);
		}
		writer.Write(b);
		writer.Write(PlayerId);
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			SetName(reader.ReadString());
			SetColor(reader.ReadByte());
			SetHat(reader.ReadPackedUInt32());
			isNew = reader.ReadBoolean();
		}
		byte b = reader.ReadByte();
		IsImpostor = (b & 1) != 0;
		IsDead = (b & 2) != 0;
		IsGameOver = (b & 4) != 0;
		PlayerId = reader.ReadByte();
	}

	public void SetPlayerMaterialColors(Renderer rend)
	{
		SetPlayerMaterialColors(ColorId, rend);
	}

	public static void SetPlayerMaterialColors(int colorId, Renderer rend)
	{
		rend.material.SetColor("_BackColor", Palette.ShadowColors[colorId]);
		rend.material.SetColor("_BodyColor", Palette.PlayerColors[colorId]);
		rend.material.SetColor("_VisorColor", Palette.VisorColor);
	}

	public void RpcSetReady()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			isReady = true;
		}
		AmongUsClient.Instance.SendRpc(NetId, 14, SendOption.None);
	}

	public void RpcPlayAnimation(byte animType)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			PlayAnimation(animType);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 0, SendOption.None);
		messageWriter.Write(animType);
		messageWriter.EndMessage();
	}

	public void RpcCompleteTask(uint idx)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			CompleteTask(idx);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 1);
		messageWriter.WritePacked(idx);
		messageWriter.EndMessage();
	}

	public void RpcSyncSettings(GameOptionsData gameOptions)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			GameOptions = gameOptions;
			SaveManager.GameOptions = gameOptions;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 2);
		messageWriter.WriteBytesAndSize(gameOptions.ToBytes());
		messageWriter.EndMessage();
	}

	public void RpcSetInfected(PlayerControl[] infected)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetInfected(infected);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 3);
		messageWriter.WritePacked(infected.Length);
		for (int i = 0; i < infected.Length; i++)
		{
			messageWriter.WriteNetObject(infected[i]);
		}
		messageWriter.EndMessage();
	}

	public void CmdCheckName(string name)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckName(name);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 5, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(name);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcSetHat(uint hatId)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetHat(hatId);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 9);
		messageWriter.WritePacked(hatId);
		messageWriter.EndMessage();
	}

	public void RpcSetName(string name)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetName(name);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 6);
		messageWriter.Write(name);
		messageWriter.EndMessage();
	}

	public void CmdCheckColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			CheckColor(bodyColor);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 7, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.Write(bodyColor);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcSetColor(byte bodyColor)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetColor(bodyColor);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 8);
		messageWriter.Write(bodyColor);
		messageWriter.EndMessage();
	}

	public void RpcSetTimesImpostor(float percImpostor)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			this.percImpostor = percImpostor;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 13, SendOption.None);
		messageWriter.Write(percImpostor);
		messageWriter.EndMessage();
	}

	public bool RpcSendChat(string chatText)
	{
		if (string.IsNullOrWhiteSpace(chatText))
		{
			return false;
		}
		chatText = BlockedWords.CensorWords(chatText);
		if (AmongUsClient.Instance.AmClient)
		{
			SendChat(chatText);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 12, SendOption.Reliable);
		messageWriter.Write(chatText);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
		return true;
	}

	public void CmdReportDeadBody(PlayerControl target)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			ReportDeadBody(target);
			return;
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 10, SendOption.Reliable, AmongUsClient.Instance.HostId);
		messageWriter.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcStartMeeting(PlayerControl target)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			StartCoroutine(CoStartMeeting(target));
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 15, SendOption.Reliable);
		messageWriter.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcMurderPlayer(PlayerControl target)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			MurderPlayer(target);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 11, SendOption.Reliable);
		messageWriter.WriteNetObject(target);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.SetReady:
			isReady = true;
			break;
		case RpcCalls.PlayAnimation:
			PlayAnimation(reader.ReadByte());
			break;
		case RpcCalls.Exiled:
			Exiled();
			break;
		case RpcCalls.CheckName:
			CheckName(reader.ReadString());
			break;
		case RpcCalls.SetName:
			SetName(reader.ReadString());
			break;
		case RpcCalls.CheckColor:
			CheckColor(reader.ReadByte());
			break;
		case RpcCalls.SetColor:
			SetColor(reader.ReadByte());
			break;
		case RpcCalls.SetHat:
			SetHat(reader.ReadPackedUInt32());
			break;
		case RpcCalls.SetInfected:
		{
			int num = reader.ReadPackedInt32();
			PlayerControl[] array = new PlayerControl[num];
			for (int i = 0; i < num; i++)
			{
				array[i] = reader.ReadNetObject<PlayerControl>();
			}
			SetInfected(array);
			break;
		}
		case RpcCalls.SyncSettings:
			GameOptions = GameOptionsData.FromBytes(reader.ReadBytesAndSize());
			break;
		case RpcCalls.ReportDeadBody:
		{
			PlayerControl target3 = reader.ReadNetObject<PlayerControl>();
			ReportDeadBody(target3);
			break;
		}
		case RpcCalls.StartMeeting:
		{
			PlayerControl target2 = reader.ReadNetObject<PlayerControl>();
			StartCoroutine(CoStartMeeting(target2));
			break;
		}
		case RpcCalls.MurderPlayer:
		{
			PlayerControl target = reader.ReadNetObject<PlayerControl>();
			MurderPlayer(target);
			break;
		}
		case RpcCalls.CompleteTask:
			CompleteTask(reader.ReadPackedUInt32());
			break;
		case RpcCalls.SendChat:
			SendChat(reader.ReadString());
			break;
		case RpcCalls.TimesImpostor:
			percImpostor = reader.ReadSingle();
			break;
		}
	}
}
