using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CounterArea : MonoBehaviour
{
	public SystemTypes RoomType;

	public ObjectPoolBehavior pool;

	private Collider2D[] buffer = new Collider2D[10];

	private ContactFilter2D filter = default(ContactFilter2D);

	private List<PoolableBehavior> myIcons = new List<PoolableBehavior>();

	public float XOffset;

	public float YOffset;

	public int MaxWidth = 5;

	public void OnEnable()
	{
		StartCoroutine(Run());
	}

	private IEnumerator Run()
	{
		ShipStatus ship = ShipStatus.Instance;
		ShipRoom room = ship.AllRooms.First((ShipRoom r) => r.RoomId == RoomType);
		filter.useLayerMask = true;
		filter.layerMask = Constants.PlayersOnlyMask;
		WaitForSeconds wait = new WaitForSeconds(0.1f);
		Collider2D myCollider = room.roomArea;
		while (true)
		{
			bool changed = false;
			int fullCnt = myCollider.OverlapCollider(filter, buffer);
			int cnt = fullCnt;
			for (int i = 0; i < fullCnt; i++)
			{
				Collider2D collider2D = buffer[i];
				PlayerControl component = collider2D.GetComponent<PlayerControl>();
				if ((bool)component && component.IsDead)
				{
					cnt--;
				}
			}
			while (myIcons.Count < cnt)
			{
				PoolableBehavior item = pool.Get<PoolableBehavior>();
				myIcons.Add(item);
				changed = true;
			}
			while (myIcons.Count > cnt)
			{
				PoolableBehavior poolableBehavior = myIcons[myIcons.Count - 1];
				myIcons.RemoveAt(myIcons.Count - 1);
				poolableBehavior.OwnerPool.Reclaim(poolableBehavior);
				changed = true;
			}
			if (changed)
			{
				for (int j = 0; j < myIcons.Count; j++)
				{
					int num = j % 5;
					int num2 = j / 5;
					float num3 = (float)(Mathf.Min(cnt - num2 * 5, 5) - 1) * XOffset / -2f;
					myIcons[j].transform.position = base.transform.position + new Vector3(num3 + (float)num * XOffset, (float)num2 * YOffset, -1f);
				}
			}
			yield return wait;
		}
	}
}
