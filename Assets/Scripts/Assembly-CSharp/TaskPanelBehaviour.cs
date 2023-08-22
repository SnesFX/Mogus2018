using UnityEngine;

public class TaskPanelBehaviour : MonoBehaviour
{
	public Vector3 OpenPosition;

	public Vector3 ClosedPosition;

	public SpriteRenderer background;

	public SpriteRenderer tab;

	public TextRenderer TaskText;

	public bool open;

	private float timer;

	public float Duration;

	private void Update()
	{
		background.transform.localScale = new Vector3(TaskText.Width + 0.2f, TaskText.Height + 0.2f, 1f);
		Vector3 extents = background.sprite.bounds.extents;
		extents.y = 0f - extents.y;
		extents = extents.Mul(background.transform.localScale);
		background.transform.localPosition = extents;
		Vector3 extents2 = tab.sprite.bounds.extents;
		extents2 = extents2.Mul(tab.transform.localScale);
		extents2.y = 0f - extents2.y;
		extents2.x += extents.x * 2f;
		tab.transform.localPosition = extents2;
		ClosedPosition.y = (OpenPosition.y = 0.6f);
		ClosedPosition.x = (0f - background.sprite.bounds.size.x) * background.transform.localScale.x;
		if (open)
		{
			timer = Mathf.Min(1f, timer + Time.deltaTime / Duration);
		}
		else
		{
			timer = Mathf.Max(0f, timer - Time.deltaTime / Duration);
		}
		Vector3 relativePos = new Vector3(Mathf.SmoothStep(ClosedPosition.x, OpenPosition.x, timer), Mathf.SmoothStep(ClosedPosition.y, OpenPosition.y, timer), OpenPosition.z);
		base.transform.localPosition = AspectPosition.ComputePosition(AspectPosition.EdgeAlignments.LeftTop, relativePos);
	}

	public void ToggleOpen()
	{
		open = !open;
	}
}
