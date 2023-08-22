using Hazel;

internal class HudOverrideSystemType : SystemType, IActivatable
{
	public const byte DamageBit = 128;

	public const byte TaskMask = 127;

	public byte TaskState = 2;

	private bool amDirty;

	public bool IsActive
	{
		get
		{
			return TaskState != 2;
		}
	}

	public override bool Detoriorate(float deltaTime)
	{
		if (TaskState == 0)
		{
			TaskState = 1;
			amDirty = true;
			PlayerControl.LocalPlayer.AddSystemTask(SystemTypes.Comms);
		}
		return amDirty;
	}

	public override void RepairDamage(PlayerControl player, byte amount)
	{
		if ((amount & 0x80u) != 0)
		{
			TaskState = 0;
		}
		else
		{
			TaskState = 2;
		}
		amDirty = true;
	}

	public override void Serialize(MessageWriter writer, bool initialState)
	{
		writer.Write(TaskState);
		amDirty = false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
		TaskState = reader.ReadByte();
		amDirty = false;
	}
}
