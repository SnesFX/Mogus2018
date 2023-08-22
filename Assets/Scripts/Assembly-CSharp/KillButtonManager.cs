using UnityEngine;

public class KillButtonManager : MonoBehaviour
{
	public PlayerControl CurrentTarget;

	public SpriteRenderer renderer;

	public bool isCoolingDown = true;

	public bool isActive;

	private Vector2 uv;

	public void Start()
	{
		renderer.SetCooldownNormalizedUvs();
		SetTarget(null);
	}

	public void PerformKill()
	{
		if (base.isActiveAndEnabled && (bool)CurrentTarget && !isCoolingDown && !PlayerControl.LocalPlayer.IsDead)
		{
			PlayerControl.LocalPlayer.RpcMurderPlayer(CurrentTarget);
			SetCoolDown(1f);
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.KillSfx, false, 0.8f);
			}
			SetTarget(null);
		}
	}

	public void SetTarget(PlayerControl target)
	{
		if ((bool)CurrentTarget && CurrentTarget != target)
		{
			SpriteRenderer component = CurrentTarget.GetComponent<SpriteRenderer>();
			component.material.SetFloat("_Outline", 0f);
		}
		CurrentTarget = target;
		if ((bool)CurrentTarget)
		{
			SpriteRenderer component2 = CurrentTarget.GetComponent<SpriteRenderer>();
			component2.material.SetFloat("_Outline", isActive ? 1 : 0);
			component2.material.SetColor("_OutlineColor", Color.red);
			renderer.color = Palette.EnabledColor;
			renderer.material.SetFloat("_Desat", 0f);
		}
		else
		{
			renderer.color = Palette.DisabledColor;
			renderer.material.SetFloat("_Desat", 1f);
		}
	}

	public void SetCoolDown(float percentCool)
	{
		if ((bool)renderer)
		{
			renderer.material.SetFloat("_Percent", percentCool);
		}
		isCoolingDown = percentCool > 0f;
	}
}
