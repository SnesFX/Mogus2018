using System;
using System.Collections.Generic;
using System.Linq;
using Assets.CoreScripts;
using Hazel;
using InnerNet;
using UnityEngine;

public class GameData : InnerNetObject, IDisconnectHandler
{
	public class TaskInfo
	{
		public uint Id;

		public bool Complete;

		public void Serialize(MessageWriter writer)
		{
			writer.WritePacked(Id);
			writer.Write(Complete);
		}

		public void Deserialize(MessageReader reader)
		{
			Id = reader.ReadPackedUInt32();
			Complete = reader.ReadBoolean();
		}
	}

	public class PlayerInfo
	{
		public byte Id;

		public string Name;

		public byte Color;

		public bool Disconnected;

		public List<TaskInfo> Tasks;

		private PlayerControl _object;

		public PlayerControl Object
		{
			get
			{
				if (!_object)
				{
					_object = PlayerControl.AllPlayerControls.FirstOrDefault((PlayerControl p) => p.PlayerId == Id);
				}
				return _object;
			}
		}

		public PlayerInfo()
		{
		}

		public PlayerInfo(PlayerControl pc)
		{
			_object = pc;
			Id = pc.PlayerId;
			Name = pc.PlayerName;
			Color = pc.ColorId;
		}

		public void Serialize(MessageWriter writer)
		{
			writer.Write(Id);
			writer.Write(Name);
			writer.Write(Color);
			writer.Write(Disconnected);
			if (Tasks != null)
			{
				writer.Write((byte)Tasks.Count);
				for (int i = 0; i < Tasks.Count; i++)
				{
					Tasks[i].Serialize(writer);
				}
			}
			else
			{
				writer.Write((byte)0);
			}
		}

		public void Deserialize(MessageReader reader)
		{
			Id = reader.ReadByte();
			Name = reader.ReadString();
			Color = reader.ReadByte();
			Disconnected = reader.ReadBoolean();
			byte b = reader.ReadByte();
			Tasks = new List<TaskInfo>(b);
			for (int i = 0; i < b; i++)
			{
				Tasks.Add(new TaskInfo());
				Tasks[i].Deserialize(reader);
			}
		}

		public TaskInfo FindTaskById(uint taskId)
		{
			for (int i = 0; i < Tasks.Count; i++)
			{
				if (Tasks[i].Id == taskId)
				{
					return Tasks[i];
				}
			}
			return null;
		}
	}

	private enum RpcCalls
	{
		SetTasks = 0,
		CompleteTask = 1
	}

	public static GameData Instance;

	public List<PlayerInfo> AllPlayers = new List<PlayerInfo>();

	public int TotalTasks;

	public int CompletedTasks;

	public DeathReason LastDeathReason;

	public bool GameStarted;

