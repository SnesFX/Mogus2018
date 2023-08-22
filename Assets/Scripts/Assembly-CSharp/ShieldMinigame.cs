using UnityEngine;

public class ShieldMinigame : Minigame
{
	public Color OnColor = Color.white;

	public Color OffColor = Color.red;

	public SpriteRenderer[] Shields;

	public SpriteRenderer Gauge;

	private byte shields;

	public AudioClip ShieldOnSound;

	public AudioClip ShieldOffSound;

	public override void Begin(PlayerTask task)
	{
		base.Begin(task);
		shields = MyNormTask.Data[0];
		UpdateButtons();
	}

	public void ToggleShield(int i)
	{
		if (MyNormTask.IsComplete)
		{
			return;
		}
		byte b = (byte)(1 << i);
		shields ^= b;
		MyNormTask.Data[0] = shields;
		if ((shields & b) != 0)
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(ShieldOnSound, false);
			}
		}
		else if (Constants.ShouldPlaySfx())
		{
			SoundManager.Instance.PlaySound(ShieldOffSound, false);
		}
		if (shields == 127)
		{
			MyNormTask.NextStep();
			StartCoroutine(CoStartClose());
			if (!ShipStatus.Instance.ShieldsImages[0].IsPlaying())
			{
				ShipStatus.Instance.StartShields();
				PlayerControl.LocalPlayer.RpcPlayAnimation(1);
			}
		}
	}

	public void FixedUpdate()
	{
		UpdateButtons();
	}

	private void UpdateButtons()
	{
		int num = 0;
		for (int i = 0; i < Shields.Length; i++)
		{
			bool flag = (shields & (1 << i)) == 0;
			if (!flag)
			{
				num++;
			}
			Shields[i].color = ((!flag) ? OnColor : OffColor);
		}
		if (shields == 127)
		{
			Gauge.transform.Rotate(0f, 0f, Time.fixedDeltaTime * 45f);
			Gauge.color = new Color(1f, 1f, 1f, 1f);
		}
		else
		{
			float num2 = Mathf.Lerp(0.1f, 0.5f, (float)num / 6f);
			Gauge.color = new Color(1f, num2, num2, 1f);
		}
	}
}
