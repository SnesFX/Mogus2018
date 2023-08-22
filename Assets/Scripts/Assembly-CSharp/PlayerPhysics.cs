using System.Collections;
using System.Linq;
using Assets.CoreScripts;
using Hazel;
using InnerNet;
using PowerTools;
using UnityEngine;

public class PlayerPhysics : InnerNetObject
{
	private enum RpcCalls
	{
		EnterVent = 0,
		ExitVent = 1,
		ExitAllVents = 2
	}

	public float Speed = 4.5f;

	[HideInInspector]
	private Rigidbody2D body;

	[HideInInspector]
	private SpriteAnim Animator;

	[HideInInspector]
	private SpriteRenderer rend;

	[HideInInspector]
	private PlayerControl myPlayer;

	public AnimationClip RunAnim;

	public AnimationClip IdleAnim;

	public AnimationClip GhostIdleAnim;

	public AnimationClip EnterVentAnim;

	public AnimationClip ExitVentAnim;

	public void Start()
	{
		body = GetComponent<Rigidbody2D>();
		Animator = GetComponent<SpriteAnim>();
		rend = GetComponent<SpriteRenderer>();
		myPlayer = GetComponent<PlayerControl>();
	}

	private void FixedUpdate()
	{
		HandleAnimation();
		if (base.AmOwner && myPlayer.canMove)
		{
			DestroyableSingleton<Telemetry>.Instance.WritePosition(myPlayer.PlayerId, base.transform.position);
			GameOptionsData gameOptions = PlayerControl.GameOptions;
			body.velocity = DestroyableSingleton<HudManager>.Instance.joystick.Delta * gameOptions.PlayerSpeedMod * Speed;
		}
	}

	private void LateUpdate()
	{
		Vector3 position = base.transform.position;
		position.z = position.y / 1000f;
		base.transform.position = position;
	}

	public Vector3 Vec2ToPosition(Vector2 pos)
	{
		return new Vector3(pos.x, pos.y, pos.y / 1000f);
	}

	public void ResetAnim()
	{
		if (!myPlayer.IsDead)
		{
			Animator.Play(IdleAnim);
			myPlayer.Visible = true;
			myPlayer.SetHatAlpha(1f);
		}
		else
		{
			Animator.Play(GhostIdleAnim);
			myPlayer.SetHatAlpha(0.5f);
		}
	}

	private void HandleAnimation()
	{
		Vector2 velocity = body.velocity;
		if (!myPlayer.IsDead)
		{
			if (velocity.sqrMagnitude >= 0.05f)
			{
				if (Animator.GetCurrentAnimation() != RunAnim)
				{
					Animator.Play(RunAnim);
				}
				if (velocity.x < -0.01f)
				{
					rend.flipX = true;
				}
				else if (velocity.x > 0.01f)
				{
					rend.flipX = false;
				}
			}
			else if (Animator.GetCurrentAnimation() == RunAnim)
			{
				Animator.Play(IdleAnim);
				myPlayer.SetHatAlpha(1f);
			}
		}
		else
		{
			if (Animator.GetCurrentAnimation() != GhostIdleAnim)
			{
				Animator.Play(GhostIdleAnim);
				myPlayer.SetHatAlpha(0.5f);
			}
			if (velocity.x < -0.01f)
			{
				rend.flipX = true;
			}
			else if (velocity.x > 0.01f)
			{
				rend.flipX = false;
			}
		}
	}

	public void EnterVent(int id, Vector3 pos)
	{
		StartCoroutine(CoEnterVent(id, pos));
	}

	public void ExitVent(int id)
	{
		StartCoroutine(CoExitVent(id));
	}

	public void ExitAllVents()
	{
		Vent[] array = Object.FindObjectsOfType<Vent>();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetButtons(false);
		}
		myPlayer.Visible = true;
		Animator.Play(IdleAnim);
		myPlayer.inVent = false;
	}

	private IEnumerator CoEnterVent(int id, Vector2 pos)
	{
		Vent vent = Object.FindObjectsOfType<Vent>().FirstOrDefault((Vent v) => v.Id == id);
		myPlayer.canMove = false;
		yield return WalkPlayerTo(pos);
		vent.EnterVent();
		yield return new WaitForAnimationFinish(Animator, EnterVentAnim);
		Animator.Play(IdleAnim);
		myPlayer.Visible = false;
		myPlayer.inVent = true;
	}

	private IEnumerator CoExitVent(int id)
	{
		Vent vent = Object.FindObjectsOfType<Vent>().FirstOrDefault((Vent v) => v.Id == id);
		myPlayer.Visible = true;
		vent.ExitVent();
		yield return new WaitForAnimationFinish(Animator, ExitVentAnim);
		Animator.Play(IdleAnim);
		myPlayer.inVent = false;
		myPlayer.canMove = true;
	}

	public IEnumerator WalkPlayerTo(Vector2 worldPos, float tolerance = 0.01f)
	{
		worldPos -= GetComponent<CircleCollider2D>().offset;
		do
		{
			Vector2 vector;
			Vector2 del = (vector = worldPos - (Vector2)base.transform.position);
			vector = vector;
			if (!(vector.sqrMagnitude > tolerance))
			{
				break;
			}
			float mag = Mathf.Clamp(del.magnitude * 2f, 0.01f, 1f);
			body.velocity = del.normalized * Speed * mag;
			yield return null;
		}
		while (!(body.velocity.magnitude < 0.0001f));
		body.velocity = Vector2.zero;
	}

	public override bool Serialize(MessageWriter writer, bool initialState)
	{
		return false;
	}

	public override void Deserialize(MessageReader reader, bool initialState)
	{
	}

	public void RpcEnterVent(int id, Vector3 pos)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			EnterVent(id, pos);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 0);
		messageWriter.WritePacked(id);
		messageWriter.Write(pos.x);
		messageWriter.Write(pos.y);
		messageWriter.EndMessage();
	}

	public void RpcExitVent(int id)
	{
		if (AmongUsClient.Instance.AmClient)
		{
			ExitVent(id);
		}
		MessageWriter messageWriter = AmongUsClient.Instance.StartRpc(NetId, 1);
		messageWriter.WritePacked(id);
		messageWriter.EndMessage();
	}

	public void RpcExitAllVents()
	{
		if (AmongUsClient.Instance.AmClient)
		{
			ExitAllVents();
		}
		AmongUsClient.Instance.SendRpc(NetId, 2);
	}

	public override void HandleRpc(byte callId, MessageReader reader)
	{
		switch ((RpcCalls)callId)
		{
		case RpcCalls.EnterVent:
			EnterVent(reader.ReadPackedInt32(), new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0f));
			break;
		case RpcCalls.ExitVent:
			ExitVent(reader.ReadPackedInt32());
			break;
		case RpcCalls.ExitAllVents:
			ExitAllVents();
			break;
		}
	}
}
