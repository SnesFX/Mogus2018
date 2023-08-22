using System.Collections.Generic;
using System.Linq;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;

public class ShipStatus : InnerNetObject, IDisconnectHandler
{
	public class SystemTypeComparer : IEqualityComparer<SystemTypes>
	{
		public static readonly SystemTypeComparer Instance = new SystemTypeComparer();

		public bool Equals(SystemTypes x, SystemTypes y)
		{
			return x == y;
		}

		public int GetHashCode(SystemTypes obj)
		{
			return (int)obj;
		}
	}

	private enum RpcCalls
	{
		CloseDoorsOfType = 0,
		RepairSystem = 1
	}

	public static ShipStatus Instance;

	public float MaxLightRadius = 100f;

	public float MinLightRadius;

	public Transform SpawnCenter;

	public float SpawnRadius = 1.55f;

	public AudioClip shipHum;

	public NormalPlayerTask[] NormalTasks;

	public PlayerTask[] SpecialTasks;

	public Doorway[] AllDoors;

	public Console[] AllConsoles;

	public Dictionary<SystemTypes, SystemType> Systems;

	public AnimationClip[] WeaponFires;

	public SpriteAnim WeaponsImage;

	public AnimationClip HatchActive;

	public SpriteAnim Hatch;

	public ParticleSystem HatchParticles;

	public AnimationClip ShieldsActive;

	public SpriteAnim[] ShieldsImages;

	public SpriteRenderer ShieldBorder;

	public Sprite ShieldBorderOn;

	public SpriteRenderer MedScanner;

	private int WeaponFireIdx;

	private RaycastHit2D[] volumeBuffer = new RaycastHit2D[5];

	private static readonly SystemTypes[] ReactorTypes = new SystemTypes[2]
	{
		SystemTypes.LifeSupp,
		SystemTypes.Reactor
	};

	public ShipRoom[] AllRooms { get; private set; }

	public ShipStatus()
	{
		Systems = new Dictionary<SystemTypes, SystemType>(SystemTypeComparer.Instance)
		{
			{
				SystemTypes.Electrical,
				new SwitchSystem()
			},
			{
				SystemTypes.MedBay,
				new MedScanSystem()
			},
			{
				SystemTypes.Reactor,
				new ReactorSystemType(SystemTypes.Reactor)
			},
			{
				SystemTypes.LifeSupp,
				new ReactorSystemType(SystemTypes.LifeSupp)
			},
			{
				SystemTypes.Security,
				new SecurityCameraSystemType()
			},
			{
				SystemTypes.Comms,
				new HudOverrideSystemType()
			},
			{
				SystemTypes.Doors,
				new DoorsSystemType()
			}
		};
	}

	private void Awake()
	{
		AllRooms = GetComponentsInChildren<ShipRoom>();
		AllConsoles = GetComponentsInChildren<Console>();
	}

	public void Start()
	{
		Instance = this;
		LobbyBehaviour lobbyBehaviour = Object.FindObjectOfType<LobbyBehaviour>();
		if ((bool)lobbyBehaviour)
		{
			Object.Destroy(lobbyBehaviour.gameObject);
		}
		SoundManager.Instance.StopAllSound();
		AudioSource audioSource = SoundManager.Instance.PlaySound(shipHum, true);
		audioSource.pitch = 0.8f;
		StarGen.instance.transform.position = new Vector3(0f, -5f, 50f);
		StarGen.instance.SetDirection(new Vector2(-1f, 0f));
		StarGen.instance.Length = 28f;
		StarGen.instance.Width = 20f;
		StarGen.instance.RegenPositions();
		if (!Constants.ShouldPlaySfx())
		{
			return;
		}
		for (int i = 0; i < AllRooms.Length; i++)
		{
			ShipRoom room = AllRooms[i];
			if ((bool)room.AmbientSound)
			{
				SoundManager.Instance.PlayDynamicSound("Amb " + room.RoomId, room.AmbientSound, true, delegate(AudioSource player, float dt)
				{
					GetAmbientSoundVolume(room, player, dt);
				});
			}
		}
	}

	public override void OnDestroy()
	{
		SoundManager.Instance.StopAllSound();
		base.OnDestroy();
	}

	public Vector2 GetSpawnLocation(int playerId, int numPlayer)
	{
		Vector2 up = Vector2.up;
		up = up.Rotate((float)(playerId - 1) * (360f / (float)numPlayer));
		up *= SpawnRadius;
		return (Vector2)SpawnCenter.position + up + new Vector2(0f, 0.3636f);
	}

	public void HandleDisconnect(PlayerControl pc)
	{
	}

	public void HandleDisconnect()
	{
	}

