using PowerTools;
using UnityEngine;

public class Vent : MonoBehaviour, IUsable
{
	public int Id;

	public Vent Left;

	public Vent Right;

	public ButtonBehavior[] Buttons;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	private static readonly Vector3 CollOffset = new Vector3(0f, -0.3636057f, 0f);

	private SpriteRenderer myRend;

	public float UsableDistance
	{
		get
		{
			return 0.75f;
		}
	}

	public float PercentCool
	{
		get
		{
			return 0f;
		}
	}

	private void Start()
	{
		SetButtons(false);
		myRend = GetComponent<SpriteRenderer>();
	}

	public void SetButtons(bool enabled)
	{
		Vent[] array = new Vent[2] { Right, Left };
		for (int i = 0; i < Buttons.Length; i++)
		{
			ButtonBehavior buttonBehavior = Buttons[i];
			if (enabled)
			{
				Vent vent = array[i];
				if ((bool)vent)
				{
					buttonBehavior.gameObject.SetActive(true);
					Vector3 localPosition = (vent.transform.position - base.transform.position).normalized * 0.7f;
					localPosition.y -= 0.08f;
					localPosition.z = -10f;
					buttonBehavior.transform.localPosition = localPosition;
					buttonBehavior.transform.LookAt2d(vent.transform);
				}
				else
				{
					buttonBehavior.gameObject.SetActive(false);
				}
			}
			else
			{
				buttonBehavior.gameObject.SetActive(false);
			}
		}
	}

	public bool CanUse(PlayerControl pc)
	{
		return pc.IsImpostor && (pc.canMove || pc.inVent);
	}

	public void SetOutline(bool on, bool mainTarget)
	{
		myRend.material.SetFloat("_Outline", on ? 1 : 0);
		myRend.material.SetColor("_OutlineColor", Color.red);
		myRend.material.SetColor("_AddColor", (!mainTarget) ? Color.clear : Color.red);
	}

	public void ClickRight()
	{
		if ((bool)Right)
		{
			DoMove(Right.transform.position - CollOffset);
			SetButtons(false);
			Right.SetButtons(true);
		}
	}

	public void ClickLeft()
	{
		if ((bool)Left)
		{
			DoMove(Left.transform.position - CollOffset);
			SetButtons(false);
			Left.SetButtons(true);
		}
	}

	private static void DoMove(Vector3 pos)
	{
		PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(pos);
		if (Constants.ShouldPlaySfx())
		{
			AudioSource audioSource = SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.VentMoveSounds.Random(), false);
			audioSource.pitch = FloatRange.Next(0.8f, 1.2f);
		}
	}

	public void Use()
	{
		PlayerControl localPlayer = PlayerControl.LocalPlayer;
		if (CanUse(localPlayer))
		{
			if (Constants.ShouldPlaySfx())
			{
				SoundManager.Instance.StopSound(PlayerControl.LocalPlayer.VentEnterSound);
				AudioSource audioSource = SoundManager.Instance.PlaySound(PlayerControl.LocalPlayer.VentEnterSound, false);
				audioSource.pitch = FloatRange.Next(0.8f, 1.2f);
			}
			PlayerPhysics component = localPlayer.GetComponent<PlayerPhysics>();
			if (localPlayer.inVent)
			{
				component.RpcExitVent(Id);
				SetButtons(false);
			}
			else
			{
				Vector2 vector = base.transform.position;
				component.RpcEnterVent(Id, vector);
				SetButtons(true);
			}
		}
	}

	internal void EnterVent()
	{
		GetComponent<SpriteAnim>().Play(EnterVentAnim);
	}

	internal void ExitVent()
	{
		GetComponent<SpriteAnim>().Play(ExitVentAnim);
	}
}
