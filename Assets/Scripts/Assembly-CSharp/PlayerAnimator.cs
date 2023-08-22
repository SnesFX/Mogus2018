using System.Collections;
using PowerTools;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
	public float Speed = 2.5f;

	public VirtualJoystick joystick;

	public SpriteRenderer UseButton;

	public FingerBehaviour finger;

	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	private Vector2 velocity;

	[HideInInspector]
	private SpriteAnim Animator;

	[HideInInspector]
	private SpriteRenderer rend;

	public int NearbyConsoles;

	private void Start()
	{
		Animator = GetComponent<SpriteAnim>();
		rend = GetComponent<SpriteRenderer>();
		rend.material.SetColor("_BackColor", Palette.ShadowColors[0]);
		rend.material.SetColor("_BodyColor", Palette.PlayerColors[0]);
		rend.material.SetColor("_VisorColor", Palette.VisorColor);
	}

	public void FixedUpdate()
	{
		base.transform.Translate(velocity * Time.fixedDeltaTime);
		UseButton.enabled = NearbyConsoles > 0;
	}

	public void LateUpdate()
	{
		if (velocity.sqrMagnitude >= 0.1f)
		{
			if (Animator.GetCurrentAnimation() != RunAnim)
			{
				Animator.Play(RunAnim);
			}
			rend.flipX = velocity.x < 0f;
		}
		else if (Animator.GetCurrentAnimation() == RunAnim)
		{
			Animator.Play(IdleAnim);
		}
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, bool relax, float tolerance = 0.01f)
	{
		worldPos.y += 0.3636f;
		if (!(joystick is DemoKeyboardStick))
		{
			finger.ClickOn();
		}
		while (true)
		{
			Vector2 vector;
			Vector2 del = (vector = worldPos - (Vector2)base.transform.position);
			vector = vector;
			if (!(vector.sqrMagnitude > tolerance))
			{
				break;
			}
			float mag = Mathf.Clamp(del.magnitude * 2f, 0.01f, 1f);
			velocity = del.normalized * Speed * mag;
			joystick.UpdateJoystick(finger, velocity, true);
			yield return null;
		}
		if (relax)
		{
			finger.ClickOff();
			velocity = Vector2.zero;
			joystick.UpdateJoystick(finger, velocity, false);
		}
	}
}
