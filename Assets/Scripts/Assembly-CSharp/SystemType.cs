using Hazel;

public abstract class SystemType
{
	public const byte MaxValue = byte.MaxValue;

	public const byte HalfValue = 128;

	public abstract bool Detoriorate(float deltaTime);

	public abstract void RepairDamage(PlayerControl player, byte amount);

	public abstract void Serialize(MessageWriter writer, bool initialState);

	public abstract void Deserialize(MessageReader reader, bool initialState);

	protected static bool HasTask<T>()
	{
		for (int num = PlayerControl.LocalPlayer.myTasks.Count - 1; num > 0; num--)
		{
			PlayerTask playerTask = PlayerControl.LocalPlayer.myTasks[num];
			if (playerTask is T)
			{
				return true;
			}
		}
		return false;
	}
}
