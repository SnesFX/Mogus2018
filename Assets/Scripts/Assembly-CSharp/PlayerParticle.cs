using UnityEngine;

public class PlayerParticle : PoolableBehavior
{
	public SpriteRenderer myRend;

	public float maxDistance = 6f;

	public Vector2 velocity;

	public float angularVelocity;

	public void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		if (localPosition.sqrMagnitude > maxDistance * maxDistance)
		{
			OwnerPool.Reclaim(this);
			return;
		}
		localPosition += (Vector3)(velocity * Time.deltaTime);
		base.transform.localPosition = localPosition;
		base.transform.Rotate(0f, 0f, Time.deltaTime * angularVelocity);
	}
}
