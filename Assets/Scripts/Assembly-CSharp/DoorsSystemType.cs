using System.Linq;
using Hazel;

public class DoorsSystemType : SystemType, IActivatable
{
	private Doorway[] doors;

	private uint dirtyBits;

	public bool IsActive
	{
		get
		{
			return doors.Any((Doorway b) => !b.Open);
		}
	}

	public void SetDoors(Doorway[] doors)
	{
		this.doors = doors;
	}

	public override bool Detoriorate(float deltaTime)
	{
		if (doors == null)
		{
			return false;
		}
		for (int i = 0; i < doors.Length; i++)
		{
			if (doors[i].DoUpdate(deltaTime))
			{
				dirtyBits |= (uint)(1 << i);
			}
		}
		return dirtyBits != 0;
	}

	public override void RepairDamage(PlayerControl player, byte amount)
	{
	}

	public override void Serialize(MessageWriter writer, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < doors.Length; i++)
			{
				doors[i].Serialize(writer);
			}
			return;
		}
		writer.WritePacked(dirtyBits);
		for (int j = 0; j < doors.Length; j++)
		{
			if ((dirtyBits & (uint)(1 << j)) != 0)
			{
				doors[j].Serialize(writer);
			}
		}
		dirtyBits = 0u;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		if (initialState)
		{
			for (int i = 0; i < doors.Length; i++)
			{
				doors[i].Deserialize(reader);
			}
			return;
		}
		uint num = reader.ReadPackedUInt32();
		for (int j = 0; j < doors.Length; j++)
		{
			if ((num & (uint)(1 << j)) != 0)
			{
				doors[j].Deserialize(reader);
			}
		}
	}

	public void CloseDoorsOfType(SystemTypes room)
	{
		for (int i = 0; i < doors.Length; i++)
		{
			Doorway doorway = doors[i];
			if (doorway.Room == room)
			{
				doorway.SetDoorway(false);
				dirtyBits |= (uint)(1 << i);
			}
		}
	}

	public float GetTimer(SystemTypes room)
	{
		for (int i = 0; i < doors.Length; i++)
		{
			Doorway doorway = doors[i];
			if (doorway.Room == room)
			{
				return doorway.CooldownTimer;
			}
		}
		return 0f;
	}
}
