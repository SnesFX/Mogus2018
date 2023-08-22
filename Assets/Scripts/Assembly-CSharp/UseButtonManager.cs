using UnityEngine;

public class UseButtonManager : MonoBehaviour
{
	private static readonly Color DisabledColor = new Color(1f, 1f, 1f, 0.3f);

	private static readonly Color EnabledColor = new Color(1f, 1f, 1f, 1f);

	public SpriteRenderer UseButton;

	public Sprite UseImage;

	public Sprite SabotageImage;

	public Sprite VentImage;

	public Sprite AdminMapImage;

	public Sprite SecurityImage;

	public Sprite OptionsImage;

	private IUsable previousTarget;

	public void SetTarget(IUsable target)
	{
		if (previousTarget != null && previousTarget != target)
		{
			previousTarget.SetOutline(true, false);
			previousTarget = null;
		}
		if (target != null)
		{
			if (target is Vent)
			{
				UseButton.sprite = VentImage;
			}
			else if (target is MapConsole)
			{
				UseButton.sprite = AdminMapImage;
			}
			else if (target is OptionsConsole)
			{
				UseButton.sprite = OptionsImage;
			}
			else if (target is SystemConsole)
			{
				SystemConsole systemConsole = (SystemConsole)target;
				if (systemConsole.name.StartsWith("Surv"))
				{
					UseButton.sprite = SecurityImage;
				}
				else if (systemConsole.name.StartsWith("TaskAdd"))
				{
					UseButton.sprite = OptionsImage;
				}
				else
				{
					UseButton.sprite = UseImage;
				}
			}
			else
			{
				UseButton.sprite = UseImage;
			}
			UseButton.SetCooldownNormalizedUvs();
			target.SetOutline(true, true);
			previousTarget = target;
			UseButton.material.SetFloat("_Percent", target.PercentCool);
			UseButton.color = EnabledColor;
		}
		else if (PlayerControl.LocalPlayer.IsImpostor && PlayerControl.LocalPlayer.canMove)
		{
			UseButton.sprite = SabotageImage;
			UseButton.SetCooldownNormalizedUvs();
			UseButton.color = EnabledColor;
		}
		else
		{
			UseButton.sprite = UseImage;
			UseButton.color = DisabledColor;
		}
	}

	public void DoClick()
	{
		if (base.isActiveAndEnabled)
		{
			if (previousTarget != null)
			{
				PlayerControl.LocalPlayer.UseClosest();
			}
			else if (PlayerControl.LocalPlayer.IsImpostor)
			{
				DestroyableSingleton<HudManager>.Instance.Map.ShowInfectedMap();
			}
		}
	}

	internal void Refresh()
	{
		SetTarget(previousTarget);
	}
}
