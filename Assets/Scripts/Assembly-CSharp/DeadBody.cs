using UnityEngine;

public class DeadBody : MonoBehaviour
{
	public bool Reported;

	public short KillIdx;

	public PlayerControl parent;

	public Collider2D myCollider;

	public Vector2 TruePosition
	{
		get
		{
			return base.transform.position + (Vector3)myCollider.offset;
		}
	}

	public void OnClick()
	{
		if (!Reported)
		{
			Vector2 truePosition = PlayerControl.LocalPlayer.GetTruePosition();
			if (!PhysicsHelpers.AnythingBetween(truePosition, TruePosition, Constants.ShipAndObjectsMask, false))
			{
				Reported = true;
				PlayerControl.LocalPlayer.CmdReportDeadBody(parent);
			}
		}
	}
}
