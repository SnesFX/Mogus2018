using System.Collections.Generic;
using System.Linq;
using Hazel;

public class ReactorSystemType : SystemType, IActivatable
{
	public const byte StartCountdown = 128;

	public const byte AddUserOp = 64;

	public const byte RemoveUserOp = 32;

	public const byte ClearCountdown = 16;

	public const float CountdownStopped = 10000f;

	public const float LifeSuppDuration = 45f;

	public const float ReactorDuration = 30f;

	public const byte ConsoleIdMask = 3;

	public const byte RequiredUserCount = 2;

	public float Countdown = 10000f;

	private HashSet<byte>[] Users = new HashSet<byte>[5];

	private SystemTypes systemType;

	public int UserCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < Users.Length; i++)
			{
				if (Users[i].Count > 0)
				{
					num++;
				}
			}
			return num;
		}
	}

	public bool IsActive
	{
		get
		{
			return Countdown < 10000f;
		}
	}

	public ReactorSystemType(SystemTypes system)
	{
		systemType = system;
		for (int i = 0; i < Users.Length; i++)
		{
			Users[i] = new HashSet<byte>();
		}
	}

	public bool GetConsoleComplete(int consoleId)
	{
		if (consoleId < 0 || consoleId >= Users.Length)
		{
			return false;
		}
		return Users[consoleId].Count > 0;
	}

	public override void RepairDamage(PlayerControl player, byte opCode)
	{
		int num = opCode & 3;
		if (opCode == 128 && !IsActive)
		{
			if (systemType == SystemTypes.LifeSupp)
			{
				Countdown = 45f;
			}
			else
			{
				Countdown = 30f;
			}
			Users.ForEach(delegate(HashSet<byte> h)
			{
				h.Clear();
			});
		}
		else if (opCode == 16)
		{
			Countdown = 10000f;
		}
		else if (opCode.HasAnyBit((byte)64))
		{
			Users[num].Add(player.PlayerId);
			if (UserCount >= 2)
			{
				Countdown = 10000f;
			}
		}
		else if (opCode.HasAnyBit((byte)32))
		{
			Users[num].Remove(player.PlayerId);
		}
	}

	public override bool Detoriorate(float deltaTime)
	{
		if (systemType == SystemTypes.Reactor)
		{
			if (IsActive)
			{
				if (DestroyableSingleton<HudManager>.Instance.reactorFlash == null)
				{
					PlayerControl.LocalPlayer.AddSystemTask(systemType);
				}
				Countdown -= deltaTime;
				return true;
			}
			if (DestroyableSingleton<HudManager>.Instance.reactorFlash != null)
			{
				ReactorShipRoom reactorShipRoom = (ReactorShipRoom)ShipStatus.Instance.AllRooms.First((ShipRoom r) => r.RoomId == SystemTypes.Reactor);
				reactorShipRoom.StopMeltdown();
			}
		}
		else
		{
			if (IsActive)
			{
				if (DestroyableSingleton<HudManager>.Instance.oxyFlash == null)
				{
					PlayerControl.LocalPlayer.AddSystemTask(systemType);
				}
				Countdown -= deltaTime;
				return true;
			}
			if (DestroyableSingleton<HudManager>.Instance.oxyFlash != null)
			{
				DestroyableSingleton<HudManager>.Instance.StopOxyFlash();
			}
		}
		return false;
	}

	public override void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(Countdown);
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		Countdown = reader.ReadSingle();
	}
}
