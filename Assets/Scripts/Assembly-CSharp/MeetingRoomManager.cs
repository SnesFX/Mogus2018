public class MeetingRoomManager : IDisconnectHandler
{
	public static readonly MeetingRoomManager Instance = new MeetingRoomManager();

	private PlayerControl reporter;

	public void AssignSelf(PlayerControl reporter)
	{
		this.reporter = reporter;
		if (!AmongUsClient.Instance.DisconnectHandlers.Contains(this))
		{
			AmongUsClient.Instance.DisconnectHandlers.Add(this);
		}
	}

	public void RemoveSelf()
	{
		AmongUsClient.Instance.DisconnectHandlers.Remove(this);
	}

	public void HandleDisconnect(PlayerControl pc)
	{
		if (AmongUsClient.Instance.AmHost)
		{
			DestroyableSingleton<HudManager>.Instance.OpenMeetingRoom(reporter);
		}
	}

	public void HandleDisconnect()
	{
		HandleDisconnect(null);
	}
}
