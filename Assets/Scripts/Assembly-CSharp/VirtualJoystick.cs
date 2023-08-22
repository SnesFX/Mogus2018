using UnityEngine;

public class VirtualJoystick : MonoBehaviour, IVirtualJoystick
{
	public float InnerRadius = 0.64f;

	public float OuterRadius = 1.28f;

	public CircleCollider2D Outer;

	public SpriteRenderer Inner;

	private Controller myController = new Controller();

	public Vector2 Delta { get; private set; }

	protected virtual void FixedUpdate()
	{
		myController.Update();
		switch (myController.CheckDrag(Outer))
		{
		case DragState.TouchStart:
		case DragState.Dragging:
		{
			float maxLength = OuterRadius - InnerRadius;
			Vector2 vector = myController.DragPosition - (Vector2)base.transform.position;
			float magnitude = vector.magnitude;
			Vector2 vector2 = new Vector2(Mathf.Sqrt(Mathf.Abs(vector.x)) * Mathf.Sign(vector.x), Mathf.Sqrt(Mathf.Abs(vector.y)) * Mathf.Sign(vector.y));
			Delta = Vector2.ClampMagnitude(vector2 / OuterRadius, 1f);
			Inner.transform.localPosition = Vector3.ClampMagnitude(vector, maxLength) + Vector3.back;
			break;
		}
		case DragState.Released:
			Delta = Vector2.zero;
			Inner.transform.localPosition = Vector3.back;
			break;
		}
	}

	public virtual void UpdateJoystick(FingerBehaviour finger, Vector2 velocity, bool syncFinger)
	{
		Vector3 localPosition = Inner.transform.localPosition;
		Vector3 vector = velocity.normalized * InnerRadius;
		vector.z = localPosition.z;
		if (syncFinger)
		{
			localPosition = Vector3.Lerp(localPosition, vector, Time.fixedDeltaTime * 5f);
			Inner.transform.localPosition = localPosition;
			localPosition = Inner.transform.position;
			localPosition.z = -26f;
			finger.transform.position = localPosition;
		}
		else if (Inner.gameObject != finger.gameObject)
		{
			Inner.transform.localPosition = vector;
		}
	}
}