	public void Start()
	{
		if ((bool)Instance && Instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		if (!AmongUsClient.Instance.DisconnectHandlers.Contains(this))
		{
			AmongUsClient.Instance.DisconnectHandlers.Add(this);
		}
		Instance = this;
	}

	public PlayerInfo GetHost()
	{
		ClientData host = AmongUsClient.Instance.GetHost();
		if (host != null && (bool)host.Character)
		{
			return AllPlayers[GetPlayerIndex(host.Character)];
		}
		return null;
	}

	public byte GetAvailableId()
	{
		for (byte b = 0; b < byte.MaxValue; b = (byte)(b + 1))
		{
			bool flag = false;
			for (int i = 0; i < AllPlayers.Count; i++)
			{
				if (AllPlayers[i].Id == b)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				return b;
			}
		}
		return 0;
	}

	public PlayerInfo GetPlayerById(PlayerControl pc)
	{
		for (sbyte b = 0; b < AllPlayers.Count; b = (sbyte)(b + 1))
		{
			if (AllPlayers[b].Object == pc)
			{
				return AllPlayers[b];
			}
		}
		return null;
	}

	public PlayerInfo GetPlayerById(byte id)
	{
		for (sbyte b = 0; b < AllPlayers.Count; b = (sbyte)(b + 1))
		{
			if (AllPlayers[b].Id == id)
			{
				return AllPlayers[b];
			}
		}
		return null;
	}

	public int GetPlayerIndex(PlayerControl pc)
	{
		for (sbyte b = 0; b < AllPlayers.Count; b = (sbyte)(b + 1))
		{
			if (AllPlayers[b].Object == pc)
			{
				return b;
			}
		}
		return -1;
	}

	public void UpdateName(int playerId, string name)
	{
		for (int num = AllPlayers.Count - 1; num > -1; num--)
		{
			if (AllPlayers[num].Id == playerId)
			{
				AllPlayers[num].Name = name;
				SetDirtyBit(1u);
			}
		}
	}

	public void UpdateColor(int playerId, byte color)
	{
		for (int num = AllPlayers.Count - 1; num > -1; num--)
		{
			if (AllPlayers[num].Id == playerId)
			{
				AllPlayers[num].Color = color;
				SetDirtyBit(1u);
			}
		}
	}

	public void AddPlayer(PlayerControl pc)
	{
		AllPlayers.Add(new PlayerInfo(pc));
	}

	public void ReclaimDisconnected(int idx, PlayerControl pc)
	{
		Debug.Log("Reclaimed :" + idx);
		PlayerInfo playerInfo = AllPlayers[idx];
		playerInfo.Disconnected = false;
		playerInfo.Id = pc.PlayerId;
		playerInfo.Color = pc.ColorId;
		SetDirtyBit(1u);
	}

	public bool RemovePlayer(PlayerControl pc)
	{
		bool result = false;
		for (int num = AllPlayers.Count - 1; num > -1; num--)
		{
			if (AllPlayers[num].Object == pc)
			{
				AllPlayers.RemoveAt(num);
				result = true;
			}
		}
		SetDirtyBit(1u);
		return result;
	}

	public void RecomputeTaskCounts()
	{
		TotalTasks = 0;
		CompletedTasks = 0;
		for (int i = 0; i < AllPlayers.Count; i++)
		{
			PlayerInfo playerInfo = AllPlayers[i];
			if (playerInfo.Disconnected || playerInfo.Tasks == null || !playerInfo.Object || (!PlayerControl.GameOptions.GhostsDoTasks && playerInfo.Object.IsDead) || playerInfo.Object.IsImpostor)
			{
				continue;
			}
			for (int j = 0; j < playerInfo.Tasks.Count; j++)
			{
				TotalTasks++;
				if (playerInfo.Tasks[j].Complete)
				{
					CompletedTasks++;
				}
			}
		}
	}

	public void TutOnlyRemoveTask(byte playerId, uint taskId)
	{
		PlayerInfo playerInfo = AllPlayers[playerId];
		TaskInfo item = playerInfo.FindTaskById(taskId);
		playerInfo.Tasks.Remove(item);
		RecomputeTaskCounts();
	}

	public void TutOnlyAddTask(byte playerId, uint taskId)
	{
		PlayerInfo playerInfo = AllPlayers[playerId];
		playerInfo.Tasks.Add(new TaskInfo
		{
			Id = taskId
		});
		TotalTasks++;
	}

	private void SetTasks(byte playerId, byte[] taskTypeIds)
	{
		PlayerInfo playerById = GetPlayerById(playerId);
		if (playerById == null)
		{
			Debug.Log("Could not set tasks for player id: " + playerId);
			return;
		}
		if (!playerById.Object)
		{
			Debug.Log(string.Format("Could not set tasks for player ({0}): ", playerById.Name) + playerId);
			return;
		}
		playerById.Tasks = new List<TaskInfo>(taskTypeIds.Length);
		for (int i = 0; i < taskTypeIds.Length; i++)
		{
			playerById.Tasks.Add(new TaskInfo());
			playerById.Tasks[i].Id = (uint)i;
		}
		playerById.Object.SetTasks(taskTypeIds);
	}

	public void CompleteTask(PlayerControl pc, uint taskId)
	{
		int playerIndex = GetPlayerIndex(pc);
		PlayerInfo playerInfo = AllPlayers[playerIndex];
		TaskInfo taskInfo = playerInfo.FindTaskById(taskId);
		if (taskInfo != null)
		{
			if (!taskInfo.Complete)
			{
				taskInfo.Complete = true;
				CompletedTasks++;
			}
			else
			{
				Debug.LogWarning("Double complete task: " + taskId);
			}
		}
		else
		{
			Debug.LogWarning("Couldn't find task: " + taskId);
		}
	}

	public void HandleDisconnect(PlayerControl player)
	{
		if (GameStarted)
		{
			int playerIndex = GetPlayerIndex(player);
			if (playerIndex > 0)
			{
				AllPlayers[playerIndex].Disconnected = true;
				LastDeathReason = DeathReason.Disconnect;
				DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(player.PlayerName + " left the game");
			}
		}
		else if (RemovePlayer(player))
		{
			DestroyableSingleton<HudManager>.Instance.Notifier.AddItem(player.PlayerName + " left the game");
		}
		RecomputeTaskCounts();
		DestroyableSingleton<Telemetry>.Instance.WriteDisconnect(player.PlayerId);
	}

	public void HandleDisconnect()
	{
		if (GameStarted)
		{
			return;
		}
		for (int num = AllPlayers.Count - 1; num >= 0; num--)
		{
			if (!AllPlayers[num].Object)
			{
				AllPlayers.RemoveAt(num);
			}
		}
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			if (!DestroyableSingleton<Telemetry>.Instance.IsInitialized)
			{
				DestroyableSingleton<Telemetry>.Instance.Initialize();
			}
			writer.WriteBytesAndSize(DestroyableSingleton<Telemetry>.Instance.CurrentGuid.ToByteArray());
		}
		writer.Write(GameStarted);
		writer.Write((byte)AllPlayers.Count);
		for (int i = 0; i < AllPlayers.Count; i++)
		{
			PlayerInfo playerInfo = AllPlayers[i];
			playerInfo.Serialize(writer);
		}
		DirtyBits = 0u;
		return true;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		TotalTasks = 0;
		CompletedTasks = 0;
		if (initialState)
		{
			Guid gameGuid = new Guid(reader.ReadBytesAndSize());
			if (!DestroyableSingleton<Telemetry>.Instance.IsInitialized)
			{
				DestroyableSingleton<Telemetry>.Instance.Initialize(gameGuid);
			}
		}
		GameStarted = reader.ReadBoolean();
		byte b = reader.ReadByte();
		AllPlayers.Clear();
		for (int i = 0; i < b; i++)
		{
			PlayerInfo playerInfo = new PlayerInfo();
			playerInfo.Deserialize(reader);
			AllPlayers.Add(playerInfo);
		}
	}

	public void RpcSetTasks(byte playerId, byte[] taskTypeIds)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			SetTasks(playerId, taskTypeIds);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 0);
		messageWriter.Write(playerId);
		messageWriter.WriteBytesAndSize(taskTypeIds);
		messageWriter.EndMessage();
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.SetTasks:
			SetTasks(reader.ReadByte(), reader.ReadBytesAndSize());
			break;
		case RpcCalls.CompleteTask:
			CompleteTask(reader.ReadNetObject<PlayerControl>(), reader.ReadPackedUInt32());
			break;
		}
	}
}
