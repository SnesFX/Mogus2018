using System;
using Hazel;
using PowerTools;
using UnityEngine;

public class Doorway : MonoBehaviour
{
	private const byte DoorOpenFlag = 1;

	private const float ClosedDuration = 10f;

	public const float CooldownDuration = 30f;

	public SystemTypes Room;

	public bool Open;

	public Collider2D myCollider;

	public SpriteRenderer door;

	public AnimationClip OpenDoorAnim;

	public AnimationClip CloseDoorAnim;

	public float ClosedTimer;

	public float CooldownTimer;

	public bool DoUpdate(float dt)
	{
		CooldownTimer = Math.Max(CooldownTimer - dt, 0f);
		if (ClosedTimer > 0f)
		{
			ClosedTimer = Math.Max(ClosedTimer - dt, 0f);
			if (ClosedTimer == 0f)
			{
				SetDoorway(true);
				return true;
			}
		}
		return false;
	}

	public void SetDoorway(bool open)
	{
		if (!open)
		{
			ClosedTimer = 10f;
			CooldownTimer = 30f;
		}
		if (Open != open && (bool)door)
		{
			SpriteAnim component = GetComponent<SpriteAnim>();
			if ((bool)component)
			{
				component.Play((!open) ? CloseDoorAnim : OpenDoorAnim);
			}
		}
		Open = open;
		myCollider.isTrigger = open;
	}

	public void Serialize(MessageWriter writer)
	{
		byte value = (byte)(Open ? 1 : 0);
		writer.Write(value);
	}

	public void Deserialize(MessageReader reader)
	{
		SetDoorway(reader.ReadByte() != 0);
	}
}
