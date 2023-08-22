using System.Collections;
using PowerTools;
using UnityEngine;

public class KillAnimation : MonoBehaviour
{
	public AnimationClip BlurAnim;

	public DeadBody bodyPrefab;

	public Vector3 BodyOffset;

	public IEnumerator CoPerformKill(int killIdx, PlayerControl source, PlayerControl target)
	{
		bool isParticipant = PlayerControl.LocalPlayer == source || PlayerControl.LocalPlayer == target;
		PlayerPhysics sourcePhys = source.GetComponent<PlayerPhysics>();
		SetMovement(source, false);
		SetMovement(target, false);
		if (isParticipant)
		{
			Camera.main.GetComponent<FollowerCamera>().Locked = true;
		}
		SpriteAnim sourceAnim = source.GetComponent<SpriteAnim>();
		yield return new WaitForAnimationFinish(sourceAnim, BlurAnim);
		source.NetTransform.SnapTo(target.transform.position);
		sourceAnim.Play(sourcePhys.IdleAnim);
		SetMovement(source, true);
		DeadBody deadBody = Object.Instantiate(bodyPrefab);
		Vector3 bodyPos = target.transform.position + BodyOffset;
		bodyPos.z = bodyPos.y / 1000f;
		deadBody.transform.position = bodyPos;
		deadBody.parent = target;
		target.SetPlayerMaterialColors(deadBody.GetComponent<Renderer>());
		target.Die(DeathReason.Kill);
		SetMovement(target, true);
		if (isParticipant)
		{
			Camera.main.GetComponent<FollowerCamera>().Locked = false;
		}
	}

	private static void SetMovement(PlayerControl source, bool canMove)
	{
		source.canMove = canMove;
		source.NetTransform.enabled = canMove;
		source.GetComponent<PlayerPhysics>().enabled = canMove;
		source.NetTransform.Halt();
	}
}
