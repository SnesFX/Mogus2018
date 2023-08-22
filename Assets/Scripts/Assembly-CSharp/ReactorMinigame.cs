using UnityEngine;

public class ReactorMinigame : Minigame
{
	private Color bad = new Color(1f, 0.16078432f, 0f);

	private Color good = new Color(0.3019608f, 0.8862745f, 71f / 85f);

	private ReactorSystemType reactor;

	public TextRenderer statusText;

	public SpriteRenderer hand;

	private FloatRange YSweep = new FloatRange(-2.15f, 1.56f);

	public SpriteRenderer sweeper;

	public AudioClip HandSound;

	private bool isButtonDown;

	public override void Begin(PlayerTask task)
	{
		ShipStatus instance = ShipStatus.Instance;
		if (!instance)
		{
			reactor = new ReactorSystemType(SystemTypes.Reactor);
		}
		else
		{
			reactor = instance.Systems[SystemTypes.Reactor] as ReactorSystemType;
		}
		hand.color = bad;
	}

	public void ButtonDown()
	{
		if (((bool)PlayerControl.LocalPlayer && PlayerControl.LocalPlayer.IsImpostor) || !reactor.IsActive)
		{
			return;
		}
		isButtonDown = !isButtonDown;
		if (isButtonDown)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(HandSound, true);
			}
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, (byte)(0x40 | base.ConsoleId));
		}
		else
		{
			SoundManager.Instance.StopSound(HandSound);
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, (byte)(0x20 | base.ConsoleId));
		}
	}

	public void FixedUpdate()
	{
		if (!reactor.IsActive)
		{
			if (amClosing == CloseState.None)
			{
				hand.color = good;
				statusText.Text = "Reactor Nominal";
				sweeper.enabled = false;
				SoundManager.Instance.StopSound(HandSound);
				StartCoroutine(CoStartClose());
			}
		}
		else if (!isButtonDown)
		{
			statusText.Text = "Hold to stop meltdown";
			sweeper.enabled = false;
		}
		else
		{
			statusText.Text = "Waiting for second user";
			Vector3 localPosition = sweeper.transform.localPosition;
			localPosition.y = YSweep.Lerp(Mathf.Sin(Time.time) * 0.5f + 0.5f);
			sweeper.transform.localPosition = localPosition;
			sweeper.enabled = true;
		}
	}

	public override void Close(bool allowMovement)
	{
		SoundManager.Instance.StopSound(HandSound);
		if ((bool)ShipStatus.Instance)
		{
			ShipStatus.Instance.RpcRepairSystem(SystemTypes.Reactor, (byte)(0x20 | base.ConsoleId));
		}
		base.Close(allowMovement);
	}
}
