using UnityEngine;

public class GameSettingMenu : MonoBehaviour
{
	public Transform[] AllItems;

	public float YStart;

	public float YOffset;

	private void OnEnable()
	{
		int num = 0;
		for (int i = 0; i < AllItems.Length; i++)
		{
			Transform transform = AllItems[i];
			if (transform.gameObject.activeSelf)
			{
				Vector3 localPosition = transform.localPosition;
				localPosition.y = YStart - (float)num * YOffset;
				transform.localPosition = localPosition;
				num++;
			}
		}
		Scroller component = GetComponent<Scroller>();
		component.YBounds.max = (float)num * YOffset / 2f + 0.1f;
	}
}