	public void StartShields()
	{
		for (int i = 0; i < ShieldsImages.Length; i++)
		{
			ShieldsImages[i].Play(ShieldsActive);
		}
		ShieldBorder.sprite = ShieldBorderOn;
	}

	public void FireWeapon()
	{
		if (!WeaponsImage.IsPlaying())
		{
			WeaponsImage.Play(WeaponFires[WeaponFireIdx]);
			WeaponFireIdx = (WeaponFireIdx + 1) % 2;
		}
	}

	public void OpenHatch()
	{
		if (!Hatch.IsPlaying())
		{
			Hatch.Play(HatchActive);
			HatchParticles.Play();
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		DoorsSystemType doorsSystemType = Systems[SystemTypes.Doors] as DoorsSystemType;
		doorsSystemType.CloseDoorsOfType(room);
		SetDirtyBit(65536u);
	}

	internal void RepairSystem(SystemTypes systemType, PlayerControl player, byte amount)
	{
		Systems[systemType].RepairDamage(player, amount);
		SetDirtyBit((uint)(1 << (int)systemType));
	}

	internal void SelectInfected()
	{
		List<PlayerControl> list = (from pcd in GameData.Instance.AllPlayers
			where !pcd.Disconnected
			select pcd.Object into pc
			where pc
			where !pc.IsDead
			select pc).ToList();
		list.Shuffle();
		PlayerControl[] infected = list.Take(PlayerControl.GameOptions.NumImpostors).ToArray();
		PlayerControl.LocalPlayer.RpcSetInfected(infected);
	}

	public void Begin()
	{
		GameOptionsData gameOptions = PlayerControl.GameOptions;
		List<GameData.PlayerInfo> allPlayers = GameData.Instance.AllPlayers;
		for (int i = 0; i < NormalTasks.Length; i++)
		{
			NormalTasks[i].Index = i;
		}
		List<byte> list = new List<byte>(10);
		List<NormalPlayerTask> list2 = NormalTasks.Where((NormalPlayerTask t) => t.IsCommon).ToList();
		for (int j = 0; j < gameOptions.NumCommonTasks; j++)
		{
			int index = list2.RandomIdx();
			list.Add((byte)list2[index].Index);
			list2.RemoveAt(index);
			if (list2.Count == 0)
			{
				Debug.LogWarning("Not enough common tasks");
				break;
			}
		}
		List<NormalPlayerTask> list3 = NormalTasks.Where((NormalPlayerTask t) => t.IsLong).ToList();
		list3.Shuffle();
		List<NormalPlayerTask> list4 = NormalTasks.Where((NormalPlayerTask t) => !t.IsCommon && !t.IsLong).ToList();
		list4.Shuffle();
		int num = 0;
		int num2 = 0;
		for (byte b = 0; b < allPlayers.Count; b = (byte)(b + 1))
		{
			list.RemoveRange(gameOptions.NumCommonTasks, list.Count - gameOptions.NumCommonTasks);
			for (int k = 0; k < gameOptions.NumLongTasks; k++)
			{
				list.Add((byte)list3[num++].Index);
				if (num >= list3.Count)
				{
					num = 0;
					list3.Shuffle();
					Debug.LogWarning("Not enough long tasks");
				}
			}
			for (int l = 0; l < gameOptions.NumShortTasks; l++)
			{
				NormalPlayerTask newTask = list4[num2++];
				if (list.All((byte t) => NormalTasks[t].TaskType != newTask.TaskType))
				{
					list.Add((byte)newTask.Index);
				}
				else
				{
					l--;
				}
				if (num2 >= list4.Count)
				{
					num2 = 0;
					list4.Shuffle();
					Debug.LogWarning("Not enough normal tasks");
				}
			}
			GameData.PlayerInfo playerInfo = allPlayers[b];
			if ((bool)playerInfo.Object && !playerInfo.Object.GetComponent<DummyBehaviour>().enabled)
			{
				byte[] taskTypeIds = list.ToArray();
				GameData.Instance.RpcSetTasks(playerInfo.Id, taskTypeIds);
			}
		}
		base.enabled = true;
	}

	public void FixedUpdate()
	{
		if ((bool)GameData.Instance)
		{
			GameData.Instance.RecomputeTaskCounts();
		}
		if (AmongUsClient.Instance.AmHost)
		{
			CheckEndCriteria();
		}
		if (!AmongUsClient.Instance.AmClient)
		{
			return;
		}
		for (int i = 0; i < SystemTypeHelpers.AllTypes.Length; i++)
		{
			SystemTypes systemTypes = SystemTypeHelpers.AllTypes[i];
			SystemType value;
			if (Systems.TryGetValue(systemTypes, out value) && value.Detoriorate(Time.fixedDeltaTime))
			{
				SetDirtyBit((uint)(1 << (int)systemTypes));
			}
		}
	}

	private void GetAmbientSoundVolume(ShipRoom room, AudioSource player, float dt)
	{
		if (!PlayerControl.LocalPlayer)
		{
			player.volume = 0f;
			return;
		}
		Vector2 vector = room.transform.position;
		Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
		float num = Vector2.Distance(vector, truePosition);
		if (num > 8f)
		{
			player.volume = 0f;
			return;
		}
		Vector2 direction = truePosition - vector;
		int num2 = Physics2D.RaycastNonAlloc(vector, direction, volumeBuffer, num, Constants.ShipOnlyMask);
		float num3 = 1f - num / 8f - (float)num2 * 0.25f;
		player.volume = Mathf.Lerp(player.volume, num3 * 0.7f, dt);
	}

	public float CalculateLightRadius(PlayerControl player)
	{
		if (player.IsDead)
		{
			return MaxLightRadius;
		}
		SwitchSystem switchSystem = (SwitchSystem)Systems[SystemTypes.Electrical];
		if (player.IsImpostor)
		{
			return MaxLightRadius * PlayerControl.GameOptions.ImpostorLightMod;
		}
		float t = (float)(int)switchSystem.Value / 255f;
		return Mathf.Lerp(MinLightRadius, MaxLightRadius, t) * PlayerControl.GameOptions.CrewLightMod;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			(Systems[SystemTypes.Doors] as DoorsSystemType).SetDoors(AllDoors);
			for (short num = 0; num < SystemTypeHelpers.AllTypes.Length; num = (short)(num + 1))
			{
				SystemTypes key = SystemTypeHelpers.AllTypes[num];
				SystemType value;
				if (Systems.TryGetValue(key, out value))
				{
					value.Serialize(writer, true);
				}
			}
			return true;
		}
		if (DirtyBits != 0)
		{
			writer.WritePacked(DirtyBits);
			for (short num2 = 0; num2 < SystemTypeHelpers.AllTypes.Length; num2 = (short)(num2 + 1))
			{
				SystemTypes systemTypes = SystemTypeHelpers.AllTypes[num2];
				SystemType value2;
				if ((DirtyBits & (1 << (int)systemTypes)) != 0 && Systems.TryGetValue(systemTypes, out value2))
				{
					value2.Serialize(writer, false);
				}
			}
			DirtyBits = 0u;
			return true;
		}
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			(Systems[SystemTypes.Doors] as DoorsSystemType).SetDoors(AllDoors);
			for (short num = 0; num < SystemTypeHelpers.AllTypes.Length; num = (short)(num + 1))
			{
				SystemTypes key = (SystemTypes)num;
				SystemType value;
				if (Systems.TryGetValue(key, out value))
				{
					value.Deserialize(reader, true);
				}
			}
			return;
		}
		uint num2 = reader.ReadPackedUInt32();
		for (short num3 = 0; num3 < SystemTypeHelpers.AllTypes.Length; num3 = (short)(num3 + 1))
		{
			SystemTypes systemTypes = SystemTypeHelpers.AllTypes[num3];
			SystemType value2;
			if ((num2 & (1 << (int)systemTypes)) != 0 && Systems.TryGetValue(systemTypes, out value2))
			{
				value2.Deserialize(reader, false);
			}
		}
	}

	private void CheckEndCriteria()
	{
		if (!GameData.Instance)
		{
			return;
		}
		for (int i = 0; i < ReactorTypes.Length; i++)
		{
			SystemTypes key = ReactorTypes[i];
			ReactorSystemType reactorSystemType = (ReactorSystemType)Systems[key];
			if (reactorSystemType.Countdown < 0f)
			{
				if (!DestroyableSingleton<TutorialManager>.InstanceExists)
				{
					base.enabled = false;
					RpcEndGame(GameOverReason.ImpostorBySabotage, !SaveManager.BoughtNoAds);
				}
				else
				{
					DestroyableSingleton<HudManager>.Instance.ShowPopUp("Normally The Impostor would have just won because of the critical sabotage. Instead we just shut it off.");
					reactorSystemType.Countdown = 10000f;
				}
				break;
			}
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int j = 0; j < GameData.Instance.AllPlayers.Count; j++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[j];
			if (playerInfo.Disconnected)
			{
				continue;
			}
			PlayerControl @object = playerInfo.Object;
			if (!@object)
			{
				continue;
			}
			if (@object.IsImpostor)
			{
				num3++;
			}
			if (!@object.IsDead)
			{
				if (@object.IsImpostor)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				base.enabled = false;
				GameOverReason endReason = ((GameData.Instance.LastDeathReason == DeathReason.Disconnect) ? GameOverReason.ImpostorDisconnect : GameOverReason.HumansByVote);
				RpcEndGame(endReason, !SaveManager.BoughtNoAds);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp("Normally The Crew would have just won because The Impostor is dead. For free play, we revive everyone instead.");
				ReviveEveryone();
			}
		}
		else if (num <= num2)
		{
			if (!DestroyableSingleton<TutorialManager>.InstanceExists)
			{
				base.enabled = false;
				GameOverReason endReason2;
				switch (GameData.Instance.LastDeathReason)
				{
				default:
					endReason2 = GameOverReason.HumansDisconnect;
					break;
				case DeathReason.Kill:
					endReason2 = GameOverReason.ImpostorByKill;
					break;
				case DeathReason.Exile:
					endReason2 = GameOverReason.ImpostorByVote;
					break;
				}
				RpcEndGame(endReason2, !SaveManager.BoughtNoAds);
			}
			else
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp("Normally The Impostor would have just won because The Crew can no longer win. For free play, we revive everyone instead.");
				ReviveEveryone();
			}
		}
		else if (!DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
			{
				base.enabled = false;
				RpcEndGame(GameOverReason.HumansByTask, !SaveManager.BoughtNoAds);
			}
		}
		else if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
		{
			DestroyableSingleton<HudManager>.Instance.ShowPopUp("Normally The Crew would have just won because the task bar is full. For free play, we issue new tasks instead.");
			Begin();
		}
	}

	public bool IsGameOverDueToDeath()
	{
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			GameData.PlayerInfo playerInfo = GameData.Instance.AllPlayers[i];
			if (playerInfo.Disconnected)
			{
				continue;
			}
			PlayerControl @object = playerInfo.Object;
			if (!@object)
			{
				continue;
			}
			if (@object.IsImpostor)
			{
				num3++;
			}
			if (!@object.IsDead)
			{
				if (@object.IsImpostor)
				{
					num2++;
				}
				else
				{
					num++;
				}
			}
		}
		if (num2 <= 0 && (!DestroyableSingleton<TutorialManager>.InstanceExists || num3 > 0))
		{
			return true;
		}
		if (num <= num2)
		{
			return true;
		}
		return false;
	}

	private static void RpcEndGame(GameOverReason endReason, bool showAd)
	{
		MessageWriter messageWriter = AmongUsClient.Instance.StartEndGame();
		messageWriter.Write((byte)endReason);
		messageWriter.Write(showAd);
		AmongUsClient.Instance.FinishEndGame(messageWriter);
	}

	private static void ReviveEveryone()
	{
		for (int i = 0; i < GameData.Instance.AllPlayers.Count; i++)
		{
			PlayerControl @object = GameData.Instance.AllPlayers[i].Object;
			@object.Revive();
		}
		DeadBody[] self = Object.FindObjectsOfType<DeadBody>();
		self.ForEach(delegate(DeadBody b)
		{
			Object.Destroy(b.gameObject);
		});
	}

	public bool CheckTaskCompletion()
	{
		if (DestroyableSingleton<TutorialManager>.InstanceExists)
		{
			if (PlayerControl.LocalPlayer.myTasks.All((PlayerTask t) => t.IsComplete))
			{
				DestroyableSingleton<HudManager>.Instance.ShowPopUp("Normally The Crew would have just won because the task bar is full. For free play, we issue new tasks instead.");
				Begin();
			}
			return false;
		}
		GameData.Instance.RecomputeTaskCounts();
		if (GameData.Instance.TotalTasks <= GameData.Instance.CompletedTasks)
		{
			base.enabled = false;
			RpcEndGame(GameOverReason.HumansByTask, !SaveManager.BoughtNoAds);
			return true;
		}
		return false;
	}

	public void RpcCloseDoorsOfType(SystemTypes type)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			CloseDoorsOfType(type);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 0, SendOption.Reliable);
		messageWriter.Write((byte)type);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public void RpcRepairSystem(SystemTypes systemType, int amount)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			RepairSystem(systemType, PlayerControl.LocalPlayer, (byte)amount);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpcImmediately(NetId, 1, SendOption.Reliable);
		messageWriter.Write((byte)systemType);
		messageWriter.WriteNetObject(PlayerControl.LocalPlayer);
		messageWriter.Write((byte)amount);
		AmongUsClient.Instance.FinishRpcImmediately(messageWriter);
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.CloseDoorsOfType:
			CloseDoorsOfType((SystemTypes)reader.ReadByte());
			break;
		case RpcCalls.RepairSystem:
			RepairSystem((SystemTypes)reader.ReadByte(), reader.ReadNetObject<PlayerControl>(), reader.ReadByte());
			break;
		}
	}
}
